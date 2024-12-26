using Cinemachine;
using DG.Tweening;
using UnityEngine;

namespace Scripts.Camera
{
    public class CameraShakeHandler : MonoBehaviour
    {
        [SerializeField] private CinemachineBasicMultiChannelPerlin _virtualCameraBasicMultiChannelPerlin;

        public static CameraShakeHandler Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void ShakeCamera(float duration, float strength)
        {
            _virtualCameraBasicMultiChannelPerlin.m_AmplitudeGain =
                strength;

            DOTween.To(
                () => _virtualCameraBasicMultiChannelPerlin.m_AmplitudeGain,
                value => _virtualCameraBasicMultiChannelPerlin.m_AmplitudeGain =
                    value,
                0f, // Target value
                duration
            ).OnComplete(() => { _virtualCameraBasicMultiChannelPerlin.m_AmplitudeGain = 0f; });
        }
    }
}