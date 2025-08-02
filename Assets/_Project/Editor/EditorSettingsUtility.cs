using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Editor
{
    public static class EditorSettingsUtility
    {
        private const string Directory = "ProjectSettings";

        public static T LoadOrCreate<T>(string fileName) where T : ScriptableObject
        {
            T settings = null;
            string filePath = GetFilePath(fileName);

            settings = Load<T>(fileName);

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<T>();
                Save(settings, fileName);
            }

            return settings;
        }

        public static void Save<T>(T settings, string fileName) where T : ScriptableObject
        {
            if (!System.IO.Directory.Exists(Directory)) 
                System.IO.Directory.CreateDirectory(Directory);
            
            try
            {
                UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(
                    new Object[] { settings }, 
                    GetFilePath(fileName), 
                    true);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Can't save {fileName}!\n{exception}");
            }
        }

        public static T Load<T>(string fileName) where T : ScriptableObject
        {
            T settings = null;
            
            try
            {
                settings = (T)UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(GetFilePath(fileName))[0];
            }
            catch (Exception exception)
            {
                Debug.Log($"Can't load file {fileName}!\n{exception}");
            }
            
            return settings;
        }

        private static string GetFilePath(string fileName) =>
            Path.Combine(Directory, fileName);
    }
}