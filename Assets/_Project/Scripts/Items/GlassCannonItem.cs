using UnityEngine;
using Scripts.Items;
using Scripts.Health;

public class GlassCannonItem : MonoBehaviour
{
    public float addValue = 0f; // e.g., +5 damage
    public float multValue = 1.5f; // e.g., x1.5 damage
    public float damageTakenMult = 1.5f; // e.g., x1.5 damage taken

    void Start()
    {
        var attack = GetComponentInParent<PlayerAttackController>();
        if (attack)
        {
            if (addValue != 0f)
                attack.AddDamageModifier(new StatModifier(StatModType.Add, addValue, 0));
            if (!Mathf.Approximately(multValue, 1f))
                attack.AddDamageModifier(new StatModifier(StatModType.Mult, multValue, 0));
        }

        var health = GetComponentInParent<HealthController>();
        if (health)
        {
            health.AddDamageTakenModifier(new StatModifier(StatModType.Mult, damageTakenMult, 0));
        }
    }
}