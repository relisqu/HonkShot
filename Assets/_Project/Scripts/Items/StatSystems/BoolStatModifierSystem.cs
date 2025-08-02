namespace Scripts.Items
{
    public class BoolStatModifierSystem : StatModifierSystem<bool>
    {
        public bool Calculate(bool baseValue)
        {
            bool result = baseValue;
            foreach (var mod in _modifiers)
            {
                var modifier = (BoolStatModifier)mod;
                switch (modifier.Type)
                {
                    case BoolModType.Override:
                        result = mod.Value;
                        break;
                    case BoolModType.And:
                        result = result && mod.Value;
                        break;
                    case BoolModType.Or:
                        result = result || mod.Value;
                        break;
                    case BoolModType.OverrideIfFalse:
                        if (!mod.Value)
                        {
                            result = false;
                        }

                        break;
                    case BoolModType.OverrideIfTrue:
                        if (mod.Value)
                        {
                            result = true;
                        }

                        break;
                }
            }

            return result;
        }
    }
}