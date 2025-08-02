using System.Collections.Generic;
using Scripts.Health;
using UnityEngine;
using Scripts.Items;

public class DamageBuffItem : MonoBehaviour
{
    public List<NumericStatModifier> NumericStatModifiers;

    void Start()
    {
        var attack = GetComponentInParent<PlayerAttackController>();
        if (attack == null) return;

        foreach (var numericStatModifier in NumericStatModifiers)
        {
            attack.NumericStatModifierSystem.AddModifier(numericStatModifier);
        }
    }
}