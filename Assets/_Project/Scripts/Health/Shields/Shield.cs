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

        /// <summary>
        /// A shield stops any single hit up to its threshold and survives doing so. Anything heavier
        /// overwhelms it - see <see cref="ShieldController.ProcessDamageThroughShields"/>, which still
        /// swallows the hit but consumes the shield.
        /// </summary>
        public bool CanBlockDamage(float damage)
        {
            return IsActive && damage <= DamageThreshold;
        }

        public void Break()
        {
            IsActive = false;
        }
    }
}
