#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizationManager))]
public class LocalizationManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        var manager = (LocalizationManager)target;
        var db = manager.languageDatabase;

        if (db == null || db.entries == null || db.entries.Count == 0)
        {
            EditorGUILayout.HelpBox("Language database is empty.", MessageType.Warning);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        string[] options = new string[db.entries.Count];
        int currentIndex = 0;

        for (int i = 0; i < db.entries.Count; i++)
        {
            options[i] = db.entries[i].displayName;

            if (manager.currentLanguageEntry == db.entries[i])
                currentIndex = i;
        }

        int newIndex = EditorGUILayout.Popup("Current Language", currentIndex, options);

        if (newIndex != currentIndex)
        {
            Undo.RecordObject(manager, "Change Language");
            manager.currentLanguageEntry = db.entries[newIndex];
            EditorUtility.SetDirty(manager);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif