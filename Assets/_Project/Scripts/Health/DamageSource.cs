using UnityEngine;

namespace Scripts.Health
{
    public readonly struct DamageSource
    {
        public readonly Vector2 Position;
        public readonly bool HasPosition;
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
