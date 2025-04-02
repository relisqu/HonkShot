using System;
using Scripts.PointSystem;
using UnityEngine;

namespace Scripts.Enemies
{
    public abstract class ShootingModule : MonoBehaviour
    {
        [SerializeField] protected GameObject _bulletPrefab;
        [SerializeField] private bool _hasMaxDistance;
        [SerializeField] protected float _shootDistance = 5f;

        protected Transform _player;

        protected virtual void Start()
        {
            _player = PointReceiver.Instance.transform;
        }

        public void TryShoot()
        {
            if (_player == null) return;

            float distance = Vector2.Distance(transform.position, _player.position);
            if (!_hasMaxDistance || distance <= _shootDistance)
            {
                Shoot();
            }
        }

        protected abstract void Shoot();

        private void OnDrawGizmos()
        {
            if (!_hasMaxDistance) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _shootDistance);
        }
    }
}