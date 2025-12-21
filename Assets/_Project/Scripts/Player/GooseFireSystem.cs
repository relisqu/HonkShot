using System;
using Scripts.Health;
using Scripts.Items;
using Scripts.Items.StatSystems;
using Scripts.Player.InputHandling;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Scripts.Player
{
    public class GooseFireSystem : MonoBehaviour
    {
        [Inject] private InputHandler _inputHandler;

        [FormerlySerializedAs("_ultimateMultiplySystem")] [Header("Fire Settings")] [SerializeField]
        private float _ultimateMultiplyCoefficient = 5f;

        [SerializeField] private float _ultimateDragCoefficient = 0f;

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
        private HealthController _healthController;

        private NumericStatModifierSystem _playerPointReceiveModifierSystem =>
            PointSystem.PointReceiver.Instance.PointReceiveModifierSystem;

        public float Fire => _fire;
        public bool IsUltimate => _isUltimate;
        public bool IsHonk => _isHonk;
        public float MaxFire => _maxFire;

        private PlayerBallMovement _playerBallMovement;

        private void Awake()
        {
            if (_playerBallMovement == null)
                _playerBallMovement = GetComponent<PlayerBallMovement>();
            _fire = 0f;
            if (_playerStatus == null)
                _playerStatus = GetComponent<PlayerStatus>();
            if (_playerStatus != null)
                _healthController = _playerStatus.GetHealthController();
        }

        private void Update()
        {
            if (_inputHandler.IsInputEnabled && Input.GetKeyDown(_honkKey) && !_isHonk && _fire > 0)
            {
                StartHonk();
            }

            if (_inputHandler.IsInputEnabled && (Input.GetKeyUp(_honkKey) && _isHonk) || (_isHonk && _fire <= 0))
            {
                StopHonk();
            }

            // Fire decay
            if (_isUltimate)
            {
                StopHonk();
                ChangeFire(-_ultimateFireDrainPerSecond * Time.deltaTime);
                if (_fire <= 0)
                {
                    EndUltimate();
                }
            }
            else if (_isHonk)
            {
                ChangeFire(-_honkFireDrainPerSecond * Time.deltaTime);
                if (_fire <= 0)
                {
                    StopHonk();
                }
            }
            else
            {
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
            if (!_isUltimate && _fire >= _maxFire)
            {
                StartUltimate();
            }

            FireChanged?.Invoke(_fire);
        }


        private void StartUltimate()
        {
            _isUltimate = true;
            if (PointSystem.PointReceiver.Instance)
            {
                Debug.Log(_playerPointReceiveModifierSystem == null);

                _playerBallMovement.DragForceModifierSystem.AddModifier(
                    new NumericStatModifier((int)ModifierTypeEnum.HonkMode, NumericModType.Mult,
                        _ultimateDragCoefficient,
                        _playerBallMovement.DragForceModifierSystem.GetLastOrder() + 1));
                _playerPointReceiveModifierSystem.AddModifier(
                    new NumericStatModifier((int)ModifierTypeEnum.HonkMode, NumericModType.Mult,
                        _ultimateMultiplyCoefficient,
                        _playerPointReceiveModifierSystem.GetLastOrder() + 1));
                UltimateStarted?.Invoke();
            }
        }

        private void EndUltimate()
        {
            _isUltimate = false;
            if (PointSystem.PointReceiver.Instance)
                _playerPointReceiveModifierSystem.RemoveModifier((int)ModifierTypeEnum.HonkMode);
            _playerBallMovement.DragForceModifierSystem.RemoveModifier((int)ModifierTypeEnum.HonkMode);
            UltimateEnded?.Invoke();
        }

        private void StartHonk()
        {
            _isHonk = true;
            if (_healthController)
                _healthController.SetInvincible((int)InvincibilityEnum.HonkMode, true);
            HonkStarted?.Invoke();
        }

        private void StopHonk()
        {
            _isHonk = false;
            if (_healthController)
                _healthController.SetInvincible((int)InvincibilityEnum.HonkMode, false);
            HonkEnded?.Invoke();
        }
    }
}