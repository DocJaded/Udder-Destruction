using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderBeeDrone : MonoBehaviour
    {
        private UdderEnemy enemy;
        private UdderPlayer player;
        private Transform queen;
        private int orbitIndex;
        private int orbitCount;
        private float orbitOffset;
        private float attackDamageTimer;
        private bool attacking;

        public bool IsAlive => enemy && enemy.IsAlive;

        public void Init(UdderEnemy droneEnemy, UdderPlayer target, Transform queenTransform, int index, int count)
        {
            enemy = droneEnemy;
            player = target;
            queen = queenTransform;
            orbitIndex = index;
            orbitCount = Mathf.Max(1, count);
            orbitOffset = Random.Range(0f, Mathf.PI * 2f);
            enemy.UsesCustomMovement = true;
        }

        public void SetAttacking(bool value)
        {
            attacking = value;
        }

        private void Update()
        {
            if (!IsAlive || !queen || !player || !player.IsAlive)
                return;

            Vector3 target;
            float moveSpeed;
            if (attacking)
            {
                target = player.transform.position;
                moveSpeed = enemy.speed * 1.8f;
            }
            else
            {
                float angle = orbitOffset + Time.time * 0.75f + orbitIndex * Mathf.PI * 2f / orbitCount;
                target = queen.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 1.45f;
                moveSpeed = enemy.speed;
            }

            Vector3 delta = target - transform.position;
            if (delta.sqrMagnitude > 0.01f)
                transform.position += delta.normalized * (moveSpeed * Time.deltaTime);

            attackDamageTimer -= Time.deltaTime;
            if (attacking && attackDamageTimer <= 0f && ((Vector2)(player.transform.position - transform.position)).sqrMagnitude <= 0.45f * 0.45f)
            {
                attackDamageTimer = 0.2f;
                player.TakeDamage(enemy.contactDamage * 0.2f);
            }
        }
    }
}
