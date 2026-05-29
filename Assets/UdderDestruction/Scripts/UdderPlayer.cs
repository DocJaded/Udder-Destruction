using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UdderDestruction
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class UdderPlayer : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float maxBovinity = 5f;
        public float maxMilk = 100f;
        public float moveSpeed = 4.8f;
        public float fireInterval = 0.38f;
        public float projectileDamage = 6f;
        public float butterInterval = 5.5f;
        public float dairyAirInterval = 8.5f;
        public int cheeseSaves;
        public MilkMode milkMode;
        public Sprite downSprite;
        public Sprite sideSprite;
        public Sprite upSprite;

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private UdderGameController game;
        private float health;
        private float bovinity;
        private float milk;
        private float fireTimer;
        private float butterTimer;
        private float dairyAirTimer;
        private Vector2 aim = Vector2.right;
        private float invulnerableTimer;

        public bool IsAlive => health > 0f;
        public float Health01 => health / maxHealth;
        public float Bovinity01 => bovinity / maxBovinity;
        public float Milk01 => milk / maxMilk;

        public void Init(UdderGameController owner)
        {
            game = owner;
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            health = maxHealth;
            bovinity = 0f;
            milk = maxMilk;
            butterTimer = 2.5f;
            dairyAirTimer = 4.5f;
        }

        private void Update()
        {
            Vector2 move = ReadMove();
            if (move.sqrMagnitude > 1f)
                move.Normalize();

            if (move.sqrMagnitude > 0.01f)
                aim = move.normalized;

            if (game)
                aim = game.GetAutoAimDirection(transform.position, aim);

            if (game)
                move = game.GetWaterBlockedMove(transform.position, move);

            body.linearVelocity = move * moveSpeed;

            Vector2 facing = move.sqrMagnitude > 0.01f ? move : aim;
            UdderSpriteFacing.Apply(spriteRenderer, facing, downSprite, sideSprite, upSprite);

            milk = Mathf.Min(maxMilk, milk + 13f * Time.deltaTime);
            invulnerableTimer -= Time.deltaTime;
            fireTimer -= Time.deltaTime;
            butterTimer -= Time.deltaTime;
            dairyAirTimer -= Time.deltaTime;

            if (fireTimer <= 0f && milk >= MilkCost())
            {
                fireTimer = fireInterval;
                milk -= MilkCost();
                game.FireMilk(transform.position, aim, milkMode, projectileDamage);
            }

            if (butterTimer <= 0f && milk >= 18f)
            {
                butterTimer = butterInterval;
                milk -= 18f;
                game.DeployButter(transform.position);
            }

            if (dairyAirTimer <= 0f && milk >= 28f)
            {
                dairyAirTimer = dairyAirInterval;
                milk -= 28f;
                game.DeployDairyAir(transform.position);
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

        private float MilkCost()
        {
            return milkMode switch
            {
                MilkMode.Buttermilk => 9f,
                MilkMode.SpoiledMilk => 12f,
                MilkMode.RawMilk => 14f,
                _ => 6f,
            };
        }

        public void TakeDamage(float amount)
        {
            if (invulnerableTimer > 0f || health <= 0f)
                return;

            health -= amount;
            game.ShowCowText(transform.position, "OW!");

            if (health <= 0f)
            {
                if (cheeseSaves > 0)
                {
                    cheeseSaves--;
                    health = maxHealth * 0.45f;
                    invulnerableTimer = 2.2f;
                    game.ShowCowText(transform.position, "CHEESED IT!");
                }
                else
                {
                    body.linearVelocity = Vector2.zero;
                    game.GameOver();
                }
            }
        }

        public void Collect(UdderPickup pickup)
        {
            switch (pickup.type)
            {
                case PickupType.Cream:
                    game.AddCream(pickup.amount);
                    game.ShowCowText(transform.position, "+CREAM");
                    break;
                case PickupType.Cheese:
                    cheeseSaves += pickup.amount;
                    game.ShowCowText(transform.position, "CHEESE SAVE");
                    break;
                case PickupType.Buttermilk:
                    milkMode = MilkMode.Buttermilk;
                    game.ShowCowText(transform.position, "BUTTERMILK");
                    break;
                case PickupType.SpoiledMilk:
                    milkMode = MilkMode.SpoiledMilk;
                    game.ShowCowText(transform.position, "SPOILED MILK");
                    break;
                case PickupType.RawMilk:
                    milkMode = MilkMode.RawMilk;
                    game.ShowCowText(transform.position, "RAW MILK");
                    break;
                case PickupType.Heal:
                    health = Mathf.Min(maxHealth, health + 24f);
                    game.ShowCowText(transform.position, "MOOCHAS GRACIAS");
                    break;
            }
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
