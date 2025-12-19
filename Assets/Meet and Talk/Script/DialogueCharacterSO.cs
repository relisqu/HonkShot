using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using MeetAndTalk.GlobalValue;
using MeetAndTalk.Localization;

namespace MeetAndTalk
{
    [CreateAssetMenu(menuName = "Dialogue/New Dialogue Character")]
    public class DialogueCharacterSO : ScriptableObject
    {
        public List<LanguageGeneric<string>> characterName;
        public Color textColor = new Color(.8f, .8f, .8f, 1);

        public string HexColor()
        {
            return $"#{ColorUtility.ToHtmlStringRGB(textColor)}";
        }

        public string GetName()
        {
            LocalizationManager _manager = (LocalizationManager)Resources.Load("Languages");
            if (_manager != null)
            {
                return characterName.Find(text => text.languageEnum == _manager.SelectedLang()).LanguageGenericType;
            }
            else
            {
                return "Can't find Localization Manager in scene";
            }
        }
    }
}


[System.Serializable]
public class EmotionClass
{
    public string EmotionName;
    public OrientationPortrait Portrait;
}

[System.Serializable]
public class OrientationPortrait
{
    public Sprite SpriteImage;
}


public enum PortraitPosition
{
    None = 0,
    Primary = 1, PrimaryDist = 2,
    Secoundary = 3, SecoundaryDist = 4,
}
