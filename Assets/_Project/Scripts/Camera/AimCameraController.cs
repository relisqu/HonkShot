using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Scripts.Camera
{
    // Owns the "aim camera" feel: while the player charges a throw the camera zooms out and pans toward
    // the throw direction, then snaps back on release. Driving the camera directly requires bypassing
    // pixel-perfect (see BeginPixelPerfectBypass), so this component owns that bypass too.
    // Callers just drive the lifecycle: BeginAim() -> UpdateAim() each frame -> ReleaseAim(punchy).
    public class AimCameraController : MonoBehaviour
    {
        public static AimCameraController Instance;

        [Header("Zoom")]
        [SerializeField] private float _maxZoomOut = 1f;
        [SerializeField] private float _zoomDuration = 0.1f;

        [Header("Pan")]
        [SerializeField] private float _maxPanOffset = 1.5f;

        [Header("Throw Release Snap (punchy overshoot)")]
        [SerializeField] private float _snapDuration = 0.25f;
        [SerializeField] private Ease _snapEase = Ease.OutBack;
        [SerializeField] private float _snapOvershoot = 2f;

        [Header("Pixel-Perfect (optional; auto-resolved from the main camera if left empty)")]
        [SerializeField] private PixelPerfectCamera _urpPixelPerfect;
        [SerializeField] private CinemachinePixelPerfect _cmPixelPerfect;

        private CinemachineCamera _virtualCamera;
        private UnityEngine.Camera _outputCamera;
        private CinemachineBrain _brain;

        private bool _pixelPerfectBypassed;
        private bool _aiming;
        private float _originalSize;
        private Vector3 _originalCameraPos;

        private Tweener _zoomTween;
        private Tweener _panTween;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (CameraShakeHandler.Instance)
                _virtualCamera = CameraShakeHandler.Instance._virtualCamera;

            if (_cmPixelPerfect == null && _virtualCamera)
                _cmPixelPerfect = _virtualCamera.GetComponent<CinemachinePixelPerfect>();

            if (_urpPixelPerfect == null)
            {
                var main = UnityEngine.Camera.main;
                if (main) _urpPixelPerfect = main.GetComponent<PixelPerfectCamera>();
            }

            if (_urpPixelPerfect)
            {
                _outputCamera = _urpPixelPerfect.GetComponent<UnityEngine.Camera>();
                _brain = _urpPixelPerfect.GetComponent<CinemachineBrain>();
            }
        }

        private void Update()
        {
            // Safety net: never leave pixel-perfect/brain disabled once aiming is over and the snap
            // tweens are done. ReleaseAim handles the normal path; this catches a killed tween.
            if (_pixelPerfectBypassed && !_aiming
                && (_zoomTween == null || !_zoomTween.IsActive())
                && (_panTween == null || !_panTween.IsActive()))
            {
                EndPixelPerfectBypass();
            }
        }

        #region Public API

        public void BeginAim()
        {
            _aiming = true;
            BeginPixelPerfectBypass();
        }

        public void UpdateAim(Vector2 dragForce, float forceScale)
        {
            if (!_outputCamera) return;
            HandleZoom(forceScale);
            HandlePan(dragForce, forceScale);
        }

        public void ReleaseAim(bool punchy)
        {
            _aiming = false;
            ResetCamera(punchy);
        }

        #endregion

        #region Zoom & Pan

        private void HandleZoom(float forceScale)
        {
            float targetSize = _originalSize + forceScale * _maxZoomOut;
            KillTween(ref _zoomTween);
            _zoomTween = DOTween.To(
                () => _outputCamera.orthographicSize,
                x => _outputCamera.orthographicSize = x,
                targetSize,
                _zoomDuration
            ).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        // The launch goes opposite the drag (see PlayerBallMovement.ThrowRigidbody), so pan toward
        // -dragForce, scaled by how hard the sling is pulled.
        private void HandlePan(Vector2 dragForce, float forceScale)
        {
            Vector2 throwDir = -dragForce.normalized;
            Vector3 target = _originalCameraPos + (Vector3)(throwDir * (forceScale * _maxPanOffset));
            target.z = _originalCameraPos.z;
            KillTween(ref _panTween);
            _panTween = _outputCamera.transform
                .DOMove(target, _zoomDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        private void ResetCamera(bool punchy)
        {
            KillTween(ref _zoomTween);
            KillTween(ref _panTween);

            if (!_outputCamera)
            {
                EndPixelPerfectBypass();
                return;
            }

            var duration = punchy ? _snapDuration : _zoomDuration;

            _zoomTween = DOTween.To(
                () => _outputCamera.orthographicSize,
                x => _outputCamera.orthographicSize = x,
                _originalSize,
                duration);

            _panTween = _outputCamera.transform.DOMove(_originalCameraPos, duration);

            if (punchy)
            {
                _zoomTween.SetEase(_snapEase, _snapOvershoot);
                _panTween.SetEase(_snapEase, _snapOvershoot);
            }
            else
            {
                _zoomTween.SetEase(Ease.OutQuad);
                _panTween.SetEase(Ease.OutQuad);
            }

            _zoomTween.SetUpdate(true).OnComplete(TryEndPixelPerfectBypass);
            _panTween.SetUpdate(true).OnComplete(TryEndPixelPerfectBypass);
        }

        #endregion

        #region Pixel-Perfect Bypass

        // Pixel-perfect (UpscaleRenderTexture) hard-locks the rendered ortho size, so the Cinemachine
        // lens is ignored. To actually zoom/pan we drive the output camera directly, which needs the
        // brain off (it overwrites transform + ortho from the vcam state) and pixel-perfect off (it
        // re-pins the size every frame). All are restored on settle. Aim is brief + time-slowed and the
        // player is stationary while charging, so freezing camera position for that window is safe.
        private void BeginPixelPerfectBypass()
        {
            KillTween(ref _zoomTween);
            KillTween(ref _panTween);

            _originalSize = _outputCamera ? _outputCamera.orthographicSize
                : (_virtualCamera ? _virtualCamera.Lens.OrthographicSize : 0f);
            if (_outputCamera) _originalCameraPos = _outputCamera.transform.position;

            if (_brain) _brain.enabled = false;
            if (_urpPixelPerfect) _urpPixelPerfect.enabled = false;
            if (_cmPixelPerfect) _cmPixelPerfect.enabled = false;
            _pixelPerfectBypassed = true;
        }

        // Both the ortho snap and the pan snap must finish before we re-pin pixel-perfect, otherwise the
        // re-enabled brain yanks the camera and cuts the overshoot short. Whichever tween lands last wins.
        private void TryEndPixelPerfectBypass()
        {
            if (_zoomTween != null && _zoomTween.IsActive()) return;
            if (_panTween != null && _panTween.IsActive()) return;
            EndPixelPerfectBypass();
        }

        private void EndPixelPerfectBypass()
        {
            if (!_pixelPerfectBypassed) return;
            _pixelPerfectBypassed = false;
            if (_brain) _brain.enabled = true;
            if (_urpPixelPerfect) _urpPixelPerfect.enabled = true;
            if (_cmPixelPerfect) _cmPixelPerfect.enabled = true;
        }

        #endregion

        private static void KillTween(ref Tweener tween)
        {
            if (tween != null && tween.IsActive()) tween.Kill();
            tween = null;
        }
    }
}
