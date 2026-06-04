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
        private readonly HashSet<UdderEnemy> countedChickenSlips = new();
        private SpriteRenderer[] spriteRenderers;

        private void Awake()
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            float alpha = Mathf.Clamp01(life / 1.5f);
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                SpriteRenderer spriteRenderer = spriteRenderers[i];
                if (!spriteRenderer)
                    continue;

                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (!enemies[i])
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                if (enemies[i].StartButterSlide(slideDuration, slideMultiplier) && countedChickenSlips.Add(enemies[i]))
                    enemies[i].RegisterButterSlickSlide();
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
