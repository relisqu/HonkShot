using UnityEngine;

namespace Scripts.Bullets
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float _speed = 10f; 
        [SerializeField]private Rigidbody2D _rigidbody;
        
        public void Launch(Vector2 direction)
        {
            _rigidbody.AddForce(direction * _speed, ForceMode2D.Impulse);
        }
    }
}