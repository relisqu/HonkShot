using System;
using Scripts.Enemies.Bullets;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Enemies.Bosses
{
    public enum BossAttackType
    {
        Single,
        Spread
    }

    public class RavageBossShootingModule : ShootingModule
    {
        [Header("Spread Settings")]
        [SerializeField] private int _spreadBulletCount = 3;
        [SerializeField] private float _spreadAngle = 30f;

        private float _bulletSpeed;
        private float _bulletDamage;
        private float _predictionCoefficient;
        private BossAttackType _currentAttackType = BossAttackType.Single;

        private PlayerBallMovement _playerBallMovement;

        public event Action OnShot;

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

        public void SetAttackType(BossAttackType attackType)
        {
            _currentAttackType = attackType;
        }

        protected override void Shoot(Action onShootFinish)
        {
            if (!_player) return;

            OnShot?.Invoke();

            var predictedPosition =
                (_playerBallMovement.transform.position + (Vector3)_playerBallMovement.CurrentMovement *
                    (_predictionCoefficient / _bulletSpeed));
            Vector2 baseDirection = (predictedPosition - transform.position).normalized;

            if (_currentAttackType == BossAttackType.Spread)
            {
                ShootSpread(baseDirection, onShootFinish);
            }
            else
            {
                ShootSingle(baseDirection, onShootFinish);
            }
        }

        private void ShootSingle(Vector2 direction, Action onShootFinish)
        {
            SpawnBullet(direction, onShootFinish);
        }

        private void ShootSpread(Vector2 baseDirection, Action onShootFinish)
        {
            float angleStep = _spreadAngle / (_spreadBulletCount - 1);
            float startAngle = -_spreadAngle / 2f;

            for (int i = 0; i < _spreadBulletCount; i++)
            {
                float angle = startAngle + (angleStep * i);
                Vector2 direction = Quaternion.Euler(0, 0, angle) * baseDirection;

                bool isLastBullet = (i == _spreadBulletCount - 1);
                SpawnBullet(direction, isLastBullet ? onShootFinish : null);
            }
        }

        private void SpawnBullet(Vector2 direction, Action onShootFinish)
        {
            var bullet = Instantiate(_bulletPrefab, transform.position,
                Quaternion.LookRotation(Vector3.forward, direction));

            if (!bullet.TryGetComponent(out BaseBullet bulletComponent))
            {
                var bulletBullet = bullet.GetComponentInChildren<BaseBullet>();
                bulletBullet.SetParameters(_bulletSpeed, _bulletDamage);
                if (onShootFinish != null)
                    bulletBullet.OnReady += onShootFinish;
            }
            else
            {
                bulletComponent.SetParameters(_bulletSpeed, _bulletDamage);
                if (onShootFinish != null)
                    bulletComponent.OnReady += onShootFinish;
            }
        }
    }
}
