using UnityEngine;

namespace UdderDestruction
{
    internal static class UdderSpriteFacing
    {
        public static void Apply(SpriteRenderer renderer, Vector2 direction, Sprite downSprite, Sprite sideSprite, Sprite upSprite, bool invertSideFlip = false)
        {
            if (!renderer || direction.sqrMagnitude <= 0.0001f)
                return;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                renderer.sprite = sideSprite ? sideSprite : renderer.sprite;
                renderer.flipX = invertSideFlip ? direction.x > 0f : direction.x < 0f;
                return;
            }

            renderer.sprite = direction.y > 0f
                ? FirstAvailable(upSprite, downSprite, sideSprite, renderer.sprite)
                : FirstAvailable(downSprite, upSprite, sideSprite, renderer.sprite);
            renderer.flipX = false;
        }

        private static Sprite FirstAvailable(Sprite preferred, Sprite fallbackA, Sprite fallbackB, Sprite fallbackC)
        {
            if (preferred)
                return preferred;
            if (fallbackA)
                return fallbackA;
            if (fallbackB)
                return fallbackB;
            return fallbackC;
        }
    }
}
