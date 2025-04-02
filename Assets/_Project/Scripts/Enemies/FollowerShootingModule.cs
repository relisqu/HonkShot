using UnityEngine;

namespace Scripts.Enemies
{
    public class FollowerShootingModule : ShootingModule
    {
        protected override void Shoot()
        {
            if (!_player) return;

            Vector2 direction = (_player.position - transform.position).normalized;
            Instantiate(_bulletPrefab, transform.position, Quaternion.LookRotation(Vector3.forward, direction));
        }
    }
}