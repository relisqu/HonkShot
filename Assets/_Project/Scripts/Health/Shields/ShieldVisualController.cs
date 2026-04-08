using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Health
{
    public class ShieldVisualController : MonoBehaviour
    {
        [SerializeField] private ShieldController _shieldController;
        [SerializeField] private ShieldVisual _shieldVisualPrefab;
        [SerializeField] private Transform _shieldVisualParent;

        private readonly Dictionary<Shield, ShieldVisual> _visuals = new Dictionary<Shield, ShieldVisual>();

        private void Awake()
        {
            if (!_shieldController)
                _shieldController = GetComponent<ShieldController>();
        }

        private void OnEnable()
        {
            if (!_shieldController) return;

            _shieldController.OnShieldAdded += ShieldController_ShieldAdded;
            _shieldController.OnShieldDamaged += ShieldController_ShieldDamaged;
            _shieldController.OnShieldDestroyed += ShieldController_ShieldDestroyed;
        }

        private void OnDisable()
        {
            if (!_shieldController) return;

            _shieldController.OnShieldAdded -= ShieldController_ShieldAdded;
            _shieldController.OnShieldDamaged -= ShieldController_ShieldDamaged;
            _shieldController.OnShieldDestroyed -= ShieldController_ShieldDestroyed;
        }

        private void ShieldController_ShieldAdded(Shield shield)
        {
            if (!_shieldVisualPrefab) return;

            var parent = _shieldVisualParent ? _shieldVisualParent : transform;
            var visual = Instantiate(_shieldVisualPrefab, parent);
            _visuals[shield] = visual;
        }

        private void ShieldController_ShieldDamaged(Shield shield)
        {
        }

        private void ShieldController_ShieldDestroyed(Shield shield)
        {
            if (_visuals.TryGetValue(shield, out var visual))
            {
                if (visual)
                    visual.PlayDestructionAnimation();
                _visuals.Remove(shield);
            }
        }
    }
}
