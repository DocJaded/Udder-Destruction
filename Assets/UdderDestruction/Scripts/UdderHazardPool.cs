using System.Collections.Generic;
using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderHazardPool : MonoBehaviour
    {
        public float damagePerSecond = 4f;
        public float life = 4f;
        public MilkMode mode = MilkMode.SpoiledMilk;

        private readonly List<UdderEnemy> enemies = new();
        private readonly List<UdderSeaUrchin> seaUrchins = new();

        private void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (!enemies[i])
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                enemies[i].TakeDamage(damagePerSecond * Time.deltaTime, mode, false);
            }

            for (int i = seaUrchins.Count - 1; i >= 0; i--)
            {
                if (!seaUrchins[i])
                {
                    seaUrchins.RemoveAt(i);
                    continue;
                }

                seaUrchins[i].TakeDamage(damagePerSecond * Time.deltaTime, mode);
            }

            float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.05f;
            transform.localScale = Vector3.one * pulse;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out UdderEnemy enemy) && !enemies.Contains(enemy))
                enemies.Add(enemy);
            if (other.TryGetComponent(out UdderSeaUrchin seaUrchin) && !seaUrchins.Contains(seaUrchin))
                seaUrchins.Add(seaUrchin);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out UdderEnemy enemy))
                enemies.Remove(enemy);
            if (other.TryGetComponent(out UdderSeaUrchin seaUrchin))
                seaUrchins.Remove(seaUrchin);
        }
    }
}
