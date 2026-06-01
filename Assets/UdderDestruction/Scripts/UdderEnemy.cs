using UnityEngine;
using System.Collections;

namespace UdderDestruction
{
    public sealed class UdderEnemy : MonoBehaviour
    {
        public float maxHealth = 10f;
        public float speed = 1.8f;
        public float contactDamage = 8f;
        public int creamValue = 1;
        public bool avoidsWater;
        public bool isFlying;
        public Sprite downSprite;
        public Sprite sideSprite;
        public Sprite upSprite;
        public Sprite rawMilkFlySprite;
        public Sprite cottonDeathSprite;
        public Sprite skullDeathSprite;

        private UdderGameController game;
        private UdderPlayer player;
        private float health;
        private float acidTimer;
        private float poisonTimer;
        private float poisonTick;
        private float lactoseTimer;
        private float rawMilkTimer;
        private float rawMilkHoldTimer;
        private float rawMilkTick;
        private float rawMilkSpreadCooldown;
        private float condensedMilkTimer;
        private float condensedMilkTick;
        private float condensedMilkDamagePerSecond;
        private MilkMode condensedMilkMode = MilkMode.WholeMilk;
        private float prionTimer;
        private float prionAttackTimer;
        private float prionSpreadChance;
        private float meleeTimer;
        private float slideTimer;
        private float slipTextCooldown;
        private float slideMultiplier = 1f;
        private Vector2 lastDirection = Vector2.down;
        private Vector2 slideDirection = Vector2.down;
        private Vector3 baseScale;
        private Color baseColor = Color.white;
        private SpriteRenderer spriteRenderer;
        private readonly SpriteRenderer[] rawMilkFlyRenderers = new SpriteRenderer[5];
        private readonly Vector2[] rawMilkFlySeeds = new Vector2[5];
        private bool dying;

        public bool IsAlive => health > 0f;
        public bool IsSliding => slideTimer > 0f;
        public bool IsRawMilkContagious => rawMilkTimer > 0f;
        public bool IsPrionInfected => prionTimer > 0f;
        public bool IsBoss { get; set; }

        public void Init(UdderGameController owner, UdderPlayer target, float healthScale, float speedScale)
        {
            game = owner;
            player = target;
            health = maxHealth * healthScale;
            speed *= speedScale;
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer)
                baseColor = spriteRenderer.color;
            baseScale = transform.localScale;
        }

        private void Update()
        {
            if (dying)
            {
                UpdateRawMilkFlies(false);
                return;
            }

            if (!player || !player.IsAlive || health <= 0f)
                return;

            meleeTimer -= Time.deltaTime;
            rawMilkSpreadCooldown -= Time.deltaTime;
            UpdatePrionPulse();
            if (dying)
                return;
            slipTextCooldown -= Time.deltaTime;
            UpdateCondensedMilk();
            if (health <= 0f)
                return;

            UdderEnemy prionTarget = IsPrionInfected ? game.FindNearestPrionTarget(this) : null;
            Vector3 targetPosition = player.transform.position;
            if (IsPrionInfected)
                targetPosition = prionTarget ? prionTarget.transform.position : transform.position;
            Vector2 delta = targetPosition - transform.position;
            Vector2 facingDirection = delta;
            if (delta.sqrMagnitude > 0.01f)
            {
                Vector2 direction;
                float currentSpeed = rawMilkHoldTimer > 0f ? 0f : speed;
                if (slideTimer > 0f)
                {
                    slideTimer -= Time.deltaTime;
                    direction = slideDirection;
                    currentSpeed *= slideMultiplier;
                }
                else
                {
                    direction = delta.normalized;
                    if (avoidsWater && game.TryGetWaterAvoidance(transform.position, direction, out Vector2 avoidanceDirection))
                        direction = avoidanceDirection;
                    if (UdderHazardPool.TryGetRepulsion(transform.position, direction, out Vector2 puddleRepulsion))
                        direction = puddleRepulsion;
                    lastDirection = direction;
                }

                Vector3 nextPosition = transform.position + (Vector3)(direction * currentSpeed * Time.deltaTime);
                if (game && game.IsInWater(nextPosition, 0.35f))
                {
                    if (game.TryGetWaterAvoidance(transform.position, direction, out Vector2 avoidanceDirection))
                    {
                        nextPosition = transform.position + (Vector3)(avoidanceDirection * currentSpeed * Time.deltaTime);
                        direction = avoidanceDirection;
                    }
                    else
                        nextPosition = transform.position;
                }

                transform.position = nextPosition;
                facingDirection = direction;
            }

            if (IsPrionInfected && prionTarget && delta.sqrMagnitude <= 0.42f * 0.42f && prionAttackTimer <= 0f)
            {
                prionAttackTimer = 0.35f;
                prionTarget.TakePrionDamage(4f, prionSpreadChance, this);
            }

            UdderSpriteFacing.Apply(spriteRenderer, facingDirection, downSprite, sideSprite, upSprite);

            if (acidTimer > 0f)
            {
                acidTimer -= Time.deltaTime;
                transform.localScale = baseScale * (1f + Mathf.Sin(Time.time * 18f) * 0.05f);
            }
            else
            {
                transform.localScale = baseScale;
            }

            if (poisonTimer > 0f)
            {
                poisonTimer -= Time.deltaTime;
                poisonTick -= Time.deltaTime;
                if (poisonTick <= 0f)
                {
                    poisonTick = 0.45f;
                    TakeDamage(1.5f, MilkMode.SpoiledMilk, false);
                }
            }

            if (rawMilkTimer > 0f)
            {
                rawMilkTimer -= Time.deltaTime;
                rawMilkHoldTimer -= Time.deltaTime;
                rawMilkTick -= Time.deltaTime;

                if (rawMilkTick <= 0f)
                {
                    rawMilkTick = 0.55f;
                    float amount = IsBoss ? 2.2f : 2.8f;
                    health -= amount;
                    game.ShowDamageText(transform.position, amount, MilkMode.RawMilk, false);
                    if (health <= 0f)
                    {
                        Die();
                        return;
                    }
                }

                if (spriteRenderer)
                    spriteRenderer.color = Color.Lerp(baseColor, new Color(1f, 0.97f, 0.68f), 0.55f + Mathf.Sin(Time.time * 12f) * 0.15f);

                UpdateRawMilkFlies(true);
            }
            else if (spriteRenderer && !IsPrionInfected)
            {
                spriteRenderer.color = baseColor;
                UpdateRawMilkFlies(false);
            }

            lactoseTimer -= Time.deltaTime;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.collider.TryGetComponent(out UdderEnemy otherEnemy))
            {
                if (IsPrionInfected)
                {
                    if (prionAttackTimer <= 0f)
                    {
                        prionAttackTimer = 0.35f;
                        otherEnemy.TakePrionDamage(4f, prionSpreadChance, this);
                    }
                    return;
                }

                SpreadRawMilkTo(otherEnemy);
                return;
            }

            if (!collision.collider.TryGetComponent(out UdderPlayer cow))
                return;

            if (meleeTimer <= 0f)
            {
                meleeTimer = 0.28f;
                float stompDamage = cow.StompDamage;
                if (stompDamage > 0f)
                    TakeHornDamage(IsSliding ? stompDamage + 2f : stompDamage);
            }

            if (!IsSliding && !IsPrionInfected)
                cow.TakeDamage(contactDamage * Time.deltaTime);
        }

        private void SpreadRawMilkTo(UdderEnemy otherEnemy)
        {
            if (!otherEnemy || rawMilkSpreadCooldown > 0f)
                return;

            if (IsRawMilkContagious && !otherEnemy.IsRawMilkContagious)
            {
                rawMilkSpreadCooldown = 0.35f;
                otherEnemy.ApplyRawMilk(true);
                return;
            }

            if (!IsRawMilkContagious && otherEnemy.IsRawMilkContagious)
            {
                rawMilkSpreadCooldown = 0.35f;
                ApplyRawMilk(true);
            }
        }

        public bool StartButterSlide(float duration, float multiplier)
        {
            if (slideTimer > duration * 0.4f)
                return false;

            slideTimer = duration;
            slideMultiplier = multiplier;
            slideDirection = lastDirection.sqrMagnitude > 0.01f ? lastDirection.normalized : Vector2.down;
            if (slipTextCooldown <= 0f)
            {
                slipTextCooldown = 1f;
                game.ShowCowText(transform.position, "Whoopsie!");
            }

            return true;
        }

        public void MakeLactoseIntolerant(float duration)
        {
            bool wasTolerant = lactoseTimer <= 0f;
            lactoseTimer = Mathf.Max(lactoseTimer, duration);
            if (wasTolerant)
                game.ShowCowText(transform.position, "Lactose Intolerance!");
        }

        private void ApplyRawMilk(bool contagiousSpread)
        {
            bool wasHealthy = rawMilkTimer <= 0f;
            rawMilkTimer = Mathf.Max(rawMilkTimer, contagiousSpread ? 3.8f : 5.2f);
            rawMilkHoldTimer = Mathf.Max(rawMilkHoldTimer, contagiousSpread ? 0.65f : 1.35f);
            rawMilkTick = Mathf.Min(rawMilkTick <= 0f ? 0.55f : rawMilkTick, 0.55f);
            if (wasHealthy)
                game.ShowCowText(transform.position, "Infected!");
        }

        public void ApplyCondensedMilk(float attackDamage, int level, MilkMode sourceMode)
        {
            float duration = Mathf.Max(0.1f, level * 0.3f);
            float totalDamage = attackDamage * level * 0.1f;
            condensedMilkTimer = Mathf.Max(condensedMilkTimer, duration);
            condensedMilkDamagePerSecond = Mathf.Max(condensedMilkDamagePerSecond, totalDamage / duration);
            condensedMilkMode = sourceMode;
            condensedMilkTick = Mathf.Min(condensedMilkTick <= 0f ? 0.3f : condensedMilkTick, 0.3f);
        }

        public void ApplyPrionPulse(float duration, float spreadChance)
        {
            if (health <= 0f || IsBoss)
                return;

            bool wasInfected = prionTimer > 0f;
            prionTimer = Mathf.Max(prionTimer, duration);
            prionSpreadChance = Mathf.Max(prionSpreadChance, spreadChance);
            prionAttackTimer = Mathf.Min(prionAttackTimer, 0.2f);
            if (spriteRenderer)
                spriteRenderer.color = new Color(1f, 0.96f, 0.35f);
            if (!wasInfected)
                game.ShowCowText(transform.position, "Prion!");
        }

        public bool TakePrionDamage(float amount, float spreadChance, UdderEnemy source)
        {
            if (health <= 0f)
                return false;

            health -= amount;
            game.ShowDamageText(transform.position, amount, MilkMode.Prion, false);
            if (health > 0f)
                return false;

            game.TrySpreadPrionFrom(source, transform.position, spreadChance);
            Die();
            return true;
        }

        private void UpdatePrionPulse()
        {
            if (prionTimer <= 0f)
                return;

            prionTimer -= Time.deltaTime;
            prionAttackTimer -= Time.deltaTime;
            if (spriteRenderer && rawMilkTimer <= 0f)
                spriteRenderer.color = Color.Lerp(baseColor, new Color(1f, 0.96f, 0.35f), 0.72f + Mathf.Sin(Time.time * 16f) * 0.12f);

            if (prionTimer <= 0f)
                Die();
        }

        private void UpdateCondensedMilk()
        {
            if (condensedMilkTimer <= 0f)
                return;

            condensedMilkTimer -= Time.deltaTime;
            condensedMilkTick -= Time.deltaTime;
            if (condensedMilkTick > 0f)
                return;

            condensedMilkTick = 0.3f;
            float amount = condensedMilkDamagePerSecond * 0.3f;
            health -= amount;
            game.ShowDamageText(transform.position, amount, condensedMilkMode, false);
            if (health <= 0f)
                Die();
        }

        private void TakeHornDamage(float amount)
        {
            if (health <= 0f)
                return;

            health -= amount;
            game.ShowDamageText(transform.position, amount, MilkMode.Stomp, false);

            if (health <= 0f)
            {
                Die();
                return;
            }

            // Stomp is contact damage, so enemies should keep pressing the player instead of being knocked away.
        }

        public void TakeDamage(float amount, MilkMode mode, bool canCrit = true, int effectLevel = 1)
        {
            if (health <= 0f)
                return;

            bool crit = canCrit && Random.value < game.CritChance;
            float dairyMultiplier = lactoseTimer > 0f ? 1.65f : 1f;
            float finalAmount = amount * dairyMultiplier;
            finalAmount = crit ? finalAmount * 2.2f : finalAmount;
            health -= finalAmount;

            if (mode == MilkMode.Buttermilk)
            {
                acidTimer = 1.2f;
                StartButterSlide(GetButtermilkSlideDuration(effectLevel), 3.4f);
            }
            if (mode == MilkMode.SpoiledMilk)
                poisonTimer = 2.5f;
            if (mode == MilkMode.RawMilk)
                ApplyRawMilk(false);

            game.ShowDamageText(transform.position, finalAmount, mode, crit);
            if (health <= 0f)
                Die();
        }

        private static float GetButtermilkSlideDuration(int effectLevel)
        {
            return 1f * (1f + Mathf.Max(0, effectLevel - 1) * 0.2f);
        }

        private void Die()
        {
            if (dying)
                return;

            dying = true;
            game.RegisterEnemyDefeated(this);
            StartCoroutine(PlayDeathSequence());
        }

        private IEnumerator PlayDeathSequence()
        {
            foreach (Collider2D collider in GetComponents<Collider2D>())
                collider.enabled = false;

            if (TryGetComponent(out Rigidbody2D body))
                body.simulated = false;

            UpdateRawMilkFlies(false);

            if (spriteRenderer)
            {
                spriteRenderer.color = Color.white;
                spriteRenderer.sortingOrder = 9;
                if (cottonDeathSprite)
                    spriteRenderer.sprite = cottonDeathSprite;
            }

            yield return new WaitForSeconds(1f);

            if (spriteRenderer && skullDeathSprite)
                spriteRenderer.sprite = skullDeathSprite;

            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

        private void UpdateRawMilkFlies(bool active)
        {
            EnsureRawMilkFlies();
            for (int i = 0; i < rawMilkFlyRenderers.Length; i++)
            {
                SpriteRenderer fly = rawMilkFlyRenderers[i];
                if (!fly)
                    continue;

                fly.gameObject.SetActive(active);
                if (!active)
                    continue;

                float t = Time.time * (4.5f + i * 0.55f);
                Vector2 seed = rawMilkFlySeeds[i];
                Vector3 offset = new(
                    Mathf.Sin(t + seed.x) * 0.18f + Mathf.Sin(t * 2.1f + seed.y) * 0.06f,
                    Mathf.Cos(t * 1.3f + seed.y) * 0.18f + Mathf.Sin(t * 2.6f + seed.x) * 0.06f,
                    0f);
                fly.transform.localPosition = offset;
                fly.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 3f) * 18f);
            }
        }

        private void EnsureRawMilkFlies()
        {
            if (!rawMilkFlySprite || rawMilkFlyRenderers[0])
                return;

            int sortingOrder = spriteRenderer ? spriteRenderer.sortingOrder + 2 : 6;
            for (int i = 0; i < rawMilkFlyRenderers.Length; i++)
            {
                GameObject flyObject = new("Raw Milk Fly");
                flyObject.transform.SetParent(transform, false);
                flyObject.transform.localScale = Vector3.one * 0.25f;
                var flyRenderer = flyObject.AddComponent<SpriteRenderer>();
                flyRenderer.sprite = rawMilkFlySprite;
                flyRenderer.sortingOrder = sortingOrder;
                flyRenderer.gameObject.SetActive(false);
                rawMilkFlyRenderers[i] = flyRenderer;
                rawMilkFlySeeds[i] = Random.insideUnitCircle * 9f;
            }
        }
    }
}
