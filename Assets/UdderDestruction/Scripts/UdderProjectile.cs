using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderProjectile : MonoBehaviour
    {
        public float speed = 9f;
        public float damage = 4f;
        public float life = 1.4f;
        public MilkMode mode;
        public int powerLevel = 1;
        public int condensedMilkLevel;
        public float condensedMilkDamageMultiplier = 1f;
        public float condensedMilkDurationMultiplier = 1f;
        public float sleepDuration;
        public UdderGameController game;
        public GameObject rawMilkFlyPrefab;
        public Sprite rawMilkFlySprite;
        public float spinDegreesPerSecond = 240f;
        public float homingStrength = 7f;

        private const int RawMilkProjectileFlyCount = 3;
        private readonly Transform[] rawMilkFlies = new Transform[RawMilkProjectileFlyCount];
        private readonly float[] rawMilkFlySeeds = new float[RawMilkProjectileFlyCount];
        private Vector2 direction;
        private UdderEnemy prionTarget;
        private float prionDamagePerSecond;
        private float prionSpreadChance;

        public void Fire(Vector2 fireDirection)
        {
            direction = fireDirection.sqrMagnitude < 0.01f ? Vector2.right : fireDirection.normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (mode == MilkMode.WholeMilk)
                angle += 180f;

            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            CreateRawMilkFlies();
        }

        private void Update()
        {
            if (mode == MilkMode.Prion)
            {
                if (!prionTarget || !prionTarget.IsAlive)
                {
                    Burst(false);
                    return;
                }

                Vector2 toTarget = prionTarget.transform.position - transform.position;
                if (toTarget.sqrMagnitude > 0.01f)
                    direction = Vector2.Lerp(direction, toTarget.normalized, homingStrength * Time.deltaTime).normalized;
                transform.Rotate(0f, 0f, spinDegreesPerSecond * Time.deltaTime);
            }

            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            UpdateRawMilkFlies();
            life -= Time.deltaTime;
            if (life <= 0f)
                Burst(false);
        }

        public void ConfigurePrionTarget(UdderEnemy target, float damagePerSecond, float spreadChance)
        {
            prionTarget = target;
            prionDamagePerSecond = damagePerSecond;
            prionSpreadChance = spreadChance;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out UdderDolphinSurface dolphin))
            {
                if (mode == MilkMode.Prion)
                    return;

                if (mode == MilkMode.SpoiledMilk && game)
                {
                    dolphin.BeachFromSpoiledMilk(transform.position);
                    game.ContaminateWaterAt(transform.position);
                }

                Burst(false);
                return;
            }

            if (mode == MilkMode.SpoiledMilk)
            {
                if (other.TryGetComponent<UdderEnemy>(out _) || other.TryGetComponent<UdderSeaUrchin>(out _))
                    Burst(true);

                return;
            }

            if (other.TryGetComponent(out UdderSeaUrchin seaUrchin))
            {
                if (mode == MilkMode.Prion)
                    return;

                seaUrchin.TakeDamage(damage, mode);
                Burst(false);
                return;
            }

            if (!other.TryGetComponent(out UdderEnemy enemy))
                return;

            if (mode == MilkMode.Prion)
            {
                if (enemy != prionTarget)
                    return;

                enemy.ApplyPrionPulse(prionDamagePerSecond, prionSpreadChance);
                Burst(false);
                return;
            }

            enemy.TakeDamage(damage, mode, true, powerLevel);
            if (sleepDuration > 0f && mode == MilkMode.WholeMilk)
                enemy.ApplySleep(sleepDuration);
            if (condensedMilkLevel > 0 && (mode == MilkMode.WholeMilk || mode == MilkMode.Buttermilk || mode == MilkMode.RawMilk || mode == MilkMode.TresLeches))
                enemy.ApplyCondensedMilk(damage, condensedMilkLevel, mode, condensedMilkDamageMultiplier, condensedMilkDurationMultiplier);
            Burst(false);
        }

        private void Burst(bool createSpoiledPool)
        {
            if (createSpoiledPool && mode == MilkMode.SpoiledMilk && game)
                game.SpawnSpoiledPool(transform.position, powerLevel);

            Destroy(gameObject);
        }

        private void CreateRawMilkFlies()
        {
            if (mode != MilkMode.RawMilk || rawMilkFlies[0])
                return;

            for (int i = 0; i < rawMilkFlies.Length; i++)
            {
                GameObject fly = rawMilkFlyPrefab ? Instantiate(rawMilkFlyPrefab, transform) : new GameObject($"Raw Milk Projectile Fly {i + 1}");
                fly.transform.SetParent(transform, false);
                fly.transform.localPosition = GetRawMilkFlyOffset(i, 0f);
                fly.transform.localScale = Vector3.one * 0.14f;

                SpriteRenderer renderer = fly.GetComponent<SpriteRenderer>();
                if (!renderer)
                    renderer = fly.AddComponent<SpriteRenderer>();
                renderer.sprite = rawMilkFlySprite ? rawMilkFlySprite : renderer.sprite;
                renderer.sortingOrder = 7;

                rawMilkFlies[i] = fly.transform;
                rawMilkFlySeeds[i] = Random.Range(0f, Mathf.PI * 2f);
            }
        }

        private void UpdateRawMilkFlies()
        {
            if (mode != MilkMode.RawMilk || !rawMilkFlies[0])
                return;

            for (int i = 0; i < rawMilkFlies.Length; i++)
            {
                Transform fly = rawMilkFlies[i];
                if (!fly)
                    continue;

                fly.localPosition = GetRawMilkFlyOffset(i, Time.time + rawMilkFlySeeds[i]);
            }
        }

        private static Vector3 GetRawMilkFlyOffset(int index, float time)
        {
            float angle = index * Mathf.PI * 2f / RawMilkProjectileFlyCount;
            Vector2 baseOffset = new(Mathf.Cos(angle) * 0.24f, Mathf.Sin(angle) * 0.18f);
            Vector2 buzz = new(Mathf.Sin(time * 13f + index) * 0.025f, Mathf.Cos(time * 11f + index * 0.7f) * 0.02f);
            return baseOffset + buzz;
        }
    }
}
