using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Language Database",  fileName = "LanguageDatabase")]
public class LanguageDatabase : ScriptableObject
{
    public List<LanguageEntry> entries = new();
}

[System.Serializable]
public class LanguageEntry
{
    public string displayName; // eg Russian, ENglish
    public string code;        // eg RU, EN
}