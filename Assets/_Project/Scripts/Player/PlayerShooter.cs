using System;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.Audio;
using Scripts.Bullets;
using Scripts.Camera;
using Scripts.Player.Stamina;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Player
{
    public class PlayerShooter : MonoBehaviour
    {
        [Header("Shooting Settings")] public Bullet bulletPrefab;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private PlayerStatus _playerStatus;
        public Transform shootingPoint;

        [FormerlySerializedAs("maxBullets")] [SerializeField]
        private int _maxBulletCount = 5;

        [SerializeField] private float _shootDelay = 0.5f;

        private Queue<Bullet> activeBullets = new Queue<Bullet>();
        private float _lastShootTime;
        private UnityEngine.Camera _camera;

        void Update()
        {
            if (_playerStatus.PlayerState != PlayerState.Shooter) return;

            if (Input.GetMouseButton(0) && Time.time >= _lastShootTime + _shootDelay)
            {
                if (_staminaManager.TrySpendStamina(5f))
                {
                    Shoot();
                    _lastShootTime = Time.time;
                }
                else
                {
                    CameraShakeHandler.Instance.ShakeCamera(0.3f, 5f);
                }
            }
        }

        private void Awake()
        {
            _camera = FindObjectOfType<UnityEngine.Camera>();
        }

        void Shoot()
        {
            if (activeBullets.Count >= _maxBulletCount)
            {
                Bullet oldestBullet = activeBullets.Dequeue();
                DestroyBullet(oldestBullet);
            }

            var mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            var shootingDirection = (mousePosition - transform.position).normalized;

            var position = transform.position + shootingDirection * 0.3f;
            var newBullet = Instantiate(bulletPrefab, position,
                shootingPoint.rotation);

            AudioManager.Instance.PlayOneShot(SoundChanelType.Player, "shoot");
            if (newBullet != null)
            {
                newBullet.Launch(shootingDirection);
            }

            activeBullets.Enqueue(newBullet);
        }

        void DestroyBullet(Bullet bullet)
        {
            bullet.transform.DOScale(Vector3.zero, 0.3f).OnComplete(() => { Destroy(bullet.gameObject); });
        }
    }
}