using UnityEngine;

namespace Scripts.Health
{
    // Where a hit came from. default(DamageSource) means "unknown source": no directional reaction.
    public readonly struct DamageSource
    {
        public readonly Vector2 Position;
        public readonly bool HasPosition;

        // Multiplier for the hit sprite's visual kick away from the source (1 = normal hit).
        // Purely cosmetic; it doesn't push the target's body.
        public readonly float KickScale;

        private DamageSource(Vector2 position, float kickScale)
        {
            Position = position;
            KickScale = kickScale;
            HasPosition = true;
        }

        public static DamageSource At(Vector2 position, float kickScale = 1f)
        {
            return new DamageSource(position, kickScale);
        }
    }
}
