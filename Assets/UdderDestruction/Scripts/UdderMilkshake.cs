using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderMilkshake : MonoBehaviour
    {
        private UdderGameController game;
        private float health;

        public bool IsAlive => health > 0f;

        public void Init(UdderGameController owner, float maxHealth)
        {
            game = owner;
            health = Mathf.Max(1f, maxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (health <= 0f)
                return;

            health -= Mathf.Max(0f, amount);
            if (health > 0f)
                return;

            game?.ClearMilkshake(this);
            Destroy(gameObject);
        }
    }
}
