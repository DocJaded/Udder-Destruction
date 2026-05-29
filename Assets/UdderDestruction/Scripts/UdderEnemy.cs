using UnityEngine;

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
        private float meleeTimer;
        private float slideTimer;
        private float slideMultiplier = 1f;
        private Vector2 lastDirection = Vector2.down;
        private Vector2 slideDirection = Vector2.down;
        private Vector3 baseScale;
        private Color baseColor = Color.white;
        private SpriteRenderer spriteRenderer;

        public bool IsAlive => health > 0f;
        public bool IsSliding => slideTimer > 0f;
        public bool IsRawMilkContagious => rawMilkTimer > 0f;
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
            if (!player || !player.IsAlive || health <= 0f)
                return;

            meleeTimer -= Time.deltaTime;
            rawMilkSpreadCooldown -= Time.deltaTime;
            Vector2 delta = player.transform.position - transform.position;
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
            }
            else if (spriteRenderer)
            {
                spriteRenderer.color = baseColor;
            }

            lactoseTimer -= Time.deltaTime;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.collider.TryGetComponent(out UdderEnemy otherEnemy))
            {
                SpreadRawMilkTo(otherEnemy);
                return;
            }

            if (!collision.collider.TryGetComponent(out UdderPlayer cow))
                return;

            if (meleeTimer <= 0f)
            {
                meleeTimer = 0.28f;
                TakeHornDamage(IsSliding ? 7.5f : 5.5f);
                game.ShowCowText(cow.transform.position, IsSliding ? "OLE!" : IsBoss ? "GORE!" : "HORNS!");
            }

            if (!IsSliding)
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

        public void StartButterSlide(float duration, float multiplier)
        {
            if (slideTimer > duration * 0.4f)
                return;

            slideTimer = duration;
            slideMultiplier = multiplier;
            slideDirection = lastDirection.sqrMagnitude > 0.01f ? lastDirection.normalized : Vector2.down;
        }

        public void MakeLactoseIntolerant(float duration)
        {
            lactoseTimer = Mathf.Max(lactoseTimer, duration);
        }

        private void ApplyRawMilk(bool contagiousSpread)
        {
            rawMilkTimer = Mathf.Max(rawMilkTimer, contagiousSpread ? 3.8f : 5.2f);
            rawMilkHoldTimer = Mathf.Max(rawMilkHoldTimer, contagiousSpread ? 0.65f : 1.35f);
            rawMilkTick = Mathf.Min(rawMilkTick <= 0f ? 0.55f : rawMilkTick, 0.55f);
            if (contagiousSpread)
                game.ShowCowText(transform.position, "INFECTED!");
            else
                game.ShowCowText(transform.position, "RAW MILK!");
        }

        private void TakeHornDamage(float amount)
        {
            if (health <= 0f)
                return;

            health -= amount;
            game.ShowDamageText(transform.position, amount, MilkMode.WholeMilk, false);

            if (health <= 0f)
            {
                Die();
                return;
            }

            if (!IsBoss)
                game.KnockEnemyBack(this);
        }

        public void TakeDamage(float amount, MilkMode mode, bool canCrit = true)
        {
            if (health <= 0f)
                return;

            bool crit = canCrit && Random.value < game.CritChance;
            float dairyMultiplier = lactoseTimer > 0f ? 1.65f : 1f;
            float finalAmount = amount * dairyMultiplier;
            finalAmount = crit ? finalAmount * 2.2f : finalAmount;
            health -= finalAmount;

            if (mode == MilkMode.Buttermilk)
                acidTimer = 1.2f;
            if (mode == MilkMode.SpoiledMilk)
                poisonTimer = 2.5f;
            if (mode == MilkMode.RawMilk)
                ApplyRawMilk(false);

            game.ShowDamageText(transform.position, finalAmount, mode, crit);
            if (lactoseTimer > 0f && Random.value < 0.2f)
                game.ShowCowText(transform.position, "LACTOSE!");

            if (health <= 0f)
                Die();
        }

        private void Die()
        {
            game.RegisterEnemyDefeated(this);
            Destroy(gameObject);
        }
    }
}
