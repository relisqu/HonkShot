using System;
using UnityEngine;

namespace Scripts.Items.StatSystems
{
    public enum BoolModType
    {
        Override,
        OverrideIfTrue,
        OverrideIfFalse,
        And,
        Or
    }

    [Serializable]
    public class BoolStatModifier : IStatModifier<bool>
    {
        private string _id;

        [SerializeField] private BoolModType _type;
        [SerializeField] private bool _value;
        [SerializeField] private int _order;

        public string Id => _id;
        public BoolModType Type => _type;
        public bool Value => _value;
        public int Order => _order;

        public void SetValue(bool newValue)
        {
            _value = newValue;
        }

        public void SetOrder(int order)
        {
            _order = order;
        }

        public void SetId(string id)
        {
            _id = id;
        }

        public BoolStatModifier(string id, BoolModType type, bool value, int order = 0)
        {
            SetId(id);
            SetOrder(order);
            SetValue(value);
            _type = type;
        }
    }
}