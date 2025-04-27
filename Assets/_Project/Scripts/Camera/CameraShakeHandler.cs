using Cinemachine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Scripts.Camera
{
    public class CameraShakeHandler : MonoBehaviour
    {
        [SerializeField] public CinemachineVirtualCamera _virtualCamera;

        public static CameraShakeHandler Instance;

        private void Awake()
        {
            Instance = this;
        }

        TweenerCore<float, float, FloatOptions> _shakeTween;

        private void Shake(float duration, float strength)
        {
            var noiseChannel = _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            noiseChannel.m_AmplitudeGain = strength;

            _shakeTween = DOTween.To(
                () => noiseChannel.m_AmplitudeGain,
                value => noiseChannel.m_AmplitudeGain =
                    value,
                0f, // Target value
                duration
            ).OnComplete(() =>
            {
                _shakeTween = null;
                noiseChannel.m_FrequencyGain = 0f;
            });
            DOTween.To(
                () => noiseChannel.m_FrequencyGain,
                value => noiseChannel.m_FrequencyGain =
                    value,
                0f, // Target value
                duration
            ).OnComplete(() =>
            {
                noiseChannel.m_FrequencyGain = 0f;
                _shakeTween = null;
            });
        }

        public void ShakeCameraOutsideOfQueue(float duration, float strength)
        {
            if (_shakeTween != null) _shakeTween.Kill();
            Shake(duration, strength);
        }

        public void ShakeCamera(float duration, float strength)
        {
            if (_shakeTween != null) return;
            Shake(duration, strength);
        }
    }
}