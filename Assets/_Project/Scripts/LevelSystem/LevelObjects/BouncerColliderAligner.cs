using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects
{
    [ExecuteAlways]
    public class BouncerColliderAligner : MonoBehaviour
    {
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Transform _visual;
        [SerializeField] private Vector2 _baseOffset;
        [SerializeField] private Vector2 _pivotCorrection;

        private void OnEnable()
        {
            UpdateCollider();
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UpdateCollider();
                return;
            }
#endif
            UpdateCollider();
        }

        private void UpdateCollider()
        {
            if (!_collider) return;

            if (_visual)
                transform.rotation = _visual.rotation;

            float visualRotation = _visual ? _visual.eulerAngles.z : transform.eulerAngles.z;
            float angle = -visualRotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector2 rotatedOffset = new Vector2(
                _baseOffset.x * cos - _baseOffset.y * sin,
                _baseOffset.x * sin + _baseOffset.y * cos
            );

            Vector2 rotatedCorrection = new Vector2(
                _pivotCorrection.x * cos - _pivotCorrection.y * sin,
                _pivotCorrection.x * sin + _pivotCorrection.y * cos
            );

            _collider.offset = rotatedOffset + _pivotCorrection - rotatedCorrection;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateCollider();
        }
#endif
    }
}
