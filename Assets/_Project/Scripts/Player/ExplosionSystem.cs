using System;
using Scripts.Items.StatSystems;
using UnityEngine;

namespace Scripts.Player
{
    public class ExplosionSystem : MonoBehaviour
    {
        public static ExplosionSystem Instance { get; private set; }

        [Header("Defaults")]
        [SerializeField] private float _defaultDamage = 10f;
        [SerializeField] private float _defaultRadius = 2f;
        [SerializeField] private float _defaultKnockback = 5f;

        [Header("References")]
        [SerializeField] private LayerMask _enemyLayerMask;

        private NumericStatModifierSystem _damageModifierSystem = new();
        private NumericStatModifierSystem _radiusModifierSystem = new();
        private NumericStatModifierSystem _knockbackModifierSystem = new();

        public NumericStatModifierSystem DamageModifierSystem => _damageModifierSystem;
        public NumericStatModifierSystem RadiusModifierSystem => _radiusModifierSystem;
        public NumericStatModifierSystem KnockbackModifierSystem => _knockbackModifierSystem;

        public float DefaultDamage => _defaultDamage;
        public float DefaultRadius => _defaultRadius;
        public float DefaultKnockback => _defaultKnockback;
        public LayerMask EnemyLayerMask => _enemyLayerMask;

        public event Action<Vector2, float> OnExplosionCreated;

        private void Awake()
        {
            Instance = this;
        }

        public float GetDamage(float baseDamage)
        {
            return _damageModifierSystem.Calculate(baseDamage);
        }

        public float GetRadius(float baseRadius)
        {
            return _radiusModifierSystem.Calculate(baseRadius);
        }

        public float GetKnockback(float baseKnockback)
        {
            return _knockbackModifierSystem.Calculate(baseKnockback);
        }

        public void NotifyExplosionCreated(Vector2 position, float finalDamage)
        {
            OnExplosionCreated?.Invoke(position, finalDamage);
        }
    }
}
