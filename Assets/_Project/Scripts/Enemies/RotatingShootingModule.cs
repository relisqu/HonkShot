namespace Scripts.Enemies
{
    using UnityEngine;

    namespace Scripts.Enemies
    {
        public class RotatedShootingModule : ShootingModule
        {
            [SerializeField] private int _bulletCount = 8;
            [SerializeField] private float _startRotation = 0f;

            protected override void Shoot()
            {
                float angleStep = 360f / _bulletCount;
                float angle = _startRotation;

                for (int i = 0; i < _bulletCount; i++)
                {
                    Quaternion rotation = Quaternion.Euler(0, 0, angle);
                    Instantiate(_bulletPrefab, transform.position, rotation);
                    angle += angleStep;
                }
            }

            private void OnDrawGizmosSelected()
            {
                Gizmos.color = Color.cyan;

                float angleStep = 360f / _bulletCount;
                float angle = _startRotation;
                float radius = 2f; // Adjust this for better visualization

                for (int i = 0; i < _bulletCount; i++)
                {
                    Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
                    Vector3 endPoint = transform.position + direction * radius;
                    Gizmos.DrawLine(transform.position, endPoint);
                    angle += angleStep;
                }
            }
        }
    }
}