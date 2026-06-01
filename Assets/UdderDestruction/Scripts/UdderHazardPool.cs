using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderHazardPool : MonoBehaviour
    {
        private static readonly System.Collections.Generic.List<UdderHazardPool> ActivePools = new();

        public float life = 3f;
        public float radius = 0.75f;
        public RuntimeAnimatorController idleController;
        public RuntimeAnimatorController hurtController;
        public RuntimeAnimatorController deathController;
        public float idleDuration = 0.8f;
        public float deathDuration = 0.8f;

        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private float age;
        private bool showingHurt;
        private bool showingDeath;

        public static bool TryGetRepulsion(Vector2 position, Vector2 desiredDirection, out Vector2 repulsionDirection)
        {
            repulsionDirection = Vector2.zero;
            float strongestPressure = 0f;

            for (int i = ActivePools.Count - 1; i >= 0; i--)
            {
                UdderHazardPool pool = ActivePools[i];
                if (!pool)
                {
                    ActivePools.RemoveAt(i);
                    continue;
                }

                Vector2 center = pool.transform.position;
                float activeRadius = pool.Radius;
                Vector2 lookAhead = desiredDirection.sqrMagnitude > 0.01f
                    ? position + desiredDirection.normalized * 0.6f
                    : position;

                float distance = Vector2.Distance(position, center);
                float lookAheadDistance = Vector2.Distance(lookAhead, center);
                if (distance > activeRadius && lookAheadDistance > activeRadius)
                    continue;

                Vector2 away = position - center;
                if (away.sqrMagnitude <= 0.0001f)
                    away = desiredDirection.sqrMagnitude > 0.01f ? -desiredDirection : Vector2.up;

                Vector2 direction = away.normalized;
                if (distance > activeRadius)
                {
                    Vector2 tangentA = new(-direction.y, direction.x);
                    Vector2 tangentB = -tangentA;
                    direction = Vector2.Dot(tangentA, desiredDirection) >= Vector2.Dot(tangentB, desiredDirection)
                        ? tangentA
                        : tangentB;
                }

                float pressure = 1f - Mathf.Clamp01(Mathf.Min(distance, lookAheadDistance) / activeRadius);
                if (pressure > strongestPressure)
                {
                    strongestPressure = pressure;
                    repulsionDirection = direction;
                }
            }

            return repulsionDirection.sqrMagnitude > 0.01f;
        }

        private float Radius => Mathf.Max(0.05f, radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y));

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            if (!ActivePools.Contains(this))
                ActivePools.Add(this);
        }

        private void OnDisable()
        {
            ActivePools.Remove(this);
        }

        private void Update()
        {
            age += Time.deltaTime;
            life -= Time.deltaTime;
            if (life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            UpdateAnimationState();

            if (spriteRenderer)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Clamp01(life / 0.65f) * 0.62f;
                spriteRenderer.color = color;
            }
        }

        private void UpdateAnimationState()
        {
            if (!animator)
                return;

            if (!showingDeath && deathController && life <= deathDuration)
            {
                showingDeath = true;
                animator.runtimeAnimatorController = deathController;
                animator.Play(0, 0, 0f);
                animator.speed = deathDuration > 0f && animator.runtimeAnimatorController.animationClips.Length > 0
                    ? animator.runtimeAnimatorController.animationClips[0].length / deathDuration
                    : 1f;
                return;
            }

            if (!showingHurt && !showingDeath && hurtController && age >= idleDuration)
            {
                showingHurt = true;
                animator.runtimeAnimatorController = hurtController;
                animator.Play(0, 0, 0f);
            }
        }
    }
}
