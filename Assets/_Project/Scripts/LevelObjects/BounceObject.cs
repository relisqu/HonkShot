using UnityEngine;

namespace Scripts.LevelObjects
{
    public class BounceObject : MonoBehaviour
    {
        [SerializeField] private float _bounciness;
        public float Bounciness => _bounciness;
    }
}