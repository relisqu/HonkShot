using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//############################################################################
//# LocalizationManager setup:                                             //#
//# 1. Add this component to an empty GameObject in the first loaded scene.//#
//# 2. Keep the GameObject alive with DontDestroyOnLoad.                   //#
//# 3. After that, access the public API via LocalizationManager.Instance. //#
//# 4. And oh yeah, insert languageDatabase asset b4 you start.            //#
//############################################################################

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }
    public LanguageDatabase languageDatabase;
    public LanguageEntry currentLanguageEntry;
    [SerializeField] private TextAsset csvFile;
    [SerializeField] private string CurrentLanguage => currentLanguageEntry != null ? currentLanguageEntry.code : "RU";

    private Dictionary<string, Dictionary<string, string>> table = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadCSV(csvFile);
        Debug.Log(CurrentLanguage);
    }

    public void LoadCSV(TextAsset csv)
    {
        table.Clear();
        if (csv == null) return;

        using var reader = new StringReader(csv.text);
        string headerLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(headerLine)) return;

        var headers = headerLine.Split(',');

        while (reader.Peek() != -1)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cells = line.Split(',');
            if (cells.Length < 2) continue;

            string key = cells[0].Trim();
            if (!table.ContainsKey(key))
                table[key] = new Dictionary<string, string>();

            for (int i = 1; i < Mathf.Min(headers.Length, cells.Length); i++)
            {
                string lang = headers[i].Trim();
                string value = cells[i].Trim();
                table[key][lang] = value;
            }
        }
    }

    public void SetLanguage(LanguageEntry entry)
    {
        currentLanguageEntry = entry;
    }

    public string Get(string key)
    {
        if (table.TryGetValue(key, out var langs))
        {
            if (langs.TryGetValue(CurrentLanguage, out var value))
                return value;

            if (langs.TryGetValue("EN", out var fallback))
                return fallback;
        }

        return key;
    }
}