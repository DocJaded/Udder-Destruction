using System.Collections.Generic;
using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderDairyAirCloud : MonoBehaviour
    {
        public float life = 7f;
        public bool permanent;
        public Transform followTarget;
        public float radiusMultiplier = 1f;

        private readonly List<UdderEnemy> enemies = new();
        private SpriteRenderer[] spriteRenderers;

        private void Awake()
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            if (followTarget)
                transform.position = followTarget.position;

            if (!permanent)
                life -= Time.deltaTime;
            if (!permanent && life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            float pulse = 1f + Mathf.Sin(Time.time * 3.5f) * 0.08f;
            transform.localScale = Vector3.one * (2.4f * Mathf.Max(0.1f, radiusMultiplier) * pulse);

            if (spriteRenderers != null)
            {
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    SpriteRenderer spriteRenderer = spriteRenderers[i];
                    if (!spriteRenderer)
                        continue;

                    Color color = spriteRenderer.color;
                    if (color.a > 0f)
                    {
                        color.a = (permanent ? 1f : Mathf.Clamp01(life / 1.5f)) * 0.42f;
                        spriteRenderer.color = color;
                    }
                }
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (!enemies[i])
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                enemies[i].MakeLactoseIntolerant(float.PositiveInfinity);
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
