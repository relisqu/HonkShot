using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConversationNode))]
public class ConversationNodeDrawer : PropertyDrawer
{
    private CharacterDatabase _cachedDB;
    private string[] _names;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // AUTO DB INIT IF THIS WORKS UX GOTTA BE PEAK
        if (_cachedDB == null)
        {
            _cachedDB = Resources.Load<CharacterDatabase>("Data/CharacterDB");
            if (_cachedDB != null) _names = _cachedDB.GetNames();
        }

        EditorGUI.BeginProperty(position, label, property);

        // HEADER
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        
        
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        
        float lineH = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        Rect speakerRect = new Rect(position.x, position.y, position.width, lineH);
        Rect lineKeyRect = new Rect(position.x, position.y + lineH + spacing, position.width, lineH);
        Rect warningRect = new Rect(position.x, position.y + (lineH + spacing) * 2, position.width, lineH);

        
        var nameProp = property.FindPropertyRelative("SpeakerNameKey");
        if (_cachedDB != null && _names != null && _names.Length > 0)
        {
            int currentIndex = System.Array.IndexOf(_names, nameProp.stringValue);
            if (currentIndex < 0) currentIndex = 0;

            currentIndex = EditorGUI.Popup(speakerRect, "Speaker", currentIndex, _names);
            nameProp.stringValue = _names[currentIndex];
        }
        else
        {
            
            nameProp.stringValue = EditorGUI.TextField(speakerRect, "Speaker (Key)", nameProp.stringValue);
            
            
            GUI.color = Color.yellow;
            EditorGUI.LabelField(warningRect, "⚠ DB not found in Resources/Data/CharacterDB", EditorStyles.miniLabel);
            GUI.color = Color.white;
        }

        
        var lineProp = property.FindPropertyRelative("ConvoLineKey");
        EditorGUI.PropertyField(lineKeyRect, lineProp, new GUIContent("Dialogue Key"));

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineH = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // we padding it a little
        int lineCount = (_cachedDB == null) ? 3 : 2;
        
        return (lineH * lineCount) + (spacing * (lineCount - 1)) + 5f;
    }
}
