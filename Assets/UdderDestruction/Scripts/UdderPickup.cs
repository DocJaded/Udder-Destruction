using UnityEngine;

namespace UdderDestruction
{
    public enum PickupType
    {
        Cream,
        Cranberry,
        Strawberry,
        Raspberry,
        Blackberries,
        MinorMoona,
        NormalMoona,
        RemarkableMoona,
        ElysianMoona,
        DairyDouble,
        Honeycomb,
    }

    public sealed class UdderPickup : MonoBehaviour
    {
        public PickupType type;
        public int amount = 1;
        private Transform attractionTarget;

        private void Update()
        {
            transform.Rotate(0f, 0f, 90f * Time.deltaTime);
            if (attractionTarget)
                transform.position = Vector3.MoveTowards(transform.position, attractionTarget.position, 9f * Time.deltaTime);
        }

        public void AttractTo(Transform target)
        {
            attractionTarget = target;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out UdderPlayer player))
                return;

            player.Collect(this);
            Destroy(gameObject);
        }
    }
}
