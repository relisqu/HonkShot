using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Character Database", menuName = "Dialogue/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    public List<CharacterEntry> characters = new();
    public string[] GetNames()
    {
        if (characters == null) return new string[0];
        return characters.ConvertAll(c => c.nameKey).ToArray();
    }
}

[System.Serializable]
public class CharacterEntry
{
    public string nameKey;
    public Sprite portrait;
}