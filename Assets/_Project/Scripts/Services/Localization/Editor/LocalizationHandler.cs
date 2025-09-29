using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Project.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Scripts.Services.Localization.Editor
{
    public class LocalizationHandler
    {
        private const string UriFormat = "https://docs.google.com/spreadsheets/d/{0}/export?format=tsv&gid={1}";
        private const int InnerColumnIndex = 0;
        private const int KeyColumnIndex = 1;
        private const int FirstLanguageKeyColumnIndex = 2;
        private const int LanguageKeyRowIndex = 0;
        private const int FirstValueRowIndex = 1;

        private UnityWebRequest _unityWebRequest;
        private LocalizationSettings _localizationSettings;

        private string TableUri => string.Format(UriFormat, LocalizationSettings.TableId, LocalizationSettings.SheetId);
        public UnityWebRequest UnityWebRequest => _unityWebRequest;
        public LocalizationSettings LocalizationSettings => _localizationSettings;
        public bool IsProcessed => _unityWebRequest != null;

        public LocalizationHandler() =>
            _localizationSettings =
                EditorSettingsUtility.LoadOrCreate<LocalizationSettings>(LocalizationSettings.FileName);

        public void Update()
        {
            if (_unityWebRequest == null)
                return;

            if (!_unityWebRequest.isDone)
                return;

            if (_unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Process. Download error {_unityWebRequest.error}");
                _unityWebRequest = null;
                return;
            }

            try
            {
                if (LocalizationSettings.IsSplitInternalAndExternal)
                {
                    var dictionaries = ParsingText(_unityWebRequest.downloadHandler.text, true);

                    foreach (var keyValuePair in dictionaries)
                    {
                        string filePath = Path.Combine(LocalizationSettings.InternalFolder, keyValuePair.Key + ".txt");
                        SaveToFile(keyValuePair.Value, filePath);
                    }

                    dictionaries = ParsingText(_unityWebRequest.downloadHandler.text, false);

                    foreach (var keyValuePair in dictionaries)
                    {
                        string filePath = Path.Combine(LocalizationSettings.ExternalFolder, keyValuePair.Key + ".txt");
                        SaveToFile(keyValuePair.Value, filePath);
                    }
                }
                else
                {
                    var dictionaries = ParsingText(_unityWebRequest.downloadHandler.text, false);

                    foreach (var keyValuePair in dictionaries)
                    {
                        string filePath = Path.Combine(LocalizationSettings.InternalFolder, keyValuePair.Key + ".txt");
                        SaveToFile(keyValuePair.Value, filePath);
                    }
                }

                _unityWebRequest = null;
                Debug.Log($"Process. Complete!");
            }
            catch (Exception e)
            {
                _unityWebRequest = null;
                Debug.LogError($"Process. Exception {e}");
            }
        }

        public void StartProcessing()
        {
            _unityWebRequest = UnityWebRequest.Get(TableUri);
            _unityWebRequest.SendWebRequest();
            Debug.Log($"Process. Starting! {_unityWebRequest.uri}");
        }

        private Dictionary<string, Dictionary<string, string>> ParsingText(string text, bool innerOnly)
        {
            var dictionaries = new Dictionary<string, Dictionary<string, string>>();

            if (string.IsNullOrEmpty(text))
                throw new Exception("ParsingText. Text is empty!");

            string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            List<string> languageKeys = lines[LanguageKeyRowIndex]
                .Split('	')
                .Select(i => i.Trim())
                .ToList();

            if (languageKeys.Count <= FirstLanguageKeyColumnIndex)
                throw new Exception("ParsingText. No columns languages found in file");

            languageKeys.RemoveRange(0, FirstLanguageKeyColumnIndex);

            foreach (var languageKey in languageKeys)
                dictionaries.Add(languageKey, new Dictionary<string, string>());

            for (int i = FirstValueRowIndex; i < lines.Length; i++)
            {
                List<string> columns = lines[i]
                    .Split('	')
                    .Select(j => j.Replace("\r\n", "\n").Replace("\r", "\n").Trim())
                    .ToList();

                if (innerOnly && string.IsNullOrEmpty(columns[InnerColumnIndex]))
                    continue;

                var index = FirstLanguageKeyColumnIndex;
                foreach (var dictionary in dictionaries.Values)
                {
                    if (dictionary.ContainsKey(columns[KeyColumnIndex]))
                        throw new Exception($"ParsingText. Duplicate key {columns[KeyColumnIndex]}");

                    dictionary.Add(columns[KeyColumnIndex], columns[index]);
                    index++;
                }
            }

            Debug.Log($"ParsingText. Complete count: {dictionaries.Count}");
            return dictionaries;
        }

        private void SaveToFile(Dictionary<string, string> dictionary, string filePath)
        {
            var lines = new List<string>();

            foreach (var keyValuePair in dictionary)
                lines.Add(keyValuePair.Key + " ~ " + keyValuePair.Value);

            File.WriteAllLines(filePath, lines);
            AssetDatabase.Refresh();
            Debug.Log($"SaveToFile. Complete {filePath} dictionary.count: {dictionary.Count}");
        }

        public void Abort()
        {
            if (_unityWebRequest == null)
                return;

            _unityWebRequest.Abort();
            _unityWebRequest = null;
        }

        public void SaveSettings() =>
            EditorSettingsUtility.Save(_localizationSettings, LocalizationSettings.FileName);
    }
}