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
        DairyDouble
    }

    public sealed class UdderPickup : MonoBehaviour
    {
        public PickupType type;
        public int amount = 1;

        private void Update()
        {
            transform.Rotate(0f, 0f, 90f * Time.deltaTime);
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
