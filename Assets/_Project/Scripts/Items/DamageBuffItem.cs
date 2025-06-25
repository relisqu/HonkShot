using Scripts.Health;
using UnityEngine;
using Scripts.Items;

public class DamageBuffItem : MonoBehaviour
{
    public float addValue = 0f; // e.g., +5 damage
    public float multValue = 1f; // e.g., x1.2 damage

    void Start()
    {
        var attack = GetComponentInParent<PlayerAttackController>();
        if (attack != null)
        {
            if (!Mathf.Approximately(addValue, 0f))
                attack.AddDamageModifier(new StatModifier(StatModType.Add, addValue, 0));
            if (!Mathf.Approximately(multValue, 1f))
                attack.AddDamageModifier(new StatModifier(StatModType.Mult, multValue, 0));
        }
    }
}