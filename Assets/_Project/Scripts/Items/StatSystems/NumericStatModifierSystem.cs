using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace Scripts.Items
{
    public class NumericStatModifierSystem : StatModifierSystem<float>
    {
        public float Calculate(float baseValue)
        {
            float result = baseValue;

            foreach (var mod in _modifiers)
            {
                var modifier = (NumericStatModifier)mod;
                if (modifier.Type == NumericModType.Add)
                    result += mod.Value;
                else if (modifier.Type == NumericModType.Mult)
                    result *= mod.Value;
            }

            return result;
        }
    }
}