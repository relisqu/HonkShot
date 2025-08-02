using System.Collections.Generic;
using UnityEngine;
using Scripts.Health;
using Scripts.Items;

public class FullHealthDamageBuffItem : MonoBehaviour
{
    public List<NumericStatModifier> NumericalStatModifiers;

    private PlayerAttackController _attackController;
    private HealthController _healthController;

    void Start()
    {
        _attackController = GetComponentInParent<PlayerAttackController>();
        _healthController = GetComponentInParent<HealthController>();
        if (_healthController != null)
        {
            _healthController.OnTakeDamageTriggered += CheckBuff;
        }

        CheckBuff();
    }

    void OnDestroy()
    {
        if (_healthController != null)
        {
            _healthController.OnTakeDamageTriggered -= CheckBuff;
        }

        RemoveBuffs();
    }

    private void CheckBuff()
    {
        if (_healthController && _attackController)
        {
            if (_healthController.RemainingHealthPercentage >= 1f)
            {
                foreach (var statModifier in NumericalStatModifiers)
                {
                    if (_attackController.NumericStatModifierSystem.GetModifier(statModifier.Id) == null)
                    {
                        statModifier.SetOrder(_attackController.NumericStatModifierSystem.GetLastOrder());
                        _attackController.NumericStatModifierSystem.AddModifier(statModifier);
                    }
                }
            }
            else
            {
                RemoveBuffs();
            }
        }
    }

    private void AddBuffs()
    {
        foreach (var statModifier in NumericalStatModifiers)
        {
            if (_attackController.NumericStatModifierSystem.GetModifier(statModifier.Id) == null)
            {
                statModifier.SetOrder(_attackController.NumericStatModifierSystem.GetLastOrder());
                _attackController.NumericStatModifierSystem.AddModifier(statModifier);
            }
        }
    }

    private void RemoveBuffs()
    {
        foreach (var statModifier in NumericalStatModifiers)
        {
            _attackController.NumericStatModifierSystem.RemoveModifier(statModifier.Id);
        }
    }
}