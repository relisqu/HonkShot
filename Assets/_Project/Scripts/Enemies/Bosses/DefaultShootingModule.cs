using System;
using Scripts.Enemies.Bullets;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Enemies.Bosses
{
    public class DefaultShootingModule : ShootingModule
    {
        private float _bulletSpeed;
        private float _bulletDamage;
        private float _predictionCoefficient;

        private PlayerBallMovement _playerBallMovement;

        public void Start()
        {
            base.Start();
            _playerBallMovement = _player.GetComponent<PlayerBallMovement>();
        }

        public void SetParameters(float bulletSpeed, float bulletDamage, float predictionCoefficient = 1f)
        {
            _predictionCoefficient = predictionCoefficient;
            _bulletSpeed = bulletSpeed;
            _bulletDamage = bulletDamage;
        }

        protected override void Shoot(Action onShootFinish)
        {
            if (!_player) return;

            var predictedPosition =
                (_playerBallMovement.transform.position + (Vector3)_playerBallMovement.CurrentMovement *
                    (_predictionCoefficient / _bulletSpeed));
            Vector2 direction = (predictedPosition - transform.position).normalized;
            var bullet = Instantiate(_bulletPrefab, transform.position,
                Quaternion.LookRotation(Vector3.forward, direction));
            if (!bullet.TryGetComponent(out BaseBullet bulletComponent))
            {
                var bulletBullet = bullet.GetComponentInChildren<BaseBullet>();
                bulletBullet.SetParameters(_bulletSpeed, _bulletDamage);
                bulletBullet.OnReady += onShootFinish;
            }
            else
            {
                bulletComponent.OnReady += onShootFinish;
            }
        }
    }
}