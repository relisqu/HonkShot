using UnityEngine;

namespace Scripts.VFX
{
    // Applies a scale multiplier and a world-space position offset on top of whatever else
    // (Animator, tweens, scripts) writes to the transform each frame. Call Apply from LateUpdate.
    // If nothing else wrote since our last write, our previous contribution is removed first,
    // so the effect never compounds; otherwise the freshly written value is taken as the base.
    public class VisualTransformLayer
    {
        private readonly Transform _target;

        private Vector3 _lastWrittenScale;
        private Vector3 _lastMultiplier = Vector3.one;
        private bool _hasWrittenScale;

        private Vector3 _lastWrittenPosition;
        private Vector3 _lastLocalOffset;
        private bool _hasWrittenPosition;

        public VisualTransformLayer(Transform target)
        {
            _target = target;
        }

        public void Apply(Vector3 scaleMultiplier, Vector3 worldOffset)
        {
            if (!_target) return;
            ApplyScale(scaleMultiplier);
            ApplyPosition(worldOffset);
        }

        public void Restore()
        {
            if (!_target) return;
            RestoreScale();
            RestorePosition();
        }

        private void ApplyScale(Vector3 multiplier)
        {
            if (multiplier == Vector3.one)
            {
                RestoreScale();
                return;
            }

            _lastWrittenScale = Vector3.Scale(GetBaseScale(), multiplier);
            _target.localScale = _lastWrittenScale;
            _lastMultiplier = multiplier;
            _hasWrittenScale = true;
        }

        private void ApplyPosition(Vector3 worldOffset)
        {
            if (worldOffset == Vector3.zero)
            {
                RestorePosition();
                return;
            }

            var parent = _target.parent;
            var localOffset = parent ? parent.InverseTransformVector(worldOffset) : worldOffset;
            localOffset.z = 0f;

            _lastWrittenPosition = GetBasePosition() + localOffset;
            _target.localPosition = _lastWrittenPosition;
            _lastLocalOffset = localOffset;
            _hasWrittenPosition = true;
        }

        private Vector3 GetBaseScale()
        {
            var current = _target.localScale;
            if (!_hasWrittenScale || current != _lastWrittenScale)
                return current;

            return new Vector3(
                SafeDivide(current.x, _lastMultiplier.x),
                SafeDivide(current.y, _lastMultiplier.y),
                SafeDivide(current.z, _lastMultiplier.z));
        }

        private Vector3 GetBasePosition()
        {
            var current = _target.localPosition;
            if (!_hasWrittenPosition || current != _lastWrittenPosition)
                return current;

            return current - _lastLocalOffset;
        }

        private void RestoreScale()
        {
            if (!_hasWrittenScale) return;
            _target.localScale = GetBaseScale();
            _hasWrittenScale = false;
            _lastMultiplier = Vector3.one;
        }

        private void RestorePosition()
        {
            if (!_hasWrittenPosition) return;
            _target.localPosition = GetBasePosition();
            _hasWrittenPosition = false;
            _lastLocalOffset = Vector3.zero;
        }

        private static float SafeDivide(float value, float divisor)
        {
            return Mathf.Approximately(divisor, 0f) ? value : value / divisor;
        }
    }
}
