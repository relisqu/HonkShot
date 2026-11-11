using UnityEngine;

namespace Scripts.Services.Localization
{
    [CreateAssetMenu(fileName = "LanguagesConfig", menuName = "Localization/Languages Config")]
    public class LanguagesConfigSO : ScriptableObject
    {
        [SerializeField] private Language[] _languages;

        public Language[] Languages => _languages;
    }
}
