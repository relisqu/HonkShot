using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects
{
    public class SpikeWallDecorations : MonoBehaviour
    {
        [Tooltip("Wooden spike child sprites. Jittered once at runtime, kept upright in editor preview.")]
        [SerializeField]
        private Transform[] _spikes;

        [Header("Per-spike random offsets (applied once on Awake at runtime)")]
        [Tooltip("Max +/- random WORLD-space position jitter per axis (X=world X, Y=world Y).")]
        [SerializeField]
        private Vector2 _positionJitter = new Vector2(0.05f, 0.05f);

        [Tooltip("Max +/- random Z rotation jitter (degrees) from world up.")] [SerializeField, Min(0f)]
        private float _rotationJitter = 8f;

        [Tooltip("Random scale multiplier range applied to existing localScale.")] [SerializeField]
        private Vector2 _scaleJitter = new Vector2(0.9f, 1.1f);

        [Tooltip("If the wall is placed at ~180° rotation, snap it back to 0° on start. Safe for a horizontal-box collider.")]
        [SerializeField] private bool _normalizeFlippedRotation = true;

        private float[] _rotationOffsets;

        private void Awake()
        {
            if (_normalizeFlippedRotation)
                NormalizeWallRotation();

            RandomizeOffsets();
            AlignToWorldUp();
        }

        private void NormalizeWallRotation()
        {
            // Mathf.DeltaAngle returns shortest signed delta to 180° in [-180, 180].
            // Near 0 means the current rotation is approximately 180° (i.e., -180° or +180°).
            float deltaTo180 = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, 180f));
            if (deltaTo180 < 1f)
                transform.Rotate(0f, 0f, 180f, Space.Self);
        }


        private void RandomizeOffsets()
        {
            if (_spikes == null) return;

            _rotationOffsets = new float[_spikes.Length];

            for (int i = 0; i < _spikes.Length; i++)
            {
                var spike = _spikes[i];
                if (!spike) continue;

                Vector3 worldDelta = new Vector3(
                    Random.Range(-_positionJitter.x, _positionJitter.x),
                    Random.Range(-_positionJitter.y, _positionJitter.y),
                    0f
                );
                spike.position += worldDelta;

                _rotationOffsets[i] = Random.Range(-_rotationJitter, _rotationJitter);

                spike.localScale *= Random.Range(_scaleJitter.x, _scaleJitter.y);
            }
        }

        private void AlignToWorldUp()
        {
            if (_spikes == null) return;

            for (int i = 0; i < _spikes.Length; i++)
            {
                var spike = _spikes[i];
                if (!spike) continue;

                float offset = (_rotationOffsets != null && i < _rotationOffsets.Length)
                    ? _rotationOffsets[i]
                    : 0f;
                spike.rotation = Quaternion.Euler(0f, 0f, offset);
            }
        }
    }
}