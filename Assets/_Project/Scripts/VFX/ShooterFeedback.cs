using DG.Tweening;
using Scripts.Enemies;
using UnityEngine;

namespace Scripts.VFX
{
    // Visual telegraph and recoil for shooting enemies: swell + tint + tremble during the wind-up,
    // then a squash recoil, a kick opposite the shot and a muzzle puff when the shot fires.
    // Only the visual child scales/moves (never the colliders); without one, only tint and puff play.
    public class ShooterFeedback : MonoBehaviour
    {
        [SerializeField] private ShootingModule _shootingModule;
        [Tooltip("Visual child that swells and recoils. Must not hold colliders. Leave empty for tint and muzzle puff only.")]
        [SerializeField] private Transform _visualTarget;
        [Tooltip("Sprites tinted during the wind-up. Leave empty for every sprite under the visual target.")]
        [SerializeField] private SpriteRenderer[] _tintRenderers;

        [Header("Wind-Up Telegraph")]
        [Tooltip("Time from wind-up start to the shot (the TryShoot event in the prepare clip). 0 disables the telegraph.")]
        [SerializeField] private float _windUpDuration = 0.4f;
        [SerializeField] private Vector3 _windUpScale = new Vector3(1.15f, 1.15f, 1f);
        [SerializeField] private Color _windUpTint = new Color(1f, 0.55f, 0.55f, 1f);
        [Tooltip("Tremble strength at the end of the wind-up; it grows from zero.")]
        [SerializeField] private float _windUpShakeStrength = 0.04f;
        [SerializeField] private int _windUpShakeVibrato = 30;

        [Header("Recoil")]
        [Tooltip("Scale offset at the first recoil peak for aimed shots: +x wider, -y flatter.")]
        [SerializeField] private Vector3 _recoilPunch = new Vector3(0.35f, -0.3f, 0f);
        [Tooltip("Scale offset for radial bursts (spread shooters).")]
        [SerializeField] private Vector3 _radialRecoilPunch = new Vector3(0.3f, 0.3f, 0f);
        [SerializeField] private float _recoilDuration = 0.3f;
        [SerializeField] private int _recoilVibrato = 6;
        [SerializeField, Range(0f, 1f)] private float _recoilElasticity = 0.7f;
        [Tooltip("How far the visual kicks back, opposite the shot direction.")]
        [SerializeField] private float _recoilKickDistance = 0.18f;
        [SerializeField] private float _recoilKickDuration = 0.22f;

        [Header("Muzzle Puff")]
        [SerializeField] private GameObject _muzzlePrefab;
        [Tooltip("Spawn distance from the shooting module along the shot direction.")]
        [SerializeField] private float _muzzleOffset = 0.3f;
        [SerializeField] private float _muzzleScale = 0.7f;

        private VisualTransformComposer _composer;
        private Color[] _originalColors;
        private float _appliedTint;

        private Tweener _windUpTween;
        private float _windUpProgress;
        private Tweener _windUpShakeTween;
        private Vector3 _windUpShakeOffset;
        private Tweener _recoilTween;
        private Vector3 _recoilOffset;
        private Tweener _kickTween;
        private Vector3 _kickOffset;

        private void Awake()
        {
            if (!_shootingModule)
                _shootingModule = GetComponentInChildren<ShootingModule>();
            if (_tintRenderers == null || _tintRenderers.Length == 0)
                _tintRenderers = (_visualTarget ? _visualTarget : transform).GetComponentsInChildren<SpriteRenderer>(true);
            if (_visualTarget)
                _composer = VisualTransformComposer.GetOrAdd(_visualTarget);

            _originalColors = new Color[_tintRenderers.Length];
            for (int i = 0; i < _tintRenderers.Length; i++)
                _originalColors[i] = _tintRenderers[i] ? _tintRenderers[i].color : Color.white;
        }

        private void Start()
        {
            if (!_shootingModule) return;
            _shootingModule.OnWindUpStarted += ShootingModule_WindUpStarted;
            _shootingModule.OnWindUpCancelled += ShootingModule_WindUpCancelled;
            _shootingModule.OnFired += ShootingModule_Fired;
        }

        private void OnDestroy()
        {
            if (!_shootingModule) return;
            _shootingModule.OnWindUpStarted -= ShootingModule_WindUpStarted;
            _shootingModule.OnWindUpCancelled -= ShootingModule_WindUpCancelled;
            _shootingModule.OnFired -= ShootingModule_Fired;
        }

        private void OnDisable()
        {
            KillTween(ref _windUpTween);
            KillTween(ref _windUpShakeTween);
            KillTween(ref _recoilTween);
            KillTween(ref _kickTween);
            _windUpProgress = 0f;
            ApplyTint(0f);
            if (_composer)
                _composer.Clear(this);
        }

        private void ShootingModule_WindUpStarted()
        {
            if (_windUpDuration <= 0f || !isActiveAndEnabled) return;

            KillTween(ref _windUpTween);
            KillTween(ref _windUpShakeTween);
            _windUpProgress = 0f;
            _windUpTween = DOTween.To(() => _windUpProgress, value => _windUpProgress = value, 1f, _windUpDuration)
                .SetEase(Ease.InQuad)
                .SetLink(gameObject);

            if (_composer)
            {
                _windUpShakeOffset = Vector3.zero;
                _windUpShakeTween = DOTween.Shake(() => _windUpShakeOffset, value => _windUpShakeOffset = value,
                        _windUpDuration, _windUpShakeStrength, _windUpShakeVibrato, 90f, true, false)
                    .SetLink(gameObject);
            }
        }

        private void ShootingModule_WindUpCancelled()
        {
            StopWindUp();
        }

        private void ShootingModule_Fired(Vector2 direction)
        {
            if (!isActiveAndEnabled) return;

            StopWindUp();
            Recoil(direction);
            SpawnMuzzlePuff(direction);
        }

        private void StopWindUp()
        {
            KillTween(ref _windUpTween);
            KillTween(ref _windUpShakeTween);
            _windUpProgress = 0f;
        }

        private void Recoil(Vector2 direction)
        {
            if (!_composer) return;

            KillTween(ref _recoilTween);
            _recoilOffset = Vector3.zero;
            var punch = direction == Vector2.zero ? _radialRecoilPunch : _recoilPunch;
            _recoilTween = DOTween.Punch(() => _recoilOffset, value => _recoilOffset = value, punch, _recoilDuration,
                    _recoilVibrato, _recoilElasticity)
                .SetLink(gameObject);

            if (direction == Vector2.zero) return;

            KillTween(ref _kickTween);
            _kickOffset = Vector3.zero;
            _kickTween = DOTween.Punch(() => _kickOffset, value => _kickOffset = value,
                    -(Vector3)(direction.normalized * _recoilKickDistance), _recoilKickDuration, 2, 0.5f)
                .SetLink(gameObject);
        }

        private void SpawnMuzzlePuff(Vector2 direction)
        {
            if (!_muzzlePrefab || !_shootingModule) return;

            var position = _shootingModule.transform.position + (Vector3)(direction.normalized * _muzzleOffset);
            var puff = Instantiate(_muzzlePrefab, position, Quaternion.identity);
            puff.transform.localScale *= _muzzleScale;
        }

        private void LateUpdate()
        {
            float windUp = IsPlaying(_windUpTween) ? _windUpProgress : 0f;
            ApplyTint(windUp);

            if (!_composer) return;

            var scale = Vector3.Lerp(Vector3.one, _windUpScale, windUp);
            if (IsPlaying(_recoilTween))
                scale = Vector3.Scale(scale, Vector3.one + _recoilOffset);

            var offset = (IsPlaying(_windUpShakeTween) ? _windUpShakeOffset * windUp : Vector3.zero)
                         + (IsPlaying(_kickTween) ? _kickOffset : Vector3.zero);
            _composer.Set(this, scale, offset);
        }

        private void ApplyTint(float amount)
        {
            if (Mathf.Approximately(amount, _appliedTint)) return;
            _appliedTint = amount;

            var multiplier = Color.Lerp(Color.white, _windUpTint, amount);
            for (int i = 0; i < _tintRenderers.Length; i++)
            {
                if (_tintRenderers[i])
                    _tintRenderers[i].color = _originalColors[i] * multiplier;
            }
        }

        private static void KillTween(ref Tweener tween)
        {
            if (tween != null && tween.IsActive())
                tween.Kill();
            tween = null;
        }

        private static bool IsPlaying(Tweener tween)
        {
            return tween != null && tween.IsActive();
        }
    }
}
