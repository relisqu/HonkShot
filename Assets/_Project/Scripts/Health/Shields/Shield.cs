using System;

namespace Scripts.Health
{
    [Serializable]
    public class Shield
    {
        public float DamageThreshold;
        public bool IsActive;

        public Shield(float damageThreshold)
        {
            DamageThreshold = damageThreshold;
            IsActive = true;
        }

        public bool CanBlockDamage(float damage)
        {
            return IsActive && damage <= DamageThreshold;
        }

        public float AbsorbDamage(float damage)
        {
            if (!CanBlockDamage(damage))
            {
                IsActive = false;
                return damage;
            }

            return 0f;
        }
    }
}
