using System;
using System.Collections.Generic;
using System.Linq;

namespace Scripts.Items.StatSystems
{
    public class StatModifierSystem<T>
    {
        protected List<IStatModifier<T>> _modifiers = new();

        public void AddModifier(IStatModifier<T> modifier)
        {
            _modifiers.Add(modifier);
            _modifiers = _modifiers.OrderBy(m => m.Order).ToList();
        }

        public void UpdateModifier(string invTag, T value)
        {
            foreach (var modifier in _modifiers.ToList())
            {
                if (modifier.Id == invTag)
                {
                    modifier.SetValue(value);
                }
            }
        }

        public void RemoveModifier(string id)
        {
            foreach (var modifier in _modifiers.ToList())
            {
                if (modifier.Id == id)
                    _modifiers.Remove(modifier);
            }
        }

        public T Calculate(T baseValue, Func<T, T, T> calculationFunc)
        {
            T result = baseValue;
            foreach (var mod in _modifiers)
            {
                result = calculationFunc(result, mod.Value);
            }

            return result;
        }

        public int GetLastOrder()
        {
            var order = -1;
            foreach (var statModifier in _modifiers)
            {
                if (statModifier.Order > order)
                {
                    order = statModifier.Order;
                }
            }

            return order;
        }

        public bool HasModifier(string id)
        {
            return _modifiers.Any(statModifier => statModifier.Id == id);
        }

        public IStatModifier<T> GetModifier(string id)
        {
            foreach (var statModifier in _modifiers)
            {
                if (statModifier.Id == id)
                {
                    return statModifier;
                }
            }

            return null;
        }
    }
}