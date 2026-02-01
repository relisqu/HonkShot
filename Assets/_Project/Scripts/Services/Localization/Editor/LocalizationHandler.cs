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

        private int _currentSheetIndex;
        private bool _isProcessing;
        private Dictionary<string, Dictionary<string, string>> _accumulatedInternal;
        private Dictionary<string, Dictionary<string, string>> _accumulatedExternal;

        public UnityWebRequest UnityWebRequest => _unityWebRequest;
        public LocalizationSettings LocalizationSettings => _localizationSettings;
        public bool IsProcessed => _isProcessing;
        public int CurrentSheetIndex => _currentSheetIndex;
        public int TotalSheets => _localizationSettings.SheetIds.Count;

        public LocalizationHandler() =>
            _localizationSettings =
                EditorSettingsUtility.LoadOrCreate<LocalizationSettings>(LocalizationSettings.FileName);

        private string GetTableUri(string sheetId) =>
            string.Format(UriFormat, LocalizationSettings.TableId, sheetId);

        public void Update()
        {
            if (!_isProcessing || _unityWebRequest == null)
                return;

            if (!_unityWebRequest.isDone)
                return;

            if (_unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Process. Download error for sheet {_currentSheetIndex + 1}/{TotalSheets}: {_unityWebRequest.error}");
                Reset();
                return;
            }

            try
            {
                var text = _unityWebRequest.downloadHandler.text;
                _unityWebRequest = null;

                if (LocalizationSettings.IsSplitInternalAndExternal)
                {
                    MergeDictionaries(_accumulatedInternal, ParsingText(text, true));
                    MergeDictionaries(_accumulatedExternal, ParsingText(text, false));
                }
                else
                {
                    MergeDictionaries(_accumulatedInternal, ParsingText(text, false));
                }

                Debug.Log($"Process. Sheet {_currentSheetIndex + 1}/{TotalSheets} parsed.");
                _currentSheetIndex++;

                if (_currentSheetIndex < TotalSheets)
                {
                    DownloadSheet(_currentSheetIndex);
                }
                else
                {
                    SaveAccumulatedResults();
                    Reset();
                    Debug.Log("Process. All sheets complete!");
                }
            }
            catch (Exception e)
            {
                Reset();
                Debug.LogError($"Process. Exception {e}");
            }
        }

        public void StartProcessing()
        {
            if (LocalizationSettings.SheetIds.Count == 0)
            {
                Debug.LogError("Process. No sheet IDs configured.");
                return;
            }

            _currentSheetIndex = 0;
            _isProcessing = true;
            _accumulatedInternal = new Dictionary<string, Dictionary<string, string>>();
            _accumulatedExternal = new Dictionary<string, Dictionary<string, string>>();

            DownloadSheet(0);
        }

        private void DownloadSheet(int index)
        {
            var sheetId = LocalizationSettings.SheetIds[index];
            _unityWebRequest = UnityWebRequest.Get(GetTableUri(sheetId));
            _unityWebRequest.SendWebRequest();
            Debug.Log($"Process. Downloading sheet {index + 1}/{TotalSheets} (gid={sheetId})");
        }

        private void MergeDictionaries(
            Dictionary<string, Dictionary<string, string>> target,
            Dictionary<string, Dictionary<string, string>> source)
        {
            foreach (var kvp in source)
            {
                if (!target.ContainsKey(kvp.Key))
                    target[kvp.Key] = new Dictionary<string, string>();

                foreach (var entry in kvp.Value)
                {
                    if (target[kvp.Key].ContainsKey(entry.Key))
                        Debug.LogWarning($"Process. Duplicate key '{entry.Key}' in language '{kvp.Key}', overwriting with later sheet value.");

                    target[kvp.Key][entry.Key] = entry.Value;
                }
            }
        }

        private void SaveAccumulatedResults()
        {
            foreach (var kvp in _accumulatedInternal)
            {
                string filePath = Path.Combine(LocalizationSettings.InternalFolder, kvp.Key + ".txt");
                SaveToFile(kvp.Value, filePath);
            }

            if (LocalizationSettings.IsSplitInternalAndExternal)
            {
                foreach (var kvp in _accumulatedExternal)
                {
                    string filePath = Path.Combine(LocalizationSettings.ExternalFolder, kvp.Key + ".txt");
                    SaveToFile(kvp.Value, filePath);
                }
            }
        }

        private void Reset()
        {
            _unityWebRequest = null;
            _isProcessing = false;
            _accumulatedInternal = null;
            _accumulatedExternal = null;
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
            if (_unityWebRequest != null)
                _unityWebRequest.Abort();

            Reset();
        }

        public void SaveSettings() =>
            EditorSettingsUtility.Save(_localizationSettings, LocalizationSettings.FileName);
    }
}
