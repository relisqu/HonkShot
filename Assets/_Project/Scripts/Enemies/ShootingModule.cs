using System;
using Scripts.Enemies.Bullets;
using Scripts.PointSystem;
using UnityEngine;
using Zenject;

namespace Scripts.Enemies
{
    public abstract class ShootingModule : MonoBehaviour
    {
        [SerializeField] protected GameObject _bulletPrefab;
        [SerializeField] private bool _hasMaxDistance;
        [SerializeField] protected float _shootDistance = 5f;

        [Inject] private PointReceiver _pointReceiver;
        
        protected Transform _player;

        protected virtual void Start()
        {
            var pointReceiver = _pointReceiver ?? PointReceiver.Instance; // Fallback to Instance if injection failed
            if (pointReceiver != null)
            {
                _player = pointReceiver.transform;
            }
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