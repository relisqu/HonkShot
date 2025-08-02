using UnityEditor;
using UnityEngine;

namespace Scripts.Services.Localization.Editor
{
    public class LocalizationGui
    {
        private readonly LocalizationHandler _handler;
        private readonly LocalizationSettings _settings;

        public LocalizationGui(LocalizationHandler handler)
        {
            _handler = handler;
            _settings = handler.LocalizationSettings;
        }

        public void OnGui()
        {
            if (_handler.IsProcessed)
            {
                EditorGUILayout.LabelField($"Uri: {_handler.UnityWebRequest.uri}");
                EditorGUILayout.LabelField($"Download... {_handler.UnityWebRequest.downloadProgress}");
                EditorGUILayout.Space();
                if (GUILayout.Button("Abort", GUILayout.MaxWidth(80)))
                    _handler.Abort();
            }
            else
            {
                EditorGUILayout.HelpBox("Table id on Google Spreadsheet.\n" +
                                        "Let's say your table has the following url https://docs.google.com/spreadsheets/d/1RvKY3VE_y5FPhEECCa5dv4F7REJ7rBtGzQg9Z_B_DE4/edit#gid=331980525\n" +
                                        "So your table id will be \"1RvKY3VE_y5FPhEECCa5dv4F7REJ7rBtGzQg9Z_B_DE4\" and sheet id will be \"331980525\" (gid parameter)!", MessageType.Info);
        
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Table ID: ");
                _settings.TableId = EditorGUILayout.TextField(_settings.TableId);
                EditorGUILayout.EndHorizontal();
        
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Sheet ID: ");
                _settings.SheetId = EditorGUILayout.TextField(_settings.SheetId);
                EditorGUILayout.EndHorizontal();
        
                EditorGUILayout.HelpBox("Specify the path to the folder to save the files. If there is a split, then in table 1 column must contain a flag", MessageType.Info);
        
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Is split Internal and External: ");
                _settings.IsSplitInternalAndExternal = EditorGUILayout.Toggle(_settings.IsSplitInternalAndExternal);
                EditorGUILayout.EndHorizontal();
        
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Internal folder: ");
                _settings.InternalFolder = EditorGUILayout.TextField(_settings.InternalFolder);
                if (GUILayout.Button("Browse", GUILayout.MaxWidth(80)))
                    _settings.InternalFolder = EditorUtility.SaveFolderPanel("Select folder", Application.dataPath, "");
                EditorGUILayout.EndHorizontal();
        
                if (_settings.IsSplitInternalAndExternal)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("External folder: ");
                    _settings.ExternalFolder = EditorGUILayout.TextField(_settings.ExternalFolder);
                    if (GUILayout.Button("Browse", GUILayout.MaxWidth(80)))
                        _settings.ExternalFolder = EditorUtility.SaveFolderPanel("Select folder", Application.dataPath, "");
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();
        
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Start processing"))
                    _handler.StartProcessing();
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}