using System;
using Scripts.Enemies;
using Scripts.Items.StatSystems;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Player;
using UnityEngine;
using Zenject;

namespace Scripts.ScoreSystem
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Difficulty Settings")]
        [SerializeField] private int _minDifficultyRating = 1;
        [SerializeField] private int _maxDifficultyRating = 10;

        [Header("Speed Settings")]
        [SerializeField] private float _minSpeedMultiplier = 1f;
        [SerializeField] private float _maxSpeedMultiplier = 2f;
        [SerializeField] private float _minSpeedThreshold = 5f;
        [SerializeField] private float _maxSpeedThreshold = 15f;

        [Header("Damage Settings")]
        [SerializeField] private float _damageDivisor = 10f;

        [Header("Time Coefficient Settings")]
        [SerializeField] private float _minTimeCoefficient = 1f;
        [SerializeField] private float _maxTimeCoefficient = 1000f;
        [SerializeField] private float _optimalTimeCoefficientMultiplier = 0.5f;
        [SerializeField] private float _optimalRoomClearTime = 10f;
        [SerializeField] private float _maxRoomClearTime = 120f;

        [Header("Score Settings")]
        [SerializeField] private float _minScore = 1f;

        [Header("Debug")]
        [SerializeField] private bool _debugMode = false;

        private long _currentRunScore;
        private float _currentRoomStartTime;
        private float _totalDamageDealt;
        private int _enemiesKilledThisRoom;

        private NumericStatModifierSystem _scoreBonusModifierSystem = new();

        [Inject] private PlayerBallMovement _playerBallMovement;

        public long CurrentRunScore => _currentRunScore;
        public NumericStatModifierSystem ScoreBonusModifierSystem => _scoreBonusModifierSystem;

        public event Action<long> OnScoreChanged;
        public event Action<long, ScoreBreakdown> OnScoreAwarded;
        public event Action<long, DamageScoreBreakdown> OnDamageScoreAwarded;
        public event Action<long, KillScoreBreakdown> OnKillScoreAwarded;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (LevelManager.Instance)
            {
                LevelManager.Instance.EnteredRoom += LevelManager_EnteredRoom;
                LevelManager.Instance.CompletedRoom += LevelManager_CompletedRoom;
            }

            if (!_playerBallMovement)
            {
                _playerBallMovement = FindObjectOfType<PlayerBallMovement>();
            }
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance)
            {
                LevelManager.Instance.EnteredRoom -= LevelManager_EnteredRoom;
                LevelManager.Instance.CompletedRoom -= LevelManager_CompletedRoom;
            }
        }

        private void LevelManager_EnteredRoom()
        {
            _currentRoomStartTime = Time.time;
            _totalDamageDealt = 0f;
            _enemiesKilledThisRoom = 0;
        }

        private void LevelManager_CompletedRoom()
        {
        }

        public void OnDamageDealt(BaseEnemy enemy, float damageDealt)
        {
            if (!enemy) return;

            _totalDamageDealt += damageDealt;

            var breakdown = CalculateDamageScore(damageDealt);
            long score = (long)breakdown.FinalScore;

            _currentRunScore += score;

            if (_debugMode)
            {
                Debug.Log($"[ScoreManager] Damage dealt to: {enemy.EnemyName} | " +
                          $"Damage: {breakdown.DamagePoints:F1} | " +
                          $"Buffs: {breakdown.ItemBuffs:F1} | " +
                          $"Time: {breakdown.TimeCoefficient:F1} | " +
                          $"Final: {score}");
            }

            OnScoreChanged?.Invoke(_currentRunScore);
            OnDamageScoreAwarded?.Invoke(score, breakdown);
        }

        public void OnEnemyKilled(BaseEnemy enemy)
        {
            if (!enemy) return;

            _enemiesKilledThisRoom++;

            var breakdown = CalculateKillScore(enemy.DifficultyRating);
            long score = (long)breakdown.FinalScore;

            _currentRunScore += score;

            if (_debugMode)
            {
                Debug.Log($"[ScoreManager] Enemy killed: {enemy.EnemyName} | " +
                          $"Difficulty: {breakdown.DifficultyPoints} | " +
                          $"Speed: {breakdown.SpeedMultiplier:F2} | " +
                          $"Final: {score}");
            }

            OnScoreChanged?.Invoke(_currentRunScore);
            OnKillScoreAwarded?.Invoke(score, breakdown);
        }

        private DamageScoreBreakdown CalculateDamageScore(float damageDealt)
        {
            var breakdown = new DamageScoreBreakdown();

            breakdown.DamagePoints = damageDealt / _damageDivisor;
            breakdown.ItemBuffs = _scoreBonusModifierSystem.Calculate(0f);
            breakdown.TimeCoefficient = CalculateTimeCoefficient();

            float baseScore = breakdown.DamagePoints + breakdown.ItemBuffs;
            breakdown.FinalScore = Mathf.Max(_minScore, baseScore * breakdown.TimeCoefficient);

            return breakdown;
        }

        private KillScoreBreakdown CalculateKillScore(int difficultyRating)
        {
            var breakdown = new KillScoreBreakdown();

            breakdown.DifficultyPoints = Mathf.Clamp(difficultyRating, _minDifficultyRating, _maxDifficultyRating);
            breakdown.SpeedMultiplier = CalculateSpeedMultiplier();
            breakdown.FinalScore = breakdown.DifficultyPoints * breakdown.SpeedMultiplier;

            return breakdown;
        }

        private float CalculateSpeedMultiplier()
        {
            if (!_playerBallMovement) return _minSpeedMultiplier;

            float currentSpeed = _playerBallMovement.CurrentSpeed;
            float normalizedSpeed = Mathf.InverseLerp(_minSpeedThreshold, _maxSpeedThreshold, currentSpeed);
            return Mathf.Lerp(_minSpeedMultiplier, _maxSpeedMultiplier, normalizedSpeed);
        }

        private float CalculateTimeCoefficient()
        {
            float timeInRoom = Time.time - _currentRoomStartTime;
            float optimalCoefficient = _maxTimeCoefficient * _optimalTimeCoefficientMultiplier;

            if (timeInRoom <= _optimalRoomClearTime)
            {
                float t = timeInRoom / _optimalRoomClearTime;
                return Mathf.Lerp(_maxTimeCoefficient, optimalCoefficient, t);
            }
            else
            {
                float t = Mathf.InverseLerp(_optimalRoomClearTime, _maxRoomClearTime, timeInRoom);
                return Mathf.Lerp(optimalCoefficient, _minTimeCoefficient, t);
            }
        }

        public void ResetRunScore()
        {
            _currentRunScore = 0;
            _totalDamageDealt = 0f;
            _enemiesKilledThisRoom = 0;
            OnScoreChanged?.Invoke(_currentRunScore);
        }

        public void AddScore(long amount)
        {
            _currentRunScore += amount;
            OnScoreChanged?.Invoke(_currentRunScore);
        }

        public float GetCurrentRoomTime()
        {
            return Time.time - _currentRoomStartTime;
        }
    }

    public struct ScoreBreakdown
    {
        public int DifficultyPoints;
        public float SpeedMultiplier;
        public float DamagePoints;
        public float ItemBuffs;
        public float TimeCoefficient;
        public float FinalScore;
    }

    public struct DamageScoreBreakdown
    {
        public float DamagePoints;
        public float ItemBuffs;
        public float TimeCoefficient;
        public float FinalScore;
    }

    public struct KillScoreBreakdown
    {
        public int DifficultyPoints;
        public float SpeedMultiplier;
        public float FinalScore;
    }
}
