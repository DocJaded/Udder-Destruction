using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderAlliedCow : MonoBehaviour
    {
        private const float MaxHealth = 100f;
        private const float StompDamage = 5.5f;

        private UdderGameController game;
        private UdderPlayer player;
        private Sprite downSprite;
        private Sprite sideSprite;
        private Sprite upSprite;
        private SpriteRenderer spriteRenderer;
        private float health;
        private float attackTimer;
        private Vector2 facing = Vector2.down;

        public bool IsAlive => health > 0f;

        public void Init(UdderGameController owner, UdderPlayer target, Sprite down, Sprite side, Sprite up)
        {
            game = owner;
            player = target;
            downSprite = down;
            sideSprite = side;
            upSprite = up;
            health = MaxHealth;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (health <= 0f || !game || !player || !player.IsAlive)
                return;

            attackTimer -= Time.deltaTime;
            UdderEnemy target = game.FindNearestEnemy(transform.position, 5.2f);
            Vector3 destination = target ? target.transform.position : player.transform.position + GetFollowOffset();
            Vector2 delta = destination - transform.position;
            if (delta.sqrMagnitude > 0.04f)
            {
                facing = delta.normalized;
                Vector2 move = facing * 3.25f * Time.deltaTime;
                move = game.GetArenaConstrainedMove(transform.position, move);
                transform.position += (Vector3)move;
            }

            UdderSpriteFacing.Apply(spriteRenderer, facing, downSprite, sideSprite, upSprite, true);

            if (target && attackTimer <= 0f && ((Vector2)(target.transform.position - transform.position)).sqrMagnitude <= 0.55f * 0.55f)
            {
                attackTimer = 0.35f;
                target.TakeDamage(StompDamage, MilkMode.Stomp, false);
            }
        }

        public void TakeDamage(float amount)
        {
            if (health <= 0f)
                return;

            health -= amount;
            if (health <= 0f)
                Destroy(gameObject);
        }

        private Vector3 GetFollowOffset()
        {
            int hash = Mathf.Abs(GetInstanceID());
            float angle = (hash % 360) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 1.8f;
        }
    }
}
