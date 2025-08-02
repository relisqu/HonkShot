using System;
using UnityEngine;

namespace Scripts.Items
{
    public enum NumericModType
    {
        Add,
        Mult
    }

    [Serializable]
    public class NumericStatModifier : IStatModifier<float>
    {
        private int _id;
        [SerializeField] private NumericModType _type;
        [SerializeField] private float _value;
        [SerializeField] private int _order;

        public int Id => _id;
        public NumericModType Type => _type;
        public float Value => _value;
        public int Order => _order;

        public void SetValue(float newValue)
        {
            _value = newValue;
        }

        public void SetOrder(int order)
        {
            _order = order;
        }

        public void SetId(int id)
        {
            _id = id;
        }

        public NumericStatModifier(int id, NumericModType type, float value, int order = 0)
        {
            SetId(id);
            SetOrder(order);
            SetValue(value);
            _type = type;
        }
    }
}