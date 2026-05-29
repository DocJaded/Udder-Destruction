using System.Collections.Generic;
using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderDairyAirCloud : MonoBehaviour
    {
        public float life = 7f;
        public float intoleranceDuration = 5f;

        private readonly List<UdderEnemy> enemies = new();
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            float pulse = 1f + Mathf.Sin(Time.time * 3.5f) * 0.08f;
            transform.localScale = Vector3.one * (2.4f * pulse);

            if (spriteRenderer)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Clamp01(life / 1.5f) * 0.42f;
                spriteRenderer.color = color;
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (!enemies[i])
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                enemies[i].MakeLactoseIntolerant(intoleranceDuration);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out UdderEnemy enemy) && !enemies.Contains(enemy))
                enemies.Add(enemy);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out UdderEnemy enemy))
                enemies.Remove(enemy);
        }
    }
}
