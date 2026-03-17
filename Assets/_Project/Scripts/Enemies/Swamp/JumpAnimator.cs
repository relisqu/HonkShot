using DG.Tweening;
using UnityEngine;

namespace Scripts.Enemies.Swamp
{
    public class JumpAnimator : MonoBehaviour
    {
        [Header("Anticipation")]
        [SerializeField] private float _anticipationDuration = 0.25f;
        [SerializeField] private float _anticipationSquash = 0.4f;

        [Header("Arc")]
        [SerializeField] private float _arcHeight = 1f;

        [Header("Squash & Stretch")]
        [SerializeField] private float _squashStrength = 0.3f;
        [SerializeField] private float _squashDuration = 0.15f;

        [Header("Shadow")]
        [SerializeField] private float _shadowScaleMin = 0.5f;

        [Header("Components")]
        [SerializeField] private Transform _visualTransform;
        [SerializeField] private Transform _shadowTransform;
        [SerializeField] private JumpingMovement _jumpingMovement;

        private Tweener _bounceTween;
        private Vector3 _shadowOriginalScale;

        public float AnticipationDuration => _anticipationDuration;

        private void Awake()
        {
            if (!_jumpingMovement)
                _jumpingMovement = GetComponent<JumpingMovement>();
            if (!_visualTransform)
            {
                var visual = transform.Find("Visual");
                _visualTransform = visual ? visual : transform;
            }
            if (!_shadowTransform)
                _shadowTransform = transform.Find("Shadow");
            if (_shadowTransform)
                _shadowOriginalScale = _shadowTransform.localScale;
        }

        private void OnEnable()
        {
            _jumpingMovement.OnJumpLand += JumpingMovement_OnJumpLand;
        }

        private void OnDisable()
        {
            _jumpingMovement.OnJumpLand -= JumpingMovement_OnJumpLand;
        }

        public void PlayAnticipation()
        {
            _bounceTween?.Kill();
            _bounceTween = _visualTransform
                .DOScale(new Vector3(1f + _anticipationSquash, 1f - _anticipationSquash, 1f), _anticipationDuration)
                .SetEase(Ease.InQuad);
        }

        public void PlayLaunchStretch()
        {
            PlaySquashStretch(1f - _squashStrength, 1f + _squashStrength);
        }

        public void UpdateArc(float t)
        {
            float arc = 4f * _arcHeight * t * (1f - t);

            if (_visualTransform && _visualTransform != transform)
                _visualTransform.localPosition = new Vector3(0f, arc, 0f);

            if (_shadowTransform)
            {
                float shadowScale = Mathf.Lerp(1f, _shadowScaleMin, arc / Mathf.Max(_arcHeight, 0.01f));
                _shadowTransform.localScale = _shadowOriginalScale * shadowScale;
            }
        }

        public void ResetArc()
        {
            if (_visualTransform && _visualTransform != transform)
                _visualTransform.localPosition = Vector3.zero;
            if (_shadowTransform)
                _shadowTransform.localScale = _shadowOriginalScale;
        }

        private void JumpingMovement_OnJumpLand()
        {
            ResetArc();
            PlaySquashStretch(1f + _squashStrength, 1f - _squashStrength);
        }

        private void PlaySquashStretch(float xScale, float yScale)
        {
            _bounceTween?.Kill();
            _visualTransform.localScale = new Vector3(xScale, yScale, 1f);
            _bounceTween = _visualTransform.DOScale(Vector3.one, _squashDuration).SetEase(Ease.OutBack);
        }

        private void OnDestroy()
        {
            _bounceTween?.Kill();
        }
    }
}
