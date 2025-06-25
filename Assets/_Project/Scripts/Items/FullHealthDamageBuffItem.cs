using UnityEngine;
using Scripts.Health;
using Scripts.Items;

public class FullHealthDamageBuffItem : MonoBehaviour
{
    public float multValue = 2f; // e.g., x2 damage at full health
    private StatModifier _activeModifier;
    private PlayerAttackController _attack;
    private HealthController _health;

    void Start()
    {
        _attack = GetComponentInParent<PlayerAttackController>();
        _health = GetComponentInParent<HealthController>();
        if (_health != null)
        {
            _health.OnTakeDamageTriggered += CheckBuff;
        }
        CheckBuff();
    }

    void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnTakeDamageTriggered -= CheckBuff;
        }
        RemoveBuff();
    }

    private void CheckBuff()
    {
        if (_health && _attack)
        {
            if (_health.RemainingHealthPercentage >= 1f)
            {
                if (_activeModifier == null)
                {
                    _activeModifier = new StatModifier(StatModType.Mult, multValue, 0);
                    _attack.AddDamageModifier(_activeModifier);
                }
            }
            else
            {
                RemoveBuff();
            }
        }
    }

    private void RemoveBuff()
    {
        if (_activeModifier != null && _attack != null)
        {
            _attack.RemoveDamageModifier(_activeModifier);
            _activeModifier = null;
        }
    }
} 