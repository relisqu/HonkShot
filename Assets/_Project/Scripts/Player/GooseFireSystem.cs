using System;
using System.Collections.Generic;
using Scripts.Enemies;
using Scripts.Health;
using Scripts.Items;
using Scripts.Items.StatSystems;
using Scripts.Player.InputHandling;
using Scripts.ScoreSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Scripts.Player
{
    public class GooseFireSystem : MonoBehaviour
    {
        public static GooseFireSystem Instance { get; private set; }

        [Inject] private InputHandler _inputHandler;
        [SerializeField] private PlayerAttackController _playerAttackController;

        [FormerlySerializedAs("_ultimateMultiplySystem")]
        [Header("Fire Settings")]
        [SerializeField] private float _ultimateMultiplyCoefficient = 5f;
        [SerializeField] private float _ultimateDragCoefficient = 0f;
        [SerializeField] private float _maxFire = 100f;
        [SerializeField] private float _fireLossPerSecond = 5f;
        [SerializeField] private float _ultimateFireDrainPerSecond = 20f;
        [SerializeField] private float _honkFireDrainPerSecond = 30f;
        [SerializeField] private KeyCode _honkKey = KeyCode.Space;
        [SerializeField] private PlayerStatus _playerStatus;

        [Header("Fire Gain Settings")]
        [SerializeField] private float _fireGainPerDash = 5f;
        [SerializeField] private float _fireGainPerKill = 20f;
        [SerializeField] private float _damageToFireDivisor = 5f;
        [SerializeField] private float _minEnvironmentFireGain = 1f;
        [SerializeField] private float _maxEnvironmentFireGain = 10f;
        [SerializeField] private int _maxEnvironmentDifficulty = 10;

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

        private NumericStatModifierSystem _fireGainModifierSystem = new();
        public NumericStatModifierSystem FireGainModifierSystem => _fireGainModifierSystem;

        public float Fire => _fire;
        public bool IsUltimate => _isUltimate;
        public bool IsHonk => _isHonk;
        public float MaxFire => _maxFire;

        private PlayerBallMovement _playerBallMovement;

        private void Awake()
        {
            Instance = this;

            if (!_playerBallMovement)
                _playerBallMovement = GetComponent<PlayerBallMovement>();
            _fire = 0f;
            if (_playerStatus)
                _playerStatus = GetComponent<PlayerStatus>();
            if (_playerStatus)
                _healthController = _playerStatus.GetHealthController();
        }

        private void Start()
        {
            if (ScoreManager.Instance)
            {
                ScoreManager.Instance.OnDamageScoreAwarded += ScoreManager_OnDamageScoreAwarded;
                ScoreManager.Instance.OnKillScoreAwarded += ScoreManager_OnKillScoreAwarded;
            }
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance)
            {
                ScoreManager.Instance.OnDamageScoreAwarded -= ScoreManager_OnDamageScoreAwarded;
                ScoreManager.Instance.OnKillScoreAwarded -= ScoreManager_OnKillScoreAwarded;
            }
        }

        private void ScoreManager_OnDamageScoreAwarded(long score, DamageScoreBreakdown breakdown)
        {
            float damageDealt = breakdown.DamagePoints * 10f;
            OnDamageDealt(damageDealt);
        }

        private void ScoreManager_OnKillScoreAwarded(long score, KillScoreBreakdown breakdown)
        {
            OnEnemyKilled();
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

            if (_isUltimate)
            {
                StopHonk();
                ChangeFire(-_ultimateFireDrainPerSecond * Time.deltaTime, false);
                if (_fire <= 0)
                {
                    EndUltimate();
                }
            }
            else if (_isHonk)
            {
                ChangeFire(-_honkFireDrainPerSecond * Time.deltaTime, false);
                if (_fire <= 0)
                {
                    StopHonk();
                }
            }
            else
            {
                ChangeFire(-_fireLossPerSecond * Time.deltaTime, false);
            }

        }


        public void OnDashUsed()
        {
            if (_isHonk || _isUltimate) return;
            float fireGain = ApplyFireGainModifiers(_fireGainPerDash);
            ChangeFire(fireGain);
        }

        public void OnDamageDealt(float damage)
        {
            if (_isHonk || _isUltimate) return;
            float fireGain = damage / _damageToFireDivisor;
            fireGain = ApplyFireGainModifiers(fireGain);
            ChangeFire(fireGain);
        }

        public void OnEnemyKilled()
        {
            if (_isHonk || _isUltimate) return;
            float fireGain = ApplyFireGainModifiers(_fireGainPerKill);
            ChangeFire(fireGain);
        }

        public void OnEnvironmentInteraction(IFireInteractable interactable)
        {
            if (_isHonk || _isUltimate) return;

            float fireGain = CalculateEnvironmentFireGain(interactable.DifficultyLevel);
            fireGain = ApplyFireGainModifiers(fireGain);
            ChangeFire(fireGain);
        }
        public void OnEnvironmentInteraction(IFireInteractable interactable, int difficultyLevel)
        {
            if (_isHonk || _isUltimate) return;

            float fireGain = CalculateEnvironmentFireGain(difficultyLevel);
            fireGain = ApplyFireGainModifiers(fireGain);
            ChangeFire(fireGain);
        }


        private float CalculateEnvironmentFireGain(int difficultyLevel)
        {
            int clampedDifficulty = Mathf.Clamp(difficultyLevel, 1, _maxEnvironmentDifficulty);
            float t = (float)(clampedDifficulty - 1) / (_maxEnvironmentDifficulty - 1);
            return Mathf.Lerp(_minEnvironmentFireGain, _maxEnvironmentFireGain, t);
        }

        private float ApplyFireGainModifiers(float baseGain)
        {
            return _fireGainModifierSystem.Calculate(baseGain);
        }

        [Obsolete("Use OnDashUsed() instead")]
        public void OnLaunchOrAcceleration()
        {
            OnDashUsed();
        }

        [Obsolete("Use OnDashUsed() instead")]
        public void OnAcceleration()
        {
            OnDashUsed();
        }

        [Obsolete("No longer used in new fire system")]
        public void OnDeceleration()
        {
        }

        public void AddFire(float amount)
        {
            ChangeFire(amount);
        }

        private void ChangeFire(float amount, bool applyModifiers = true)
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
            if (!_isUltimate) return;
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
            if (!_isHonk) return;
            _isHonk = false;
            if (_healthController)
                _healthController.SetInvincible((int)InvincibilityEnum.HonkMode, false);
            HonkEnded?.Invoke();
        }

        public void ResetStats()
        {
            EndUltimate();
            StopHonk();
            _fire = 0;
        }
    }
}
