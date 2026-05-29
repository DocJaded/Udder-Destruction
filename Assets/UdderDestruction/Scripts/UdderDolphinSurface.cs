using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderDolphinSurface : MonoBehaviour
    {
        public float life = 1.25f;

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

            float bob = Mathf.Sin(Time.time * 9f) * 0.08f;
            transform.position += Vector3.up * (bob * Time.deltaTime);

            if (spriteRenderer)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Clamp01(life / 0.55f);
                spriteRenderer.color = color;
            }
        }
    }
}
