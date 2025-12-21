using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using MeetAndTalk.Nodes;
using UnityEditor.UIElements;
using UnityEditor;
using MeetAndTalk.Settings;

namespace MeetAndTalk.Editor
{
    [ExecuteInEditMode]
    public class DialogueGraphView : GraphView
    {
        // Reference to the Dialogue Editor Window
        public DialogueEditorWindow editorWindow;

        // Reference to the Node Search Window
        private NodeSearchWindow searchWindow;

        // Settings Box
        public Box settingsBox;
        public Toggle AutoSave, AllowSave, BlockReopening, ResetSavedOnChoice, LimitChoice, ShowMinimap, ShowWarning, ShowError;
        public IntegerField MaxChoice;

        public MiniMap minimap;

        //
        public int Errors, Warnings;

        // Constructor for the DialogueGraphView
        public DialogueGraphView(DialogueEditorWindow _editorWindow)
        {
            editorWindow = _editorWindow;

            StyleSheet tmpStyleSheet = Resources.Load<StyleSheet>("Themes/DarkTheme");
            styleSheets.Add(tmpStyleSheet);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            /*serializeGraphElements += CutCopyOperation;
            unserializeAndPaste += PasteOperation;
            canPasteSerializedData += CanPaste;*/
            ///unserializeAndPaste += OnPause

            //Debug.Log(canPaste);
            //Debug.Log(canCopySelection);

            GridBackground grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddSearchWindow();
            AddMiniMap();
            AddSettings();
        }

        /// <summary>
        /// Validates all nodes in the graph.
        /// </summary>
        public void ValidateDialogue()
        {
            Errors = 0;
            Warnings = 0;

            List<BaseNode> bases = nodes.ToList().Where(node => node is BaseNode).Cast<BaseNode>().ToList();
            foreach (BaseNode node in bases)
            {
                node.Validate();

                // Add to Indicators
                Warnings += node.WarningList.Count;
                Errors += node.ErrorList.Count;

                node.UpdateNodeUI();
            }


            //Debug.Log(canCopySelection);
        }



        /// <summary>
        /// Updates the theme for the graph and all nodes.
        /// </summary>
        /// <param name="name">The name of the new theme.</param>
        public void UpdateTheme(string name)
        {
            styleSheets.Remove(styleSheets[styleSheets.count - 1]);
            styleSheets.Add(Resources.Load<StyleSheet>($"Themes/{name}Theme"));

            List<BaseNode> bases = nodes.ToList().Where(node => node is BaseNode).Cast<BaseNode>().ToList();
            foreach (BaseNode node in bases) { node.UpdateTheme(name); }

        }

        private void AddMiniMap()
        {
            minimap = new MiniMap()
            {
                anchored = true,
                elementTypeColor = Color.green,
                name = "minimap",
                maxHeight = 100,
                maxWidth = 150
            };
            minimap.SetPosition(new Rect(0, 14, 200, 100));
            Add(minimap);
        }

        private void AddSettings()
        {
            settingsBox = new Box();
            settingsBox.AddToClassList("settingBox");
            settingsBox.style.width = 220;
            settingsBox.style.top = 34;
            settingsBox.style.right = 4;
            settingsBox.style.position = Position.Absolute;


            Label _title2 = new Label("Editor Settings");
            _title2.style.unityTextAlign = TextAnchor.MiddleCenter;
            _title2.style.fontSize = 10;
            _title2.style.unityFontStyleAndWeight = FontStyle.Bold;
            _title2.AddToClassList("title");
            settingsBox.Add(_title2);

            AutoSave = new Toggle($"Auto Save Dialogue [{Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").AutoSaveInterval} Sec]");
            AutoSave.RegisterValueChangedCallback(evt =>
            {
                editorWindow.AutoSave = evt.newValue;
                Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").AutoSave = evt.newValue;
                editorWindow.Save();
            });
            AutoSave.value = Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").AutoSave;
            settingsBox.Add(AutoSave);

            ShowMinimap = new Toggle($"Show Minimap in Dialogue Editor");
            ShowMinimap.RegisterValueChangedCallback(evt =>
            {
                Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ShowMinimap = evt.newValue;
            });
            ShowMinimap.value = Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ShowMinimap;
            settingsBox.Add(ShowMinimap);

            ShowWarning = new Toggle($"Show Warning's in Editor");
            ShowWarning.RegisterValueChangedCallback(evt =>
            {
                Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ShowWarnings = evt.newValue;
            });
            ShowWarning.value = Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ShowWarnings;
            settingsBox.Add(ShowWarning);

            ShowError = new Toggle($"Show Error's in Editor");
            ShowError.RegisterValueChangedCallback(evt =>
            {
                Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ShowErrors = evt.newValue;
            });
            ShowError.value = Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ShowErrors;
            settingsBox.Add(ShowError);

            Button settingsBtn = new Button();
            settingsBtn.text = "Open Advanced Meet and Talk Settings";
            settingsBtn.style.fontSize = 10;
            settingsBtn.clicked += () =>
            {
                SettingsService.OpenProjectSettings("Project/Meet and Talk");
            };
            settingsBox.Add(settingsBtn);


            Label _title = new Label("Dialogue Settings");
            _title.style.unityTextAlign = TextAnchor.MiddleCenter;
            _title.style.fontSize = 10;
            _title.style.unityFontStyleAndWeight = FontStyle.Bold;
            _title.AddToClassList("title");
            settingsBox.Add(_title);

            AllowSave = new Toggle("Allow Save Progression");
            AllowSave.SetEnabled(false);
            //AllowSave.value = editorWindow.currentDialogueContainer.AllowDialogueSave;
            settingsBox.Add(AllowSave);

            BlockReopening = new Toggle("Block Reopening when Ended");
            BlockReopening.SetEnabled(false);
            //BlockReopening.value = editorWindow.currentDialogueContainer.BlockingReopeningDialogue;
            settingsBox.Add(BlockReopening);

            ResetSavedOnChoice = new Toggle("Disable \"Go Back\" on after Choice");
            //ResetSavedOnChoice.value = editorWindow.currentDialogueContainer.ResetSavedNodeOnChoice;
            settingsBox.Add(ResetSavedOnChoice);

            LimitChoice = new Toggle("Limit Max Choice Count per Node");
            //LimitChoice.value = editorWindow.currentDialogueContainer.LimitChoiceOptionPerNode;
            settingsBox.Add(LimitChoice);

            MaxChoice = new IntegerField("Limit of Choice per Node");
            //MaxChoice.value = editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode;
            settingsBox.Add(MaxChoice);

            Add(settingsBox);
        }

        /// <summary>
        /// Adds a search window for node creation.
        /// </summary>
        private void AddSearchWindow()
        {
            // Create and configure the node search window
            searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            searchWindow.Configure(editorWindow, this);

            // Set the node creation request to open the search window
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        /// <summary>
        /// Gets compatible ports for connecting nodes.
        /// </summary>
        /// <param name="startPort">The starting port.</param>
        /// <param name="nodeAdapter">Adapter for node compatibility (unused).</param>
        /// <returns>A list of compatible ports.</returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            Port startPortView = startPort;

            // Iterate through all ports and find compatible ones
            ports.ForEach((port) =>
            {
                Port portView = port;

                // Ports are compatible if they are not on the same node and have opposite directions
                if (startPortView != portView && startPortView.node != portView.node && startPortView.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        /// <summary>
        /// Reloads language settings in all nodes.
        /// </summary>
        public void LanguageReload()
        {
            foreach (BaseNode node in nodes)
            {
                node.ReloadLanguage();
            }
        }

        // Note: Placeholder for transferring node creation logic to relevant classes
        // Transfer: CreateNewNode to Classes
    }
}
