using System.Collections.Generic;
using Scripts.Enemies;
using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Player.Dash;
using Scripts.Player.Shooting;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.Player.Shooting
{
    public class PlayerFeatherShooter : MonoBehaviour
    {
        //pool
        [Header("Pool")]
        [SerializeField] private PlayerFeather _featherPrefab;
        [SerializeField] private int _poolSize = 20;
        [SerializeField] private Transform _poolParent;
        
        private ComponentPool<PlayerFeather> _featherPool;
        
        [Header("Shoot")]
        [SerializeField] private float _featherSize = 1f;
        [SerializeField] private float _featherSpeed = 12f;
        [SerializeField] private float _featherDamage = 8f;
        
        public static event System.Action<Vector2> OnShoot;
        
        #region stat modifiers
        private readonly NumericStatModifierSystem _sizeModifierSystem = new();
        private readonly NumericStatModifierSystem _damageModifierSystem = new();
        private readonly NumericStatModifierSystem _speedModifierSystem = new();

        public NumericStatModifierSystem SizeModifierSystem => _sizeModifierSystem;
        public NumericStatModifierSystem DamageModifierSystem => _damageModifierSystem;
        public NumericStatModifierSystem SpeedModifierSystem => _speedModifierSystem;
        
        public float ModFeatherSize => SizeModifierSystem.Calculate(_featherSize);
        public float ModFeatherDamage => DamageModifierSystem.Calculate(_featherDamage);
        public float ModFeatherSpeed => SpeedModifierSystem.Calculate(_featherSpeed);
        #endregion
        private void Start()
        {
            if (!_poolParent)
            {
                GameObject poolContainer = new GameObject($"featherPool");
                _poolParent = poolContainer.transform;
            }

            _featherPool = new ComponentPool<PlayerFeather>(_featherPrefab, _poolSize, _poolParent);
            _featherPool.Initialize(_poolSize);
        }
        //method - shoot 
        public void Shoot(Vector2 direction)
        {
            OnShoot?.Invoke(direction);
            if (direction == Vector2.zero)
                return;

            direction = direction.normalized;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

            var feather = _featherPool.Get(transform.position, rotation);
            feather.SetParameters(ModFeatherSpeed, ModFeatherDamage, ModFeatherSize);
            Debug.Log("SHOOOT");
        }
        //method - shoot at
        public void ShootAt(Vector2 targetPosition)
        {
            if (targetPosition == Vector2.zero) return;
            
            Vector2 direction = targetPosition - (Vector2)transform.position;
            Shoot(direction);
        }
        
    }
}