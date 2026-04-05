using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PopupFromDatabaseAttribute))]
public class PopupFromDatabaseDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string[] names = CharacterDatabase.GetNames();
        if (names.Length == 0)
        {
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.LabelField(position, GUIContent.none, new GUIContent("Run 'Refresh Characters DB'"));
            return;
        }

        int index = System.Array.IndexOf(names, property.stringValue);
        if (index < 0) index = 0;

        Rect popupRect = EditorGUI.PrefixLabel(position, label);
        int newIndex = EditorGUI.Popup(popupRect, index, names); // GUIContent auto от string[]
        property.stringValue = names[newIndex];
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
}

public class PopupFromDatabaseAttribute : PropertyAttribute { }