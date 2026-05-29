using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderSeaUrchin : MonoBehaviour
    {
        public float health = 12f;
        public float speed = 4.4f;
        public float contactDamage = 10f;

        private UdderGameController game;
        private Vector3 target;
        private bool grounded;

        public bool IsGrounded => grounded;
        public bool IsAlive => health > 0f;

        public void Init(UdderGameController owner, Vector3 targetPosition)
        {
            game = owner;
            target = targetPosition;
        }

        private void Update()
        {
            if (grounded)
                return;

            Vector3 delta = target - transform.position;
            float step = speed * Time.deltaTime;
            if (delta.magnitude <= step)
            {
                transform.position = target;
                grounded = true;
                game.RegisterGroundedSeaUrchin(this);
                return;
            }

            transform.position += delta.normalized * step;
            transform.Rotate(0f, 0f, 360f * Time.deltaTime);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponent(out UdderPlayer cow))
            {
                cow.TakeDamage(contactDamage * Time.deltaTime);
                return;
            }

            if (!grounded)
                return;

            if (other.TryGetComponent(out UdderEnemy enemy) && !enemy.isFlying)
                enemy.TakeDamage(contactDamage * Time.deltaTime, MilkMode.WholeMilk, false);
        }

        public void TakeDamage(float amount, MilkMode mode)
        {
            if (health <= 0f)
                return;

            health -= amount;
            game.ShowDamageText(transform.position, amount, mode, false);

            if (health <= 0f)
            {
                game.UnregisterSeaUrchin(this);
                Destroy(gameObject);
            }
        }
    }
}
