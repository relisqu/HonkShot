using Scripts.Audio;
using Scripts.Services.Settings;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Scripts.UI.PauseMenu
{
    public class VolumeSliderView : MonoBehaviour
    {
        [SerializeField] private VolumeCategory _category;
        [SerializeField] private Slider _slider;

        [Header("Preview")]
        [SerializeField] private string _previewSoundName = "scorePoint";

        [Tooltip("Minimum unscaled time between preview sounds while the slider is dragged.")]
        [SerializeField] private float _previewCooldown = 0.12f;

        private SettingsService _settingsService;
        private float _lastPreviewTime = float.NegativeInfinity;

        public Slider Slider => _slider;

        [Inject]
        private void Construct(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private void Awake()
        {
            _slider.onValueChanged.AddListener(Slider_ValueChanged);
        }

        private void OnDestroy()
        {
            _slider.onValueChanged.RemoveListener(Slider_ValueChanged);
        }

        public void Refresh() =>
            _slider.SetValueWithoutNotify(_settingsService.GetVolume(_category));

        private void Slider_ValueChanged(float value)
        {
            _settingsService.SetVolume(_category, value);
            PlayPreview(value);
        }

        // The preview goes through the UI channel, which keeps playing on pause and is scaled by master
        // volume only, so music and effects are previewed by scaling the one-shot itself.
        private void PlayPreview(float value)
        {
            if (!AudioManager.Instance) return;
            if (Time.unscaledTime - _lastPreviewTime < _previewCooldown) return;

            _lastPreviewTime = Time.unscaledTime;
            float volumeScale = _category == VolumeCategory.Master ? 1f : value;
            AudioManager.Instance.PlayOneShot(SoundChanelType.UI, _previewSoundName, volumeScale);
        }
    }
}
