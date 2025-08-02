using UnityEditor;
using UnityEngine;

namespace _Project.Editor
{
    public class Tools
    {
        [MenuItem("Tools/ClearPlayerPrefs")]
        public static void ClearPlayerPrefs() => 
            PlayerPrefs.DeleteAll();
        
        [MenuItem("Tools/ClearCacheFiles")]
        public static void ClearCacheFiles() =>
            Caching.ClearCache();
    }
}