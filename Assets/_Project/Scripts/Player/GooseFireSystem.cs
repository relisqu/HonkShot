using System;
using UnityEngine;

namespace Scripts.Player
{
    public class GooseFireSystem : MonoBehaviour
    {
        [Header("Fire Settings")]
        [SerializeField] private float _ultimateMultiplySystem = 5f;
        [SerializeField] private float _maxFire = 100f;
        [SerializeField] private float _fireGainPerLaunch = 20f;
        [SerializeField] private float _fireGainPerAcceleration = 5f;
        [SerializeField] private float _fireLossPerDeceleration = 10f;
        [SerializeField] private float _fireLossPerSecond = 5f;
        [SerializeField] private float _ultimateFireDrainPerSecond = 20f;
        [SerializeField] private float _honkFireDrainPerSecond = 30f;
        [SerializeField] private KeyCode _honkKey = KeyCode.Space;
        [SerializeField] private PlayerStatus _playerStatus;

        public event Action<float> FireChanged;
        public event Action UltimateStarted;
        public event Action UltimateEnded;
        public event Action HonkStarted;
        public event Action HonkEnded;

        private float _fire;
        private bool _isUltimate;
        private bool _isHonk;
        private bool _canGainFire = true;
        private Scripts.Health.HealthController _healthController;

        public float Fire => _fire;
        public bool IsUltimate => _isUltimate;
        public bool IsHonk => _isHonk;
        public float MaxFire => _maxFire;
        private void Awake()
        {
            _fire = 0f;
            if (_playerStatus == null)
                _playerStatus = GetComponent<PlayerStatus>();
            if (_playerStatus != null)
                _healthController = _playerStatus.GetHealthController();
        }

        private void Update()
        {
            // Handle Honk input
            if (Input.GetKeyDown(_honkKey) && !_isHonk && _fire > 0)
            {
                StartHonk();
            }
            if ((Input.GetKeyUp(_honkKey) && _isHonk) || (_isHonk && _fire <= 0))
            {
                StopHonk();
            }

            // Fire decay
            if (_isUltimate)
            {
                StopHonk();
                Debug.Log("ULTING");
                ChangeFire(-_ultimateFireDrainPerSecond * Time.deltaTime);
                if (_fire <= 0)
                {
                    EndUltimate();
                }
            }
            else if (_isHonk)
            {
                Debug.Log("Honking");
                ChangeFire(-_honkFireDrainPerSecond * Time.deltaTime);
                if (_fire <= 0)
                {
                    StopHonk();
                }
            }
            else
            {
                Debug.Log("small default fire drain");
                ChangeFire(-_fireLossPerSecond * Time.deltaTime);
            }
        }

        public void OnLaunchOrAcceleration()
        {
            if (_isHonk) return; // No fire gain during honk
            if (_isUltimate) return; // No fire gain during ultimate
            ChangeFire(_fireGainPerLaunch);
        }

        public void OnAcceleration()
        {
            if (_isHonk) return;
            if (_isUltimate) return;
            ChangeFire(_fireGainPerAcceleration);
        }

        public void OnDeceleration()
        {
            if (_isHonk) return;
            if (_isUltimate) return;
            ChangeFire(-_fireLossPerDeceleration);
        }

        private void ChangeFire(float amount)
        {
            float prev = _fire;
            _fire = Mathf.Clamp(_fire + amount, 0, _maxFire);
            Debug.Log($"Fire: {_fire}");
            if (!_isUltimate && _fire >= _maxFire)
            {
                StartUltimate();
            }
            FireChanged?.Invoke(_fire);
        }

        private void StartUltimate()
        {
            Debug.Log("ULT TIME");
            _isUltimate = true;
            if (Scripts.PointSystem.PointReceiver.Instance )
                Scripts.PointSystem.PointReceiver.Instance.AddMultiplier("ultimate", _ultimateMultiplySystem);
            UltimateStarted?.Invoke();
        }

        private void EndUltimate()
        {
            _isUltimate = false;
            if (Scripts.PointSystem.PointReceiver.Instance)
                Scripts.PointSystem.PointReceiver.Instance.RemoveMultiplier("ultimate");
            UltimateEnded?.Invoke();
        }

        private void StartHonk()
        {
            _isHonk = true;
            if (_healthController)
                _healthController.SetInvincible(true);
            HonkStarted?.Invoke();
        }

        private void StopHonk()
        {
            _isHonk = false;
            if (_healthController)
                _healthController.SetInvincible(false);
            HonkEnded?.Invoke();
        }
    }
} 