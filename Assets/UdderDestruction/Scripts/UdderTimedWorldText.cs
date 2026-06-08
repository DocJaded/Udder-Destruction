using TMPro;
using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderTimedWorldText : MonoBehaviour
    {
        public TextMeshPro label;
        public float lingerTime = 3f;
        public float fadeTime = 0.7f;
        public float bobAmount = 0.06f;
        public float pulseAmount = 0.08f;
        public Transform followTarget;
        public Vector3 worldOffset;

        private Color baseColor = Color.white;
        private Vector3 baseLocalPosition;
        private Vector3 baseLocalScale;
        private float age;

        private void Awake()
        {
            if (!label)
                label = GetComponent<TextMeshPro>();

            if (label)
                baseColor = label.color;

            baseLocalPosition = transform.localPosition;
            baseLocalScale = transform.localScale;
        }

        private void Update()
        {
            age += Time.deltaTime;

            float pulse = 1f + Mathf.Sin(age * 8f) * pulseAmount;
            transform.localScale = baseLocalScale * pulse;
            if (followTarget)
                transform.position = followTarget.position + worldOffset + Vector3.up * (Mathf.Sin(age * 5f) * bobAmount);
            else
                transform.localPosition = baseLocalPosition + Vector3.up * (Mathf.Sin(age * 5f) * bobAmount);

            if (label && age > lingerTime)
            {
                float fade01 = Mathf.Clamp01((age - lingerTime) / fadeTime);
                Color color = baseColor;
                color.a = 1f - fade01;
                label.color = color;
            }

            if (age >= lingerTime + fadeTime)
                Destroy(gameObject);
        }
    }
}
