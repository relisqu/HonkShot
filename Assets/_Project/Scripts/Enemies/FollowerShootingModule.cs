using System;
using Scripts.Enemies.Bullets;
using UnityEngine;

namespace Scripts.Enemies
{
    public class FollowerShootingModule : ShootingModule
    {
        protected override void Shoot(Action onShootFinish)
        {
            if (!_player) return;

            Vector2 direction = (_player.position - transform.position).normalized;
            var bullet = Instantiate(_bulletPrefab, transform.position,
                Quaternion.LookRotation(Vector3.forward, direction));
            if (!bullet.TryGetComponent(out BaseBullet bulletComponent))
            {
                var bulletBullet = bullet.GetComponentInChildren<BaseBullet>();
                bulletBullet.OnReady += onShootFinish;
            }
            else
            {
                
                bulletComponent.OnReady += onShootFinish;
            }
        }
    }
}