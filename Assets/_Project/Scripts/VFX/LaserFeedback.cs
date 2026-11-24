using System.Collections;
using DG.Tweening;
using Scripts.Camera;
using Scripts.Enemies.Bullets;
using UnityEngine;

namespace Scripts.VFX
{
    // Laser juice: while charging, a thin aim line shows the beam's path and then flickers; it snaps wide when it
    // fires (with a small camera kick),
    // and sparks where it hits a wall. Only the beam's visual child changes width, not the damage collider.
    public class LaserFeedback : MonoBehaviour
    {
        [SerializeField] private LaserBaseBullet _laser;
        [Tooltip("Visual child of the beam (sprite only, no collider). Its X scale is the beam width multiplier.")]
        [SerializeField] private Transform _beamVisual;

        [Header("Charge: Aim Line")]
        [Tooltip("Width multiplier of the thin aim line at the start (x) and end (y) of its phase.")]
        [SerializeField] private Vector2 _aimLineWidthRange = new Vector2(0.08f, 0.18f);

        [Header("Charge: Flicker")]
        [Tooltip("Share of the prefire spent flickering, at its end. The rest shows the steady aim line.")]
        [SerializeField, Range(0f, 1f)] private float _flickerShare = 0.35f;
        [Tooltip("Beam width multiplier range while flickering; it jumps between these.")]
        [SerializeField] private Vector2 _chargeWidthRange = new Vector2(0.25f, 0.6f);
        [SerializeField] private float _flickerInterval = 0.04f;

        [Header("Fire Snap")]
        [Tooltip("Beam width multiplier right when it fires, settling back to 1.")]
        [SerializeField] private float _fireWidthOvershoot = 1.7f;
        [SerializeField] private float _fireSettleDuration = 0.14f;
        [SerializeField] private float _fireShakeDuration = 0.08f;
        [SerializeField] private float _fireShakeStrength = 0.7f;

        [Header("Beam-End Sparks")]
        [SerializeField] private GameObject _impactPrefab;
        [SerializeField] private float _impactScale = 0.8f;

        private float _baseWidth;

        private void Awake()
        {
            if (!_laser)
                _laser = GetComponentInChildren<LaserBaseBullet>();
            if (_beamVisual)
                _baseWidth = _beamVisual.localScale.x;
        }

        private void Start()
        {
            if (!_laser) return;
            _laser.OnReady += Laser_Ready;
            if (_beamVisual)
                StartCoroutine(ChargeFlicker());
        }

        private void OnDestroy()
        {
            if (_laser)
                _laser.OnReady -= Laser_Ready;
        }

        private IEnumerator ChargeFlicker()
        {
            float startTime = Time.time;
            float endTime = startTime + _laser.PrefireDuration;
            float aimLineDuration = _laser.PrefireDuration * (1f - _flickerShare);

            while (Time.time < startTime + aimLineDuration)
            {
                float t = aimLineDuration > 0f ? (Time.time - startTime) / aimLineDuration : 1f;
                SetWidth(Mathf.Lerp(_aimLineWidthRange.x, _aimLineWidthRange.y, t));
                yield return null;
            }

            bool thin = true;
            while (Time.time < endTime)
            {
                SetWidth(thin ? _chargeWidthRange.x : _chargeWidthRange.y);
                thin = !thin;
                yield return new WaitForSeconds(_flickerInterval);
            }
        }

        private void Laser_Ready()
        {
            StopAllCoroutines();

            if (_beamVisual)
            {
                SetWidth(_fireWidthOvershoot);
                _beamVisual.DOScaleX(_baseWidth, _fireSettleDuration).SetEase(Ease.OutQuad).SetLink(gameObject);
            }

            if (CameraShakeHandler.Instance)
                CameraShakeHandler.Instance.ShakeCamera(_fireShakeDuration, _fireShakeStrength);

            if (_impactPrefab && _laser.BeamHitsObstacle)
            {
                var sparks = Instantiate(_impactPrefab, _laser.BeamEnd, Quaternion.identity);
                sparks.transform.localScale *= _impactScale;
            }
        }

        private void SetWidth(float multiplier)
        {
            var scale = _beamVisual.localScale;
            scale.x = _baseWidth * multiplier;
            _beamVisual.localScale = scale;
        }
    }
}
