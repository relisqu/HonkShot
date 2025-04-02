using System;
using Scripts.Enemies.Bullets;
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

        public void TryShoot(Action onShootFinish)
        {
            Debug.Log("try");
            if (_player == null)
            {
                Debug.Log("null");
                onShootFinish?.Invoke();
            }

            float distance = Vector2.Distance(transform.position, _player.position);
            if (!_hasMaxDistance || distance <= _shootDistance)
            {
                Debug.Log("Shoot");
                Shoot(onShootFinish);
            }
            else
            {
                Debug.Log("distance <= _shootDistance");
                onShootFinish?.Invoke();
            }
        }

        protected abstract void Shoot(Action onShootFinish);

        private void OnDrawGizmos()
        {
            if (!_hasMaxDistance) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _shootDistance);
        }
    }
}