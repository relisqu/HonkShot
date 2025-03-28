using Scripts.PointSystem;

namespace Scripts.Health
{
    public class PlayerAttackController : AttackController
    {
        public override float GetDamage()
        {
            return PointReceiver.Instance.GetAttackPoints();
        }
    }
}