using Scripts.LevelSystem.LevelGeneration;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Scripts.LevelSystem
{
    public class FloorVisualController : MonoBehaviour
    {
        [SerializeField] private Volume _volume;

        private UnityEngine.Camera _camera;
        private Vignette _vignette;

        private void Awake()
        {
            _camera = UnityEngine.Camera.main;

            if (_volume && _volume.profile)
                _volume.profile.TryGet(out _vignette);
        }

        private void Start()
        {
            LevelManager.Instance.FloorEntered += LevelManager_FloorEntered;
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance)
                LevelManager.Instance.FloorEntered -= LevelManager_FloorEntered;
        }

        private void LevelManager_FloorEntered(FloorConfigSO floorConfig)
        {
            if (!floorConfig) return;

            _camera.backgroundColor = floorConfig.BackgroundColor;

            if (_vignette != null)
            {
                _vignette.active = true;
                _vignette.color.Override(floorConfig.VignetteColor);
            }
        }
    }
}
