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
        public UdderGameController game;
        public float spinDegreesPerSecond = 240f;
        public float homingStrength = 7f;

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
            if (condensedMilkLevel > 0 && (mode == MilkMode.WholeMilk || mode == MilkMode.Buttermilk || mode == MilkMode.RawMilk))
                enemy.ApplyCondensedMilk(damage, condensedMilkLevel, mode);
            Burst(false);
        }

        private void Burst(bool createSpoiledPool)
        {
            if (createSpoiledPool && mode == MilkMode.SpoiledMilk && game)
                game.SpawnSpoiledPool(transform.position, powerLevel);

            Destroy(gameObject);
        }
    }
}
