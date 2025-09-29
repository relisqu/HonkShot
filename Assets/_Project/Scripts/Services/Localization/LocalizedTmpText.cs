using TMPro;
using UnityEngine;
using Zenject;

namespace Scripts.Services.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTmpText : MonoBehaviour
    {
        private ILocalizationService _localizationService;
        
        [SerializeField] public string _localizedKey = "INSERT_KEY_HERE";
        
        private TMP_Text _text;

        [Inject]
        public void Construct(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _text = GetComponent<TMP_Text>();
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