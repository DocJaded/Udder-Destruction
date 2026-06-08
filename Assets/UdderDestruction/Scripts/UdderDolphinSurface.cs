using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderDolphinSurface : MonoBehaviour
    {
        public float life = 1.25f;
        public float health = 8f;
        public UdderGameController game;
        public Sprite cottonDeathSprite;
        public Sprite skullDeathSprite;
        public Sprite skeletonDeathSprite;

        private SpriteRenderer spriteRenderer;
        private SpriteRenderer deathEmoteRenderer;
        private Vector3 beachTarget;
        private float meleeTimer;
        private float dehydrationTick = 0.5f;
        private bool dying;
        private bool beaching;
        private bool beached;

        public bool IsAlive => health > 0f && !dying;
        public bool CanBeSpoiledMilkTarget => IsAlive && !beaching && !beached;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (dying)
                return;

            if (beaching || beached)
            {
                UpdateBeachedState();
                return;
            }

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

        public void BeachFromSpoiledMilk(Vector3 milkPosition)
        {
            if (dying || health <= 0f || beached || beaching)
                return;

            if (game && game.TryGetBeachPosition(transform.position, milkPosition, out Vector2 target))
                beachTarget = target;
            else
                beachTarget = transform.position + (transform.position - milkPosition).normalized;

            life = float.PositiveInfinity;
            beaching = true;
            dehydrationTick = 0.5f;

            if (spriteRenderer)
                spriteRenderer.color = new Color(0.86f, 0.96f, 0.72f);
        }

        public void TakeStompDamage(float amount)
        {
            TakeDamage(amount, MilkMode.Stomp);
        }

        private void UpdateBeachedState()
        {
            if (beaching)
            {
                Vector3 delta = beachTarget - transform.position;
                float step = 3.2f * Time.deltaTime;
                if (delta.magnitude <= step)
                {
                    transform.position = beachTarget;
                    beaching = false;
                    beached = true;
                }
                else
                {
                    transform.position += delta.normalized * step;
                }
            }

            if (!beached)
                return;

            dehydrationTick -= Time.deltaTime;
            if (dehydrationTick > 0f)
                return;

            dehydrationTick = 0.5f;
            TakeDamage(0.65f, MilkMode.Dehydration);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (dying || health <= 0f || !other.TryGetComponent(out UdderPlayer cow))
                return;

            meleeTimer -= Time.deltaTime;
            if (meleeTimer > 0f)
                return;

            meleeTimer = 0.28f;
            float stompDamage = cow.StompDamage;
            if (stompDamage > 0f)
                TakeStompDamage(stompDamage);
        }

        private void TakeDamage(float amount, MilkMode mode)
        {
            if (dying || health <= 0f)
                return;

            health -= amount;
            if (game)
                game.ShowDamageText(transform.position, amount, mode, false);

            if (health <= 0f)
                StartCoroutine(PlayDeathSequence());
        }

        private System.Collections.IEnumerator PlayDeathSequence()
        {
            dying = true;
            UdderPersistence.RecordEnemyDefeated(UdderEnemyKind.Dolphin, false);
            foreach (Collider2D collider in GetComponents<Collider2D>())
                collider.enabled = false;

            if (spriteRenderer)
            {
                spriteRenderer.color = Color.white;
                spriteRenderer.sortingOrder = 9;
                if (cottonDeathSprite)
                    spriteRenderer.sprite = cottonDeathSprite;
            }

            yield return new WaitForSeconds(1f);

            if (spriteRenderer)
                spriteRenderer.enabled = false;
            ShowDeathEmote();

            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

        private void ShowDeathEmote()
        {
            if (!skeletonDeathSprite)
                return;

            if (!deathEmoteRenderer)
            {
                GameObject emote = new("Death Emote");
                emote.transform.SetParent(transform, false);
                emote.transform.localPosition = Vector3.up * 0.28f;
                emote.transform.localScale = Vector3.one * 0.065f;
                deathEmoteRenderer = emote.AddComponent<SpriteRenderer>();
                deathEmoteRenderer.sortingOrder = 12;
            }

            deathEmoteRenderer.sprite = skeletonDeathSprite;
            deathEmoteRenderer.gameObject.SetActive(true);
        }
    }
}
