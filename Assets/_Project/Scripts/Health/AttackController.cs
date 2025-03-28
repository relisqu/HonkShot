using UnityEngine;

namespace Scripts.Health
{
    public class AttackController : MonoBehaviour
    {
        public virtual float GetDamage()
        {
            return -1;
        }
    }
}