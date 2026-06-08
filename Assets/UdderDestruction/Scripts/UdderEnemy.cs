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
        public Sprite skeletonDeathSprite;
        public Sprite prionAngrySprite;
        public Sprite proudEmoteSprite;
        public Sprite spoiledMilkEmoteSprite;
        public Sprite rawMilkEmoteSprite;
        public GameObject prionIndicatorPrefab;
        public UdderBossType bossType = UdderBossType.MiyamotoMoosashi;
        public UdderEnemyKind enemyKind = UdderEnemyKind.DebtChicken;
        public int rewardWave;

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
        private float sleepTimer;
        private float prionTimer;
        private float prionTick;
        private float prionDamagePerSecond;
        private float prionSpreadChance;
        private float prionAttackTimer;
        private float meleeTimer;
        private float slideTimer;
        private float butterSlowTimer;
        private float butterSlowMultiplier = 1f;
        private float slipTextCooldown;
        private float waterRouteTimer;
        private float chargeWindupTimer;
        private float chargeTimer;
        private float chargeCooldown;
        private float queuedChargeDuration;
        private float separationRefreshTimer;
        private float slideMultiplier = 1f;
        private int waterRouteSign;
        private Vector2 waterRouteCenter;
        private Vector2 lastDirection = Vector2.down;
        private Vector2 slideDirection = Vector2.down;
        private Vector2 chargeDirection;
        private Vector2 cachedSeparation;
        private Vector3 baseScale;
        private Color baseColor = Color.white;
        private SpriteRenderer spriteRenderer;
        private SpriteRenderer prionIndicatorRenderer;
        private SpriteRenderer statusEmoteRenderer;
        private Transform bossHealthBarRoot;
        private Transform bossHealthFill;
        private readonly SpriteRenderer[] rawMilkFlyRenderers = new SpriteRenderer[5];
        private readonly Vector2[] rawMilkFlySeeds = new Vector2[5];
        private bool dying;
        private bool ruminatorRespawned;
        private bool proudEmoteLocked;
        private static Sprite healthBarSprite;

        public bool IsAlive => health > 0f;
        public bool IsSliding => slideTimer > 0f;
        public bool IsRawMilkContagious => rawMilkTimer > 0f && !IsBoss;
        public bool IsPrionInfected => prionTimer > 0f;
        public bool IsSleeping => sleepTimer > 0f;
        public bool IsBee => enemyKind == UdderEnemyKind.Bee || bossType == UdderBossType.Beeatrice;
        public bool IsBoss { get; set; }
        public bool IsInvulnerable { get; set; }
        public bool UsesCustomMovement { get; set; }

        public void Init(UdderGameController owner, UdderPlayer target, float healthScale, float speedScale)
        {
            game = owner;
            player = target;
            maxHealth *= healthScale;
            health = maxHealth;
            speed *= speedScale;
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer)
                baseColor = spriteRenderer.color;
            baseScale = transform.localScale;
            UpdateBossHealthBar();
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
            butterSlowTimer -= Time.deltaTime;
            UpdatePrionPulse();
            UpdatePrionIndicator();
            UpdateBossHealthBar();
            UpdateBossBehavior();
            if (dying)
                return;
            slipTextCooldown -= Time.deltaTime;
            UpdateCondensedMilk();
            UpdateStatusEmote();
            if (health <= 0f)
                return;
            if (sleepTimer > 0f)
                sleepTimer -= Time.deltaTime;

            Transform milkshakeTarget = game.GetActiveMilkshakeTarget();
            UdderEnemy prionTarget = IsPrionInfected && !milkshakeTarget ? game.FindNearestPrionTarget(this) : null;
            Vector3 targetPosition = UsesCustomMovement ? transform.position : player.transform.position;
            if (milkshakeTarget)
                targetPosition = milkshakeTarget.position;
            else if (IsPrionInfected)
            {
                if (prionTarget)
                    targetPosition = prionTarget.transform.position;
                else
                {
                    targetPosition = player.transform.position;
                    TryStartPrionPlayerCharge();
                }
            }
            Vector2 delta = targetPosition - transform.position;
            Vector2 facingDirection = delta;
            bool nearCamera = !game || game.IsNearCameraView(transform.position);
            if (!IsSleeping && delta.sqrMagnitude > 0.01f)
            {
                Vector2 direction;
                float currentSpeed = rawMilkHoldTimer > 0f ? 0f : speed * GetCurrentButterSlowMultiplier();
                if (chargeTimer > 0f)
                {
                    chargeTimer -= Time.deltaTime;
                    direction = chargeDirection;
                    currentSpeed = speed * 4.2f;
                }
                else if (chargeWindupTimer > 0f)
                {
                    chargeWindupTimer -= Time.deltaTime;
                    direction = chargeDirection.sqrMagnitude > 0.01f ? chargeDirection : delta.normalized;
                    currentSpeed = 0f;
                    if (chargeWindupTimer <= 0f)
                    {
                        chargeTimer = queuedChargeDuration;
                        queuedChargeDuration = 0f;
                    }
                }
                else if (slideTimer > 0f)
                {
                    slideTimer -= Time.deltaTime;
                    direction = slideDirection;
                    currentSpeed *= slideMultiplier;
                }
                else
                {
                    direction = delta.normalized;
                    if (nearCamera && avoidsWater && chargeTimer <= 0f)
                        direction = GetWaterAwareDirection(direction);
                    if (nearCamera && UdderHazardPool.TryGetRepulsion(transform.position, direction, out Vector2 puddleRepulsion))
                        direction = puddleRepulsion;
                    if (nearCamera && game)
                    {
                        separationRefreshTimer -= Time.deltaTime;
                        if (separationRefreshTimer <= 0f)
                        {
                            cachedSeparation = game.GetEnemySeparation(this, transform.position);
                            separationRefreshTimer = Random.Range(0.08f, 0.16f);
                        }

                        direction = (direction + cachedSeparation * 0.45f).normalized;
                    }
                    else
                    {
                        separationRefreshTimer = 0f;
                        cachedSeparation = Vector2.zero;
                    }
                    lastDirection = direction;
                }

                Vector2 desiredMove = direction * currentSpeed * Time.deltaTime;
                desiredMove = ApplyPlayerPushResistance(desiredMove);
                if (nearCamera && avoidsWater && game)
                    desiredMove = game.GetWaterBlockedMove(transform.position, desiredMove, GetNavigationRadius(), waterRouteSign);
                else if (!nearCamera && game)
                    desiredMove = game.GetArenaConstrainedMove(transform.position, desiredMove);

                Vector3 nextPosition = transform.position + (Vector3)desiredMove;
                if (nearCamera && game && game.IsInWater(nextPosition, GetNavigationRadius()))
                {
                    if (game.TryGetWaterAvoidance(transform.position, direction, waterRouteSign, out Vector2 avoidanceDirection, out _))
                    {
                        Vector2 avoidanceMove = avoidanceDirection * currentSpeed * Time.deltaTime;
                        if (avoidsWater)
                            avoidanceMove = game.GetWaterBlockedMove(transform.position, avoidanceMove, GetNavigationRadius(), waterRouteSign);
                        nextPosition = transform.position + (Vector3)avoidanceMove;
                        direction = avoidanceDirection;
                    }
                    else
                        nextPosition = transform.position;
                }

                transform.position = nextPosition;
                facingDirection = direction;
            }

            if (!IsSleeping && IsPrionInfected && prionTarget && delta.sqrMagnitude <= 0.42f * 0.42f && prionAttackTimer <= 0f)
            {
                prionAttackTimer = 0.35f;
                prionTarget.TakePrionDamage(4f);
            }

            if (!IsSleeping && milkshakeTarget && delta.sqrMagnitude <= 0.75f * 0.75f && milkshakeTarget.TryGetComponent(out UdderMilkshake milkshake))
                milkshake.TakeDamage(contactDamage * Time.deltaTime);

            if (nearCamera)
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
                spriteRenderer.color = IsSleeping ? Color.Lerp(baseColor, new Color(0.68f, 0.82f, 1f), 0.6f + Mathf.Sin(Time.time * 5f) * 0.12f) : baseColor;
                UpdateRawMilkFlies(false);
            }

            lactoseTimer -= Time.deltaTime;
            UpdateStatusEmote();
        }

        private void UpdateBossBehavior()
        {
            if (!IsBoss || bossType != UdderBossType.Lidia || !player || !player.IsAlive)
                return;

            chargeCooldown -= Time.deltaTime;
            if (chargeCooldown > 0f || chargeTimer > 0f || chargeWindupTimer > 0f)
                return;

            Vector2 toPlayer = player.transform.position - transform.position;
            if (toPlayer.sqrMagnitude < 1.8f * 1.8f || toPlayer.sqrMagnitude > 8f * 8f)
                return;

            chargeDirection = toPlayer.normalized;
            chargeWindupTimer = 0.45f;
            queuedChargeDuration = 0.8f;
            chargeCooldown = 3.6f;
            game.ShowCowText(transform.position, "CHARGE!");
        }

        private void TryStartPrionPlayerCharge()
        {
            if (!player || !player.IsAlive || chargeCooldown > 0f || chargeTimer > 0f || chargeWindupTimer > 0f)
                return;

            Vector2 toPlayer = player.transform.position - transform.position;
            if (toPlayer.sqrMagnitude < 1.8f * 1.8f || toPlayer.sqrMagnitude > 8f * 8f)
                return;

            chargeDirection = toPlayer.normalized;
            chargeWindupTimer = 0.45f;
            queuedChargeDuration = 0.8f;
            chargeCooldown = 3.6f;
            game.ShowCowText(transform.position, "CHARGE!");
        }

        private Vector2 ApplyPlayerPushResistance(Vector2 desiredMove)
        {
            if (!player || desiredMove.sqrMagnitude <= 0.000001f)
                return desiredMove;

            float resistance = player.EnemyPushResistance;
            if (resistance <= 0f)
                return desiredMove;

            Vector2 toPlayer = (Vector2)player.transform.position - (Vector2)transform.position;
            float distanceSqr = toPlayer.sqrMagnitude;
            if (distanceSqr <= 0.0001f || distanceSqr > 1.05f * 1.05f)
                return desiredMove;

            Vector2 intoPlayer = toPlayer.normalized;
            float inwardSpeed = Vector2.Dot(desiredMove, intoPlayer);
            if (inwardSpeed <= 0f)
                return desiredMove;

            return desiredMove - intoPlayer * (inwardSpeed * resistance);
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
                        otherEnemy.TakePrionDamage(4f);
                    }
                    return;
                }

                SpreadRawMilkTo(otherEnemy);
                return;
            }

            if (!collision.collider.TryGetComponent(out UdderPlayer cow))
            {
                if (collision.collider.TryGetComponent(out UdderAlliedCow alliedCow))
                {
                    if (meleeTimer <= 0f)
                    {
                        meleeTimer = 0.28f;
                        TakeHornDamage(IsSliding ? 7.5f : 5.5f);
                    }

                    if (!IsSliding && !IsPrionInfected && !IsSleeping)
                        alliedCow.TakeDamage(contactDamage * Time.deltaTime);
                }

                return;
            }

            if (meleeTimer <= 0f)
            {
                meleeTimer = 0.28f;
                float stompDamage = cow.StompDamage;
                if (stompDamage > 0f)
                    TakeHornDamage(IsSliding ? stompDamage + 2f : stompDamage);
            }

            if (!IsSliding && !IsPrionInfected && !IsSleeping)
            {
                float damage = chargeTimer > 0f ? contactDamage * 3.5f : contactDamage * Time.deltaTime;
                cow.TakeDamage(damage);
                if (chargeTimer > 0f)
                {
                    chargeTimer = 0f;
                    chargeCooldown = Mathf.Max(chargeCooldown, 2.2f);
                }
            }
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

        private Vector2 GetWaterAwareDirection(Vector2 desiredDirection)
        {
            if (!game)
                return desiredDirection;

            waterRouteTimer -= Time.deltaTime;
            if (waterRouteTimer <= 0f)
                waterRouteSign = 0;

            if (!game.TryGetWaterAvoidance(transform.position, desiredDirection, waterRouteSign, out Vector2 avoidanceDirection, out Vector2 center))
                return desiredDirection;

            if (waterRouteSign == 0 || (center - waterRouteCenter).sqrMagnitude > 0.04f)
            {
                Vector2 away = (Vector2)transform.position - center;
                if (away.sqrMagnitude < 0.01f)
                    away = Vector2.up;

                Vector2 clockwise = new(away.y, -away.x);
                waterRouteSign = Vector2.Dot(clockwise, desiredDirection) >= 0f ? -1 : 1;
                waterRouteCenter = center;
            }

            waterRouteTimer = 1.2f;
            return game.TryGetWaterAvoidance(transform.position, desiredDirection, waterRouteSign, out avoidanceDirection, out _)
                ? avoidanceDirection
                : desiredDirection;
        }

        private float GetNavigationRadius()
        {
            if (TryGetComponent(out CircleCollider2D circle))
                return Mathf.Max(0.08f, circle.radius * Mathf.Max(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y)) * 0.65f);

            return IsBoss ? 0.18f : 0.12f;
        }

        public bool StartButterSlide(float duration, float multiplier)
        {
            if (IsInvulnerable)
                return false;

            if (slideTimer > duration * 0.4f)
                return false;

            slideTimer = duration;
            slideMultiplier = multiplier;
            slideDirection = lastDirection.sqrMagnitude > 0.01f ? lastDirection.normalized : Vector2.down;
            if (slipTextCooldown <= 0f)
                slipTextCooldown = 1f;

            return true;
        }

        public bool ApplyButterSlick(float duration, float multiplier)
        {
            if (IsBee)
                return false;

            if (IsBoss)
            {
                butterSlowTimer = Mathf.Max(butterSlowTimer, duration);
                butterSlowMultiplier = Mathf.Min(butterSlowMultiplier, 0.45f);
                return false;
            }

            return StartButterSlide(duration, multiplier);
        }

        public void RegisterButterSlickSlide()
        {
            if (enemyKind == UdderEnemyKind.DebtChicken)
                UdderPersistence.RecordChickenSlippedOnButter();
        }

        public void MakeLactoseIntolerant(float duration)
        {
            if (IsInvulnerable)
                return;

            lactoseTimer = Mathf.Max(lactoseTimer, duration);
        }

        private void ApplyRawMilk(bool contagiousSpread)
        {
            if (IsInvulnerable)
                return;

            rawMilkTimer = Mathf.Max(rawMilkTimer, contagiousSpread ? 3.8f : 5.2f);
            if (!IsBoss)
                rawMilkHoldTimer = Mathf.Max(rawMilkHoldTimer, contagiousSpread ? 0.65f : 1.35f);
            rawMilkTick = Mathf.Min(rawMilkTick <= 0f ? 0.55f : rawMilkTick, 0.55f);
        }

        public void ApplyCondensedMilk(float attackDamage, int level, MilkMode sourceMode)
        {
            ApplyCondensedMilk(attackDamage, level, sourceMode, 1f, 1f);
        }

        public void ApplyCondensedMilk(float attackDamage, int level, MilkMode sourceMode, float damageMultiplier, float durationMultiplier)
        {
            if (IsInvulnerable)
                return;

            float duration = Mathf.Max(0.1f, level * 0.3f * Mathf.Max(0.1f, durationMultiplier));
            float totalDamage = attackDamage * level * 0.1f * Mathf.Max(0.1f, damageMultiplier);
            condensedMilkTimer = Mathf.Max(condensedMilkTimer, duration);
            condensedMilkDamagePerSecond = Mathf.Max(condensedMilkDamagePerSecond, totalDamage / duration);
            condensedMilkMode = sourceMode;
            condensedMilkTick = Mathf.Min(condensedMilkTick <= 0f ? 0.3f : condensedMilkTick, 0.3f);
        }

        public void ApplySleep(float duration)
        {
            if (health <= 0f || IsInvulnerable)
                return;

            sleepTimer = Mathf.Max(sleepTimer, duration);
            chargeTimer = 0f;
            chargeWindupTimer = 0f;
            slideTimer = 0f;
        }

        public void ApplyPrionPulse(float damagePerSecond, float spreadChance)
        {
            if (health <= 0f || IsBoss || IsBee)
                return;

            prionTimer = float.PositiveInfinity;
            prionDamagePerSecond = Mathf.Max(prionDamagePerSecond, damagePerSecond);
            prionSpreadChance = Mathf.Max(prionSpreadChance, spreadChance);
            prionTick = Mathf.Min(prionTick <= 0f ? 1f : prionTick, 1f);
            prionAttackTimer = Mathf.Min(prionAttackTimer, 0.2f);
            if (spriteRenderer)
                spriteRenderer.color = new Color(1f, 0.96f, 0.35f);
            UpdatePrionIndicator();
        }

        public bool TakePrionDamage(float amount)
        {
            if (health <= 0f || IsInvulnerable || IsBee)
                return false;

            health -= amount;
            game.ShowDamageText(transform.position, amount, MilkMode.Prion, false);
            if (health <= 0f)
                Die();
            return health <= 0f;
        }

        private void UpdatePrionPulse()
        {
            if (prionTimer <= 0f)
                return;

            prionAttackTimer -= Time.deltaTime;
            prionTick -= Time.deltaTime;
            if (spriteRenderer && rawMilkTimer <= 0f)
                spriteRenderer.color = Color.Lerp(baseColor, new Color(1f, 0.96f, 0.35f), 0.72f + Mathf.Sin(Time.time * 16f) * 0.12f);

            if (prionTick > 0f)
                return;

            prionTick = 1f;
            health -= Mathf.Max(0.1f, prionDamagePerSecond);
            if (health <= 0f)
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
            if (health <= 0f || IsInvulnerable)
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
            if (health <= 0f || IsInvulnerable)
                return;

            bool crit = canCrit && Random.value < game.CritChance;
            float dairyMultiplier = lactoseTimer > 0f ? 1.65f : 1f;
            float finalAmount = amount * dairyMultiplier;
            finalAmount = crit ? finalAmount * 2.2f : finalAmount;
            health -= finalAmount;

            if ((mode == MilkMode.Buttermilk || mode == MilkMode.TresLeches) && !IsBee)
            {
                acidTimer = 1.2f;
                StartButterSlide(GetButtermilkSlideDuration(effectLevel), 3.4f);
            }
            else if (mode == MilkMode.Buttermilk || mode == MilkMode.TresLeches)
            {
                acidTimer = 1.2f;
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

        private float GetCurrentButterSlowMultiplier()
        {
            if (butterSlowTimer <= 0f)
            {
                butterSlowMultiplier = 1f;
                return 1f;
            }

            return Mathf.Clamp(butterSlowMultiplier, 0.1f, 1f);
        }

        private void Die()
        {
            if (dying)
                return;

            if (bossType == UdderBossType.Ruminator && IsBoss && !ruminatorRespawned)
            {
                ruminatorRespawned = true;
                game.ShowCowText(transform.position, "I'll be back");
                health = maxHealth * 2f;
                maxHealth *= 2f;
                speed *= 0.5f;
                rawMilkTimer = 0f;
                rawMilkHoldTimer = 0f;
                prionTimer = 0f;
                prionSpreadChance = 0f;
                slideTimer = 0f;
                poisonTimer = 0f;
                condensedMilkTimer = 0f;
                if (spriteRenderer)
                    spriteRenderer.color = baseColor;
                UpdateBossHealthBar();
                return;
            }

            if (IsPrionInfected)
                game.SpreadPrionOnDeath(this, transform.position, GetColliderDiameter(), prionDamagePerSecond, prionSpreadChance);

            dying = true;
            game.RegisterEnemyDefeated(this);
            StartCoroutine(PlayDeathSequence());
        }

        private float GetColliderDiameter()
        {
            if (TryGetComponent(out CircleCollider2D circle))
                return circle.radius * 2f * Mathf.Max(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y));

            return GetNavigationRadius() * 2f;
        }

        private IEnumerator PlayDeathSequence()
        {
            foreach (Collider2D collider in GetComponents<Collider2D>())
                collider.enabled = false;

            if (TryGetComponent(out Rigidbody2D body))
                body.simulated = false;

            UpdateRawMilkFlies(false);
            if (prionIndicatorRenderer)
                prionIndicatorRenderer.gameObject.SetActive(false);
            if (bossHealthBarRoot)
                bossHealthBarRoot.gameObject.SetActive(false);
            if (statusEmoteRenderer)
                statusEmoteRenderer.gameObject.SetActive(false);

            if (spriteRenderer)
            {
                spriteRenderer.color = Color.white;
                spriteRenderer.sortingOrder = 9;
                if (cottonDeathSprite)
                    spriteRenderer.sprite = cottonDeathSprite;
            }

            yield return new WaitForSeconds(1f);

            if (spriteRenderer)
                spriteRenderer.enabled = false;
            SetStatusEmote(skeletonDeathSprite);

            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

        public void ShowProudEmote()
        {
            proudEmoteLocked = true;
            SetStatusEmote(proudEmoteSprite);
        }

        private void UpdateStatusEmote()
        {
            if (dying || !IsAlive)
                return;
            if (proudEmoteLocked)
                return;

            Sprite emote = null;
            if (rawMilkTimer > 0f)
                emote = rawMilkEmoteSprite;
            else if (poisonTimer > 0f)
                emote = spoiledMilkEmoteSprite;

            SetStatusEmote(emote);
        }

        private void SetStatusEmote(Sprite emote)
        {
            if (!emote)
            {
                if (statusEmoteRenderer)
                    statusEmoteRenderer.gameObject.SetActive(false);
                return;
            }

            EnsureStatusEmoteRenderer();
            if (!statusEmoteRenderer)
                return;

            statusEmoteRenderer.sprite = emote;
            statusEmoteRenderer.gameObject.SetActive(true);
            statusEmoteRenderer.transform.localPosition = Vector3.up * (IsBoss ? 0.42f : 0.32f);
            statusEmoteRenderer.transform.localScale = Vector3.one * GetStatusEmoteScale();
        }

        private void EnsureStatusEmoteRenderer()
        {
            if (statusEmoteRenderer)
                return;

            GameObject emote = new("Status Emote");
            emote.transform.SetParent(transform, false);
            emote.transform.localScale = Vector3.one * GetStatusEmoteScale();
            statusEmoteRenderer = emote.AddComponent<SpriteRenderer>();
            statusEmoteRenderer.sortingOrder = 12;
        }

        private float GetStatusEmoteScale()
        {
            return IsBoss ? 0.09f : 0.065f;
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

        private void UpdatePrionIndicator()
        {
            if (!prionAngrySprite)
                return;

            if (!prionIndicatorRenderer)
                CreatePrionIndicator();

            if (prionIndicatorRenderer)
                prionIndicatorRenderer.gameObject.SetActive(IsPrionInfected && !dying);
        }

        private void CreatePrionIndicator()
        {
            GameObject indicator = prionIndicatorPrefab ? Instantiate(prionIndicatorPrefab) : new GameObject("Prion Anger Indicator");
            indicator.name = "Prion Anger Indicator";
            indicator.transform.SetParent(transform, false);
            indicator.transform.localPosition = new Vector3(0f, 0.58f, 0f);
            prionIndicatorRenderer = indicator.GetComponent<SpriteRenderer>();
            if (!prionIndicatorRenderer)
                prionIndicatorRenderer = indicator.AddComponent<SpriteRenderer>();
            prionIndicatorRenderer.sprite = prionAngrySprite;
            prionIndicatorRenderer.sortingOrder = 8;

            float height = prionAngrySprite.bounds.size.y;
            if (height > 0f)
            {
                float scale = 0.24f / height;
                indicator.transform.localScale = Vector3.one * scale;
            }
        }

        private void UpdateBossHealthBar()
        {
            if (!IsBoss || dying || health <= 0f)
            {
                if (bossHealthBarRoot)
                    bossHealthBarRoot.gameObject.SetActive(false);
                return;
            }

            EnsureBossHealthBar();
            if (!bossHealthBarRoot || !bossHealthFill)
                return;

            bossHealthBarRoot.gameObject.SetActive(true);
            Vector3 parentScale = transform.localScale;
            bossHealthBarRoot.localScale = new Vector3(
                parentScale.x != 0f ? 1f / Mathf.Abs(parentScale.x) : 1f,
                parentScale.y != 0f ? 1f / Mathf.Abs(parentScale.y) : 1f,
                1f);
            bossHealthBarRoot.localPosition = new Vector3(-0.875f / Mathf.Max(0.001f, Mathf.Abs(parentScale.x)), parentScale.y != 0f ? 1.22f / Mathf.Abs(parentScale.y) : 1.22f, 0f);
            Vector3 scale = bossHealthFill.localScale;
            scale.x = 1.62f * Mathf.Clamp01(health / Mathf.Max(1f, maxHealth));
            bossHealthFill.localScale = scale;
        }

        private void EnsureBossHealthBar()
        {
            if (bossHealthBarRoot)
                return;

            bossHealthBarRoot = new GameObject("Boss Health Bar").transform;
            bossHealthBarRoot.SetParent(transform, false);

            GameObject back = new("Health Bar Back");
            back.transform.SetParent(bossHealthBarRoot, false);
            back.transform.localPosition = Vector3.zero;
            back.transform.localScale = new Vector3(1.7f, 0.12f, 1f);
            var backRenderer = back.AddComponent<SpriteRenderer>();
            backRenderer.sprite = GetHealthBarSprite();
            backRenderer.color = new Color(0.05f, 0f, 0f, 0.78f);
            backRenderer.sortingOrder = 10;

            GameObject fill = new("Health Bar Fill");
            fill.transform.SetParent(bossHealthBarRoot, false);
            fill.transform.localPosition = new Vector3(0.04f, 0f, -0.01f);
            fill.transform.localScale = new Vector3(1.62f, 0.075f, 1f);
            var fillRenderer = fill.AddComponent<SpriteRenderer>();
            fillRenderer.sprite = GetHealthBarSprite();
            fillRenderer.color = new Color(0.92f, 0.02f, 0.02f, 1f);
            fillRenderer.sortingOrder = 11;
            bossHealthFill = fill.transform;
        }

        private static Sprite GetHealthBarSprite()
        {
            if (healthBarSprite)
                return healthBarSprite;

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            healthBarSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0f, 0.5f), 1f);
            healthBarSprite.name = "Runtime Boss Health Bar Pixel";
            return healthBarSprite;
        }
    }
}
