using System.Collections.Generic;
using UnityEngine;
using Scripts.Items;
using Scripts.Health;

public class GlassCannonItem : MonoBehaviour
{
    public List<NumericStatModifier> DamageStatModifiers;
    public List<NumericStatModifier> HealthStatModifiers;

    void Start()
    {
        var attackController = GetComponentInParent<PlayerAttackController>();
        if (attackController)
        {
            foreach (var damageStatModifier in DamageStatModifiers)
            {
                damageStatModifier.SetOrder(attackController.NumericStatModifierSystem.GetLastOrder() + 1);
                attackController.NumericStatModifierSystem.AddModifier(damageStatModifier);
            }
        }

        var health = GetComponentInParent<HealthController>();
        if (health)
        {
            foreach (var healthStatModifier in HealthStatModifiers)
            {
                healthStatModifier.SetOrder(attackController.NumericStatModifierSystem.GetLastOrder() + 1);
                health.DamageTakenModifierSystem.AddModifier(healthStatModifier);
            }
        }
    }
}