using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderProjectile : MonoBehaviour
    {
        public float speed = 9f;
        public float damage = 4f;
        public float life = 1.4f;
        public MilkMode mode;
        public UdderGameController game;

        private Vector2 direction;

        public void Fire(Vector2 fireDirection)
        {
            direction = fireDirection.sqrMagnitude < 0.01f ? Vector2.right : fireDirection.normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            life -= Time.deltaTime;
            if (life <= 0f)
                Burst();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out UdderSeaUrchin seaUrchin))
            {
                seaUrchin.TakeDamage(damage, mode);
                Burst();
                return;
            }

            if (!other.TryGetComponent(out UdderEnemy enemy))
                return;

            enemy.TakeDamage(damage, mode);
            Burst();
        }

        private void Burst()
        {
            if (mode == MilkMode.SpoiledMilk && game)
                game.SpawnSpoiledPool(transform.position);

            Destroy(gameObject);
        }
    }
}
