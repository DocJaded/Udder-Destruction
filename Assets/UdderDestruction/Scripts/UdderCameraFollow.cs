using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderCameraFollow : MonoBehaviour
    {
        public Transform target;
        public float followSharpness = 12f;

        private void LateUpdate()
        {
            if (!target)
                return;

            Vector3 desired = new(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSharpness * Time.deltaTime));
        }
    }
}
