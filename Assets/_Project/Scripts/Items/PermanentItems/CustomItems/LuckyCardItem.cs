using Scripts.Items.PlayerItemManager;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class LuckyCardItem : Item, IExtraPickProvider, IRerollProvider
    {
        [Header("Reroll Settings")]
        [SerializeField] private int _rerollCount = 2;

        [Header("Extra Pick Settings")]
        [SerializeField] private int _guaranteedExtraPicks = 0;
        [SerializeField] private float _extraPickChance = 0.25f;

        private int _remainingRerolls;

        public int RemainingRerolls => _remainingRerolls;

        public int GuaranteedExtraPicks => _guaranteedExtraPicks;
        public float ExtraPickChance => _extraPickChance;

        private void Start()
        {
            _remainingRerolls = _rerollCount;

            if (ExtraPickSystem.Instance)
            {
                ExtraPickSystem.Instance.Register(this);
            }

            if (RerollSystem.Instance)
            {
                RerollSystem.Instance.Register(this);
            }
        }

        private void OnDestroy()
        {
            if (ExtraPickSystem.Instance)
            {
                ExtraPickSystem.Instance.Unregister(this);
            }

            if (RerollSystem.Instance)
            {
                RerollSystem.Instance.Unregister(this);
            }
        }

        public bool TryUseReroll()
        {
            if (_remainingRerolls <= 0) return false;

            _remainingRerolls--;
            Debug.Log($"[LuckyCardItem] Reroll used! Remaining: {_remainingRerolls}");
            return true;
        }

        public void OnExtraPickUsed()
        {
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
        }
    }
}
