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

        public Action OnWindUpStarted;
        public Action OnWindUpCancelled;
        // Direction of the shot, or Vector2.zero for radial bursts.
        public Action<Vector2> OnFired;

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
            if (!_player)
            {
                onShootFinish?.Invoke();
                return;
            }

            float distance = Vector2.Distance(transform.position, _player.position);
            if (!_hasMaxDistance || distance <= _shootDistance)
                Shoot(onShootFinish);
            else
                onShootFinish?.Invoke();
        }

        // Called by the owning enemy when its shooting wind-up (animation) begins.
        public void NotifyWindUpStarted()
        {
            OnWindUpStarted?.Invoke();
        }

        public void NotifyWindUpCancelled()
        {
            OnWindUpCancelled?.Invoke();
        }

        protected abstract void Shoot(Action onShootFinish);

        protected void RaiseFired(Vector2 direction)
        {
            OnFired?.Invoke(direction);
        }

        private void OnDrawGizmos()
        {
            if (!_hasMaxDistance) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _shootDistance);
        }
    }
}