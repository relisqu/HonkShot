using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterDatabase : MonoBehaviour
{
    private static List<Character> characters = new();

    public static List<Character> Characters => characters;

    public static string[] GetNames() => characters.ConvertAll(c => c.name).ToArray();
    public static Sprite GetPortrait(string name) => characters.FirstOrDefault(c => c.name == name)?.portrait;
    
#if UNITY_EDITOR
    public static void AddCharacter(Character ch)
    {
        if (!characters.Contains(ch)) characters.Add(ch);
    }

    [MenuItem("Dialogue/Refresh Characters DB")]
    public static void RefreshDB()
    {
        characters.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Character"); // Все SO типа Character
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Character ch = AssetDatabase.LoadAssetAtPath<Character>(path);
            if (ch != null) AddCharacter(ch);
        }
        AssetDatabase.Refresh();
        Debug.Log($"Refreshed {characters.Count} characters");
    }
#endif
}
