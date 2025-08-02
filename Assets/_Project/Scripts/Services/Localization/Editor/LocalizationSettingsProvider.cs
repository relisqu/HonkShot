using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Scripts.Services.Localization.Editor
{
    public class LocalizationSettingsProvider : SettingsProvider
    {
        private bool _isActive;
        private LocalizationHandler _localizationHandler;
        private LocalizationGui _localizationGui;
        
        public LocalizationSettingsProvider() 
            : base("Project Tools/Localization", SettingsScope.Project, null)
        {
            label = "Localization";
            keywords = new HashSet<string>(new[] {"localization"});
        }

        private void Initialize()
        {
            _localizationHandler = new LocalizationHandler();
            _localizationGui = new LocalizationGui(_localizationHandler);
        }
        
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            
            _isActive = true;
            Initialize();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            
            // Fix in play mode causes an error
            if (!_isActive)
                return;

            _isActive = false;
            _localizationHandler.SaveSettings();
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            _localizationGui.OnGui();
        }

        public override void OnInspectorUpdate()
        {
            base.OnInspectorUpdate();
            _localizationHandler.Update();
            _localizationHandler.SaveSettings();
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider() => 
            new LocalizationSettingsProvider();
    }
}