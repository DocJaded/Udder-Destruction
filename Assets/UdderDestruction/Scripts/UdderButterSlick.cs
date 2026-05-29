using System.Collections.Generic;
using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderButterSlick : MonoBehaviour
    {
        public float life = 8f;
        public float slideDuration = 0.85f;
        public float slideMultiplier = 3.4f;

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

            if (spriteRenderer)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Clamp01(life / 1.5f) * 0.72f;
                spriteRenderer.color = color;
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (!enemies[i])
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                enemies[i].StartButterSlide(slideDuration, slideMultiplier);
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
