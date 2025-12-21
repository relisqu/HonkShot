#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEditor.Callbacks;
using MeetAndTalk.Localization;
using MeetAndTalk.Settings;

namespace MeetAndTalk.Editor
{
    [ExecuteInEditMode]
    public class DialogueEditorWindow : EditorWindow
    {
        public DialogueContainerSO currentDialogueContainer;
        private DialogueGraphView graphView;
        private DialogueSaveAndLoad saveAndLoad;

        private LocalizationEnum languageEnum = LocalizationEnum.English;
        private Label nameOfDialogueContainer;
        private ToolbarMenu toolbarMenu;
        private ToolbarMenu toolbarTheme;
        HelpBox WarningLabel, ErrorLabel;
        Toolbar toolbar;

        public bool AutoSave = true;
        private float lastUpdateTime = 0f;

        private Box _box;
        private HelpBox _infoBox;
        private bool _NoEditInfo = false;

        public bool showSettings = true;

        public LocalizationEnum LanguageEnum { get => languageEnum; set => languageEnum = value; }



        [OnOpenAsset(1)]
        public static bool ShowWindow(int _instanceId, int line)
        {
            UnityEngine.Object item = EditorUtility.InstanceIDToObject(_instanceId);

            if (item is DialogueContainerSO && !Application.isPlaying)
            {
                DialogueEditorWindow window = (DialogueEditorWindow)GetWindow(typeof(DialogueEditorWindow));
                window.titleContent = new GUIContent("Dialogue Editor", EditorGUIUtility.FindTexture("d_Favorite Icon"));
                window.currentDialogueContainer = item as DialogueContainerSO;
                window.minSize = new Vector2(500, 250);
                window.Load();
            }
            else if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Can't Open a Dialogue", "Dialogue Editor can only be opened when the project is not on!\nTurn off Play Mode to open the Editor", "I understand");
            }

            return false;
        }

        private void OnEnable()
        {
            AutoSave =  Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").AutoSave;
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnResize);
            ContructGraphView();
            GenerateToolbar();
            Load();
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(graphView);
        }

        private void ContructGraphView()
        {
            graphView = new DialogueGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);

            saveAndLoad = new DialogueSaveAndLoad(graphView);
        }

        void OnResize(GeometryChangedEvent evt)
        {
            float width = rootVisualElement.resolvedStyle.width;

            if (width < 810)
            {
                toolbar.style.flexDirection = FlexDirection.Column;
                toolbar.RemoveFromClassList("normal");
                toolbar.AddToClassList("compact");
            }
            else
            {
                toolbar.style.flexDirection = FlexDirection.Row;
                toolbar.RemoveFromClassList("compact");
                toolbar.AddToClassList("normal");
            }
        }

        private void GenerateToolbar()
        {
            // Load Defualt Theme
            StyleSheet styleSheet = Resources.Load<StyleSheet>("Themes/DarkTheme");
            rootVisualElement.styleSheets.Add(styleSheet);

            // Create Toolbar
            toolbar = new Toolbar(); 

            VisualElement leftContainer = new VisualElement();
            VisualElement rightContainer = new VisualElement();

            leftContainer.style.flexDirection = FlexDirection.Row;
            leftContainer.style.flexGrow = 1;
            leftContainer.AddToClassList("section");
            rightContainer.style.flexDirection = FlexDirection.Row;
            rightContainer.style.flexGrow = 1;
            rightContainer.AddToClassList("section");

            // Toolbar: Add Save Button
            ToolbarButton saveBtn = new ToolbarButton()
            {
                text = "Save",
                name = "save_btn",
                tooltip = "Save Current Status Dialogue\n[Cannot be undone]"
            };
            saveBtn.clicked += () =>
            {
                Save();
                if (Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ManualSaveLogs) Debug.Log("Manual Save");
            };
            saveBtn.AddToClassList("ToolbarBtnIcon");
            leftContainer.Add(saveBtn);
            // Save Button: Add Button Icon
            Box saveIcon = new Box();
            saveIcon.style.backgroundImage = Resources.Load("Icon/Editor/Save") as Texture2D;
            saveIcon.AddToClassList("ToolbarIcon");
            saveBtn.Add(saveIcon);



            // Toolbar: Add Load Button
            ToolbarButton loadBtn = new ToolbarButton()
            {
                text = "Load",
                name = "load_btn",
                tooltip = "Load the last saved state of Dialogue\n[Cannot be undone]"
            };
            loadBtn.clicked += () =>
            {
                bool confirmed = EditorUtility.DisplayDialog("Load the Dialogue Save?", "Are you sure you want to load the dialogue saving?\nThis will delete all unsaved dialogue changes", "Confirm", "Cancel");

                if (confirmed)
                {
                    Load();
                }
            };
            loadBtn.AddToClassList("ToolbarBtnIcon");
            leftContainer.Add(loadBtn);
            // Save Button: Add Button Icon
            Box loadIcon = new Box();
            loadIcon.style.backgroundImage = Resources.Load("Icon/Editor/Reload") as Texture2D;
            loadIcon.AddToClassList("ToolbarIcon");
            loadBtn.Add(loadIcon);



            // Toolbar: Add Dialogue Icon
            Box dialogueIcon = new Box();
            dialogueIcon.style.backgroundImage = Resources.Load("Icon/MT_Dialogue_Gizmo") as Texture2D;
            dialogueIcon.AddToClassList("MatLogo");
            leftContainer.Add(dialogueIcon);

            // Toolbar: Add Dialogue Name
            nameOfDialogueContainer = new Label("");
            leftContainer.Add(nameOfDialogueContainer);
            nameOfDialogueContainer.AddToClassList("nameOfDialogueContainer");

            

            // Toolbar: Add Separator
            ToolbarSpacer _sep1 = new ToolbarSpacer();
            _sep1.style.width = 1;
            _sep1.style.backgroundColor = new Color(.19f, .19f, .19f, 1);
            leftContainer.Add(_sep1);



            // Toolbar: Add Localization Icon
            Box translateMenuIcon = new Box();
            translateMenuIcon.style.backgroundImage = Resources.Load("Icon/Editor/Translate") as Texture2D;
            translateMenuIcon.style.backgroundColor = new Color(0, 0, 0, 0);
            translateMenuIcon.AddToClassList("IconInToolbar"); 
            translateMenuIcon.style.unityBackgroundImageTintColor = new Color(.82f, .82f, .82f, 1);
            translateMenuIcon.tooltip = "Language selected for editing\n[Switch to another language to edit it";
            leftContainer.Add(translateMenuIcon);



            // Toolbar: Add Localization Enum
            toolbarMenu = new ToolbarMenu();
            toolbarMenu.name = "localization_enum";
            foreach (LocalizationEnum language in (LocalizationEnum[])Enum.GetValues(typeof(LocalizationEnum)))
            {
                toolbarMenu.menu.AppendAction(language.ToString(), new Action<DropdownMenuAction>(x => Language(language, toolbarMenu)));
            }
            leftContainer.Add(toolbarMenu);



            // Toolbar: Add Separator
            ToolbarSpacer _sep2 = new ToolbarSpacer();
            _sep2.style.width = 1;
            _sep2.style.backgroundColor = new Color(.19f, .19f, .19f, 1);
            leftContainer.Add(_sep2);



            // Toolbar: Add Localization Icon
            Box themeIcon = new Box();
            themeIcon.style.backgroundImage = Resources.Load("Icon/Editor/Theme") as Texture2D;
            themeIcon.style.backgroundColor = new Color(0, 0, 0, 0);
            themeIcon.AddToClassList("IconInToolbar");
            themeIcon.style.unityBackgroundImageTintColor = new Color(.82f, .82f, .82f, 1);
            themeIcon.tooltip = "Selected Editor Theme";
            leftContainer.Add(themeIcon);



            // Toolbar: Add Theme Enum
            toolbarTheme = new ToolbarMenu();
            toolbarTheme.name = "theme_enum";
            foreach (MeetAndTalkTheme theme in (MeetAndTalkTheme[])Enum.GetValues(typeof(MeetAndTalkTheme)))
            {
                toolbarTheme.menu.AppendAction(theme.ToString(), new Action<DropdownMenuAction>(x => ChangeTheme(theme, toolbarTheme)));
            }
            leftContainer.Add(toolbarTheme);

            ToolbarSpacer sep_3 = new ToolbarSpacer();
            leftContainer.Add(sep_3);



            // Toolbar: Add Flex Space 
            ToolbarSpacer sep_2 = new ToolbarSpacer();
            sep_2.style.flexGrow = 1;
            leftContainer.Add(sep_2);

            toolbar.Add(leftContainer);
            
            ToolbarSpacer sep_4 = new ToolbarSpacer();
            sep_4.style.flexGrow = 1;
            rightContainer.Add(sep_4);

            // Toolbar: Warning Indicator
            WarningLabel = new HelpBox("War", HelpBoxMessageType.Warning);
            WarningLabel.AddToClassList("Indicator");
            WarningLabel.AddToClassList("WarningIndicator");
            WarningLabel.tooltip = "Number of Warnings occurring in the Dialogue";
            rightContainer.Add(WarningLabel);

            // Toolbar: Error Indicator
            ErrorLabel = new HelpBox("Err", HelpBoxMessageType.Error);
            ErrorLabel.AddToClassList("Indicator");
            ErrorLabel.AddToClassList("ErrorIndicator");
            ErrorLabel.tooltip = "Number of Errors occurring in the Dialogue";
            rightContainer.Add(ErrorLabel);




            ToolbarButton importBtn = new ToolbarButton()
            {
                text = "Import",
                name = "import_btn",
                tooltip = "Import Dialogue Text from a .tsv file\n[Tab-Separated Values] File"
            };
            importBtn.SetEnabled(false);
            importBtn.clicked += () =>
            {
                // Save Actual Work
                Save();
                string path = EditorUtility.OpenFilePanel("Import Dialogue Localization File", Application.dataPath, "csv");
                if (path.Length != 0)
                {
                    //currentDialogueContainer.ImportText(path, currentDialogueContainer);
                }
            };
            Box importIcon = new Box();
            importIcon.style.backgroundImage = Resources.Load("Icon/Editor/Import") as Texture2D;
            importIcon.AddToClassList("ToolbarIcon");
            importBtn.Add(importIcon);
            rightContainer.Add(importBtn);

            ToolbarButton exportBtn = new ToolbarButton()
            {
                text = "Export",
                name = "export_btn",
                tooltip = "Export Dialogue text to .tsv\n[Tab-Separated Values] File"
            };
            exportBtn.SetEnabled(false);
            exportBtn.clicked += () =>
            {
                // Save Actual Work
                Save();

                string path = EditorUtility.SaveFilePanel("Export Dialogue Localization File", Application.dataPath, currentDialogueContainer.name, "csv");
                if (path.Length != 0)
                {
                    //currentDialogueContainer.GenerateCSV(path, currentDialogueContainer);
                }
            };
            Box exportIcon = new Box();
            exportIcon.style.backgroundImage = Resources.Load("Icon/Editor/Export") as Texture2D;
            exportIcon.AddToClassList("ToolbarIcon");
            exportBtn.Add(exportIcon);
            rightContainer.Add(exportBtn);



            // Toolbar: Add Separator
            ToolbarSpacer _sep3 = new ToolbarSpacer();
            _sep3.style.width = 1;
            _sep3.style.backgroundColor = new Color(.19f, .19f, .19f, 1);
            rightContainer.Add(_sep3);

            ToolbarButton translateBtn = new ToolbarButton()
            {
                text = "Auto Translate",
                name = "translate_btn",
                tooltip = "This feature, along with many other advanced options, is available exclusively in the Pro Version of Meet and Talk."
            };
            translateBtn.SetEnabled(false);
            Box translateIcon = new Box();
            translateIcon.style.backgroundImage = Resources.Load("Icon/Editor/AutoTranslate") as Texture2D;
            translateIcon.AddToClassList("ToolbarIcon");
            translateBtn.Add(translateIcon);
            translateBtn.AddToClassList("ToolbarBtnIcon");
            rightContainer.Add(translateBtn);


            ToolbarButton settingsBtn = new ToolbarButton()
            {
                name = "setting_btn",
                tooltip = "Edit Dialogue and Editor Settings"
            };
            settingsBtn.clicked += () =>
            {
                showSettings = !showSettings;
            };
            Box settingIcon = new Box();
            settingIcon.style.backgroundImage = Resources.Load("Icon/Editor/Gear") as Texture2D;
            settingIcon.AddToClassList("ToolbarIcon");
            settingsBtn.Add(settingIcon);
            settingsBtn.AddToClassList("ToolbarBtnIcon");
            rightContainer.Add(settingsBtn);
            toolbar.Add(rightContainer);

            rootVisualElement.Add(toolbar);

            Label version = new Label()
            {
                name = "version_text",
                text = "MEET AND TALK - FREE VERSION\nVersion 2.0.0a"
            };
            version.enableRichText = true;
            version.pickingMode = PickingMode.Ignore;
            rootVisualElement.Add(version);
        }

        private void OnGUI()
        {
            if (AutoSave && EditorApplication.timeSinceStartup - lastUpdateTime >= Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").AutoSaveInterval && !Application.isPlaying)
            {
                lastUpdateTime = (float)EditorApplication.timeSinceStartup;
                Save();
                if (Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").AutoSaveLogs) Debug.Log($"Auto Save [{DateTime.Now.ToString("HH:mm:ss")}]");
            }

            if (!Application.isPlaying)
            {
                _NoEditInfo = false;
                if (_box != null)
                {
                    rootVisualElement.Remove(_box);
                    _box = null;
                }

                if (_infoBox != null)
                {
                    rootVisualElement.Remove(_infoBox);
                    _infoBox = null;
                }

                graphView?.ValidateDialogue();

                if (showSettings && currentDialogueContainer != null && graphView != null)
                {
                    graphView.settingsBox.style.display = DisplayStyle.Flex;

                    // Update Value
                    UpdateSettingsBox();

                    //
                    if (currentDialogueContainer.LimitChoiceOptionPerNode) graphView.MaxChoice.style.display = DisplayStyle.Flex;
                    else graphView.MaxChoice.style.display = DisplayStyle.None;
                }
                else if (graphView != null) { graphView.settingsBox.style.display = DisplayStyle.None; }

                WarningLabel.text = graphView.Warnings.ToString();
                ErrorLabel.text = graphView.Errors.ToString();

                // SHow / Hide Indicators
                if (graphView.ShowError.value && graphView.Errors != 0) ErrorLabel.style.display = DisplayStyle.Flex; else ErrorLabel.style.display = DisplayStyle.None;
                if (graphView.ShowWarning.value && graphView.Warnings != 0) WarningLabel.style.display = DisplayStyle.Flex; else WarningLabel.style.display = DisplayStyle.None;

                // SHow / Hide Minimap
                if (graphView.ShowMinimap.value) graphView.minimap.style.display = DisplayStyle.Flex; else graphView.minimap.style.display = DisplayStyle.None;
            }

            if (Application.isPlaying && !_NoEditInfo)
            {
                _box = new Box();
                _box.StretchToParentSize();
                _box.style.backgroundColor = new StyleColor(new Color(0, 0, 0, .5f));
                rootVisualElement.Add(_box);

                _infoBox = new HelpBox("Dialogue Editor cannot be used when Play Mode is ON,\n TURN OFF the game to edit Dialogue", HelpBoxMessageType.Warning);
                _infoBox.AddToClassList("PlayWarning");
                _infoBox.StretchToParentSize();

                _infoBox.style.width = 300;
                _infoBox.style.height = 150;
                _infoBox.style.flexDirection = FlexDirection.Column;
                _infoBox.style.justifyContent = Justify.Center;

                rootVisualElement.Add(_infoBox);

                _NoEditInfo = true;
            }
        }

        void UpdateSettingsBox()
        {
            currentDialogueContainer.AllowDialogueSave = graphView.AllowSave.value;
            currentDialogueContainer.BlockingReopeningDialogue = graphView.BlockReopening.value;
            currentDialogueContainer.ResetSavedNodeOnChoice = graphView.ResetSavedOnChoice.value;
            currentDialogueContainer.LimitChoiceOptionPerNode = graphView.LimitChoice.value;
            currentDialogueContainer.MaxChoiceOptionPerNode = graphView.MaxChoice.value;
            graphView.ValidateDialogue();
            //Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").AutoSave = graphView.AutoSave.value;
        }
        void LoadSettingsBox()
        {
            graphView.AllowSave.value = currentDialogueContainer.AllowDialogueSave;
            graphView.BlockReopening.value = currentDialogueContainer.BlockingReopeningDialogue;
            graphView.ResetSavedOnChoice.value = currentDialogueContainer.ResetSavedNodeOnChoice;
            graphView.LimitChoice.value = currentDialogueContainer.LimitChoiceOptionPerNode;
            graphView.MaxChoice.value = currentDialogueContainer.MaxChoiceOptionPerNode;
            //graphView.AutoSave.value = Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").AutoSave;
        }

        private void Load()
        {
            if (currentDialogueContainer != null && !Application.isPlaying)
            {
                Language(LocalizationEnum.English, toolbarMenu);
                ChangeTheme(Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").Theme, toolbarTheme);
                nameOfDialogueContainer.text = "" + currentDialogueContainer.name;
                saveAndLoad.Load(currentDialogueContainer);

                if (Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").LoadLogs) Debug.Log($"Load {currentDialogueContainer.name}");
                LoadSettingsBox();
            }
        }
        public void Save()
        {
            if (currentDialogueContainer != null && !Application.isPlaying)
            {
                saveAndLoad.Save(currentDialogueContainer);
            }
        }
        private void Language(LocalizationEnum _language, ToolbarMenu _toolbarMenu)
        {
            toolbarMenu.text = _language.ToString().ToUpper() + "";
            languageEnum = _language;
            graphView.LanguageReload();
        }
        private void ChangeTheme(MeetAndTalkTheme _theme, ToolbarMenu _toolbarMenu)
        {
            toolbarTheme.text = _theme.ToString().ToUpper() + "";
            Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").Theme = _theme;

            rootVisualElement.styleSheets.Remove(rootVisualElement.styleSheets[rootVisualElement.styleSheets.count - 1]);
            rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>($"Themes/{_theme.ToString()}Theme"));

            graphView.UpdateTheme(_theme.ToString());
        }
    }
#endif
}