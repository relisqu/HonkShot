using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Scripts.Services.Localization
{
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        private ILocalizationService _localizationService;
        
        [SerializeField] private string _localizedKey = "INSERT_KEY_HERE";
        
        private Text _text;

        [Inject]
        public void Construct(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _text = GetComponent<Text>();
        }

        private void Start()
        {
            _localizationService.LanguageChanged += LocalizationService_LanguageChanged;
            UpdateText();
        }

        private void OnDestroy() => 
            _localizationService.LanguageChanged -= LocalizationService_LanguageChanged;

        private void UpdateText() => 
            _text.text = _localizationService.Get(_localizedKey);

        private void LocalizationService_LanguageChanged(string language) => 
            UpdateText();
    }
}