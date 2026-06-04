using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UdderDestruction
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class UdderPlayer : MonoBehaviour
    {
        private const int MaxPowerLevel = 10;
        private const float BaseBovinityMax = 5f;

        public float maxHealth = 100f;
        public float maxBovinity = 5f;
        public float maxMilk = 100f;
        public float moveSpeed = 4.8f;
        public float fireInterval = 0.38f;
        public float projectileDamage = 6f;
        public float butterInterval = 5.5f;
        public float dairyAirInterval = 8.5f;
        public Sprite downSprite;
        public Sprite sideSprite;
        public Sprite upSprite;

        private readonly int[] powerLevels = new int[System.Enum.GetValues(typeof(UdderPower)).Length];
        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private UdderGameController game;
        private float baseMaxHealth;
        private float baseMoveSpeed;
        private float stompDamage;
        private float health;
        private float bovinity;
        private float milk;
        private float wholeMilkTimer;
        private float buttermilkTimer;
        private float spoiledMilkTimer;
        private float rawMilkTimer;
        private float butterTimer;
        private float dairyAirTimer;
        private float prionPulseTimer;
        private float maillardRegenTimer;
        private float drowningTickTimer;
        private float drowningTextTimer;
        private Vector2 aim = Vector2.right;
        private Vector2 visualFacing = Vector2.right;
        private float invulnerableTimer;

        public bool IsAlive => health > 0f;
        public float Health01 => health / maxHealth;
        public float Bovinity => bovinity;
        public int BovinityLevel { get; private set; } = 1;
        public float Bovinity01 => maxBovinity <= 0f ? 0f : Mathf.Clamp01(bovinity / maxBovinity);
        public bool CanLevelBovinity => bovinity >= maxBovinity;
        public float StompDamage => GetPowerLevel(UdderPower.Stomp) > 0 ? stompDamage : 0f;
        public float CheeseItChance => 0.01f + GetPowerLevel(UdderPower.Legendary) * 0.01f;

        public void Init(UdderGameController owner)
        {
            game = owner;
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseMaxHealth = maxHealth;
            baseMoveSpeed = moveSpeed;
            stompDamage = 5.5f;
            health = maxHealth;
            maxBovinity = BaseBovinityMax;
            bovinity = 0f;
            milk = maxMilk;
            BovinityLevel = 1;
            for (int i = 0; i < powerLevels.Length; i++)
                powerLevels[i] = 0;
            powerLevels[(int)UdderPower.Stomp] = 1;
            wholeMilkTimer = 0.2f;
            buttermilkTimer = 1.1f;
            spoiledMilkTimer = 1.8f;
            rawMilkTimer = 2.4f;
            butterTimer = 2.5f;
            dairyAirTimer = 4.5f;
            prionPulseTimer = 6f;
            maillardRegenTimer = GetMaillardRegenInterval();
        }

        private void Update()
        {
            Vector2 move = ReadMove();
            if (move.sqrMagnitude > 1f)
                move.Normalize();

            if (move.sqrMagnitude > 0.01f)
            {
                aim = move.normalized;
                visualFacing = aim;
            }

            Vector2 desiredVelocity = move * GetCurrentMoveSpeed();
            bool drowning = game && game.IsInWater(transform.position, 0f);
            if (drowning)
                desiredVelocity *= 0.5f;
            if (game && desiredVelocity.sqrMagnitude > 0.0001f)
            {
                Vector2 desiredMove = desiredVelocity * Time.deltaTime;
                Vector2 constrainedMove = game.GetArenaConstrainedMove(transform.position, desiredMove);
                desiredVelocity = Time.deltaTime > 0f ? constrainedMove / Time.deltaTime : Vector2.zero;
            }

            body.linearVelocity = desiredVelocity;

            UdderSpriteFacing.Apply(spriteRenderer, visualFacing, downSprite, sideSprite, upSprite, true);

            milk = Mathf.Min(maxMilk, milk + 24f * Time.deltaTime);
            invulnerableTimer -= Time.deltaTime;
            wholeMilkTimer -= Time.deltaTime;
            buttermilkTimer -= Time.deltaTime;
            spoiledMilkTimer -= Time.deltaTime;
            rawMilkTimer -= Time.deltaTime;
            butterTimer -= Time.deltaTime;
            dairyAirTimer -= Time.deltaTime;
            UpdateDrowning(drowning);
            UpdateMaillardReaction();

            TryFireMilkPower(UdderPower.WholeMilk, MilkMode.WholeMilk, ref wholeMilkTimer, fireInterval, 6f, 0.5f);
            TryFireMilkPower(UdderPower.Buttermilk, MilkMode.Buttermilk, ref buttermilkTimer, 1f, 9f, 1.1f);
            TryFireMilkPower(UdderPower.SpoiledMilk, MilkMode.SpoiledMilk, ref spoiledMilkTimer, 5f, 12f, 1f);
            TryFireMilkPower(UdderPower.RawMilk, MilkMode.RawMilk, ref rawMilkTimer, 1f, 14f, 0.85f);
            UpdateAuraFarming();
            UpdatePrionPulse();

            if (GetPowerLevel(UdderPower.Butter) > 0 && butterTimer <= 0f)
            {
                int butterLevel = GetPowerLevel(UdderPower.Butter);
                butterTimer = butterInterval;
                SpendMilk(18f);
                game.DeployButter(transform.position, butterLevel);
            }

            if (GetPowerLevel(UdderPower.DairyAir) > 0 && dairyAirTimer <= 0f)
            {
                int dairyAirLevel = GetPowerLevel(UdderPower.DairyAir);
                dairyAirTimer = dairyAirInterval;
                SpendMilk(28f);
                game.DeployDairyAir(transform.position, dairyAirLevel, dairyAirInterval);
            }
        }

        private void UpdateDrowning(bool drowning)
        {
            if (!drowning)
            {
                drowningTickTimer = 0f;
                drowningTextTimer = 0f;
                return;
            }

            drowningTickTimer -= Time.deltaTime;
            drowningTextTimer -= Time.deltaTime;
            if (drowningTextTimer <= 0f)
            {
                drowningTextTimer = 0.85f;
                game?.ShowDrowningText(transform.position);
            }

            if (drowningTickTimer > 0f)
                return;

            drowningTickTimer = 1f;
            TakeDrowningDamage(maxHealth * 0.15f);
        }

        private void TakeDrowningDamage(float amount)
        {
            if (health <= 0f)
                return;

            health -= amount;
            if (health <= 0f)
            {
                if (Random.value < CheeseItChance)
                {
                    health = maxHealth;
                    invulnerableTimer = 2.2f;
                    game.ShowCheesedItText(transform);
                }
                else
                {
                    body.linearVelocity = Vector2.zero;
                    game.GameOver();
                }
            }
        }

        private static Vector2 ReadMove()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                Vector2 move = Vector2.zero;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) move.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) move.x += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) move.y -= 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) move.y += 1f;
                return move;
            }
#endif
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        private void TryFireMilkPower(UdderPower power, MilkMode mode, ref float timer, float interval, float cost, float damageMultiplier)
        {
            int level = GetPowerLevel(power);
            if (level <= 0 || timer > 0f)
                return;

            if (!game || !game.TryGetAutoAimDirectionInRange(transform.position, aim, game.GetMilkShotRange(mode), mode == MilkMode.SpoiledMilk, out Vector2 fireDirection))
                return;

            timer = interval;
            SpendMilk(cost);
            game.FireMilk(transform.position, fireDirection, mode, projectileDamage * damageMultiplier * (1f + (level - 1) * 0.12f), level, GetPowerLevel(UdderPower.CondensedMilk));
        }

        private void SpendMilk(float cost)
        {
            milk = Mathf.Max(0f, milk - cost);
        }

        public void TakeDamage(float amount)
        {
            if (invulnerableTimer > 0f || health <= 0f)
                return;

            health -= amount * (1f - GetDamageResistance());

            if (health <= 0f)
            {
                if (Random.value < CheeseItChance)
                {
                    health = maxHealth;
                    invulnerableTimer = 2.2f;
                    game.ShowCheesedItText(transform);
                }
                else
                {
                    body.linearVelocity = Vector2.zero;
                    game.GameOver();
                }
            }
        }

        public void RestoreHealthToFull()
        {
            health = maxHealth;
        }

        public void FillBovinity()
        {
            bovinity = maxBovinity;
        }

        public void Collect(UdderPickup pickup)
        {
            switch (pickup.type)
            {
                case PickupType.Cream:
                    game.AddCream(pickup.amount);
                    game.ShowCowText(transform.position, "+CREAM");
                    break;
                case PickupType.Cranberry:
                    ShowBerryHealing(0.05f);
                    break;
                case PickupType.Strawberry:
                    ShowBerryHealing(0.1f);
                    break;
                case PickupType.Raspberry:
                    ShowBerryHealing(0.2f);
                    break;
                case PickupType.Blackberries:
                    ShowBerryHealing(0.4f);
                    break;
                case PickupType.MinorMoona:
                    game.AddBovinity(pickup.amount);
                    game.ShowCowText(transform.position, "+1 BOVINITY");
                    break;
                case PickupType.NormalMoona:
                    game.AddBovinity(pickup.amount);
                    game.ShowCowText(transform.position, "+2 BOVINITY");
                    break;
                case PickupType.RemarkableMoona:
                    game.AddBovinity(pickup.amount);
                    game.ShowCowText(transform.position, "+5 BOVINITY");
                    break;
                case PickupType.ElysianMoona:
                    game.AddBovinity(pickup.amount);
                    game.ShowCowText(transform.position, "+10 BOVINITY");
                    break;
                case PickupType.DairyDouble:
                    game.BankDairyDouble();
                    game.ShowCowText(transform.position, "DAIRY DOUBLE BANKED");
                    break;
            }
        }

        public void AddBovinity(float amount)
        {
            bovinity += amount;
        }

        public void CompleteBovinityLevelUp()
        {
            bovinity = Mathf.Max(0f, bovinity - maxBovinity);
            BovinityLevel++;
            maxBovinity = BaseBovinityMax * BovinityLevel;
        }

        public int GetPowerLevel(UdderPower power)
        {
            return powerLevels[(int)power];
        }

        public bool CanGainPowerLevel(UdderPower power, int gain)
        {
            return GetPowerLevel(power) + gain <= MaxPowerLevel;
        }

        public int AddPowerLevel(UdderPower power, int gain)
        {
            int index = (int)power;
            int applied = Mathf.Min(gain, MaxPowerLevel - powerLevels[index]);
            int previousLevel = powerLevels[index];
            powerLevels[index] += applied;
            ApplyPassivePowerGain(power, previousLevel, applied);
            return applied;
        }

        public static string GetPowerLabel(UdderPower power)
        {
            return power switch
            {
                UdderPower.WholeMilk => "Whole Milk",
                UdderPower.SpoiledMilk => "Spoiled Milk",
                UdderPower.RawMilk => "Raw Milk",
                UdderPower.DairyAir => "Dairy Air",
                UdderPower.CondensedMilk => "Condensed Milk",
                UdderPower.ILikeToMooveIt => "I Like to MOOve It",
                UdderPower.Legendary => "Legen-dairy!",
                UdderPower.MoreCowbell => "More Cowbell",
                UdderPower.MaillardReaction => "Maillard Reaction",
                UdderPower.AuraFarming => "Aura Farming",
                UdderPower.PrionInfection => "Prion Infection",
                _ => power.ToString(),
            };
        }

        public static string GetPowerDescription(UdderPower power)
        {
            return power switch
            {
                UdderPower.Stomp => "Contact damage when enemies touch the cow. Leveling adds stomp damage.",
                UdderPower.WholeMilk => "Fast milk projectile. Leveling increases projectile damage.",
                UdderPower.Buttermilk => "Milk projectile that makes targets slip. Leveling increases damage and slip duration.",
                UdderPower.SpoiledMilk => "Fires spoiled milk that creates repulsing puddles, beaches dolphins, and contaminates pond tiles.",
                UdderPower.RawMilk => "Milk projectile that makes enemies Diseased with damage over time. Leveling increases projectile damage.",
                UdderPower.Butter => "Drops a slippery butter tile area. Odd levels increase duration; even levels increase size up to 4x4.",
                UdderPower.DairyAir => "Creates a cloud that makes enemies lactose intolerant for life. Leveling increases cloud uptime.",
                UdderPower.CondensedMilk => "Milk projectiles gain damage over time. Leveling increases DoT damage and duration.",
                UdderPower.ILikeToMooveIt => "Increases player movement speed by 10% of base speed per level.",
                UdderPower.Beefcake => "Increases stomp damage and max HP. Stomp damage scales from current value.",
                UdderPower.Legendary => "Increases chance to Cheese It by 1 percentage point per level.",
                UdderPower.MoreCowbell => "Adds more enemies per wave and increases all drop chances by 10% per level.",
                UdderPower.MaillardReaction => "Regenerates 10% HP over time. Leveling speeds the regeneration up to once per second.",
                UdderPower.Rawhide => "Reduces incoming damage. Starts at 10% resistance, then +1% per level.",
                UdderPower.AuraFarming => "Attracts nearby drops. Each level adds one cow-collider radius to its attraction range.",
                UdderPower.PrionInfection => "Infects enemies with prions, causing damage over time and making them attack other enemies. On death, infection has a level-scaled chance to spread within its AoE.",
                _ => "No description available.",
            };
        }

        private void ApplyPassivePowerGain(UdderPower power, int previousLevel, int applied)
        {
            if (applied <= 0)
                return;

            if (power == UdderPower.Beefcake)
            {
                for (int i = 0; i < applied; i++)
                    stompDamage *= 1.1f;

                float previousMaxHealth = maxHealth;
                maxHealth = baseMaxHealth * (1f + GetPowerLevel(UdderPower.Beefcake) * 0.1f);
                health += maxHealth - previousMaxHealth;
            }
            else if (power == UdderPower.Stomp)
            {
                stompDamage += applied;
            }
            else if (power == UdderPower.MaillardReaction && previousLevel <= 0)
            {
                maillardRegenTimer = GetMaillardRegenInterval();
            }
            else if (power == UdderPower.PrionInfection && previousLevel <= 0)
            {
                prionPulseTimer = 0.25f;
            }
        }

        private float GetCurrentMoveSpeed()
        {
            return baseMoveSpeed * (1f + GetPowerLevel(UdderPower.ILikeToMooveIt) * 0.1f);
        }

        private float GetDamageResistance()
        {
            int level = GetPowerLevel(UdderPower.Rawhide);
            return level <= 0 ? 0f : Mathf.Clamp01(0.1f + (level - 1) * 0.01f);
        }

        private void UpdateMaillardReaction()
        {
            int level = GetPowerLevel(UdderPower.MaillardReaction);
            if (level <= 0 || health <= 0f)
                return;

            maillardRegenTimer -= Time.deltaTime;
            if (maillardRegenTimer > 0f)
                return;

            maillardRegenTimer = GetMaillardRegenInterval();
            HealPercent(0.1f);
        }

        private void UpdateAuraFarming()
        {
            int level = GetPowerLevel(UdderPower.AuraFarming);
            if (level <= 0 || !game)
                return;

            float colliderRadius = 0.18f;
            if (TryGetComponent(out CircleCollider2D circle))
                colliderRadius = circle.radius * Mathf.Max(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y));
            game.AttractNearbyDrops(transform.position, colliderRadius * level);
        }

        private void UpdatePrionPulse()
        {
            int level = GetPowerLevel(UdderPower.PrionInfection);
            if (level <= 0 || !game)
                return;

            prionPulseTimer -= Time.deltaTime;
            if (prionPulseTimer > 0f)
                return;

            prionPulseTimer = 6f;
            game.TryStartPrionPulse(transform.position, level);
        }

        private float GetMaillardRegenInterval()
        {
            int level = GetPowerLevel(UdderPower.MaillardReaction);
            if (level <= 0)
                return 3f;

            return Mathf.Lerp(3f, 1f, Mathf.Clamp01((level - 1) / 9f));
        }

        private void HealPercent(float percent)
        {
            health = Mathf.Min(maxHealth, health + maxHealth * percent);
        }

        private void ShowBerryHealing(float percent)
        {
            float before = health;
            HealPercent(percent);
            game.ShowHealText(transform.position, health - before);
        }

        public void ApplyUpgrade(int choice)
        {
            switch (choice)
            {
                case 0:
                    projectileDamage += 2f;
                    game.ShowCowText(transform.position, "CALCIUM CANNON");
                    break;
                case 1:
                    fireInterval = Mathf.Max(0.16f, fireInterval * 0.82f);
                    game.ShowCowText(transform.position, "RAPID RUMINANT");
                    break;
                case 2:
                    maxHealth += 18f;
                    health += 18f;
                    game.ShowCowText(transform.position, "BEEFIER");
                    break;
            }
        }
    }
}
