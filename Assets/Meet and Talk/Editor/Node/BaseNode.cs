using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

using MeetAndTalk.Editor;
using MeetAndTalk.Settings;

using System.Collections.Generic;
using System;

namespace MeetAndTalk.Nodes
{
    public class BaseNode : Node
    {
        public string nodeGuid;
        protected Vector2 defualtNodeSize = new Vector2(200, 250);

        public List<string> ErrorList = new List<string>();
        public List<string> WarningList = new List<string>();

        protected string NodeGrid { get => NodeGrid; set => nodeGuid = value; }
        protected DialogueGraphView graphView;
        protected DialogueEditorWindow editorWindow;

        public Box NewTitleBox;

        public BaseNode()
        {

        }

        #region Port Functions

        public void AddInputPort(string name, Port.Capacity capality) { AddInputPort(name, "", capality, typeof(float)); }
        public void AddInputPort(string name, string className, Port.Capacity capality) { AddInputPort(name, className, capality, typeof(float)); }
        public void AddInputPort(string name, Port.Capacity capality, Type portType) { AddInputPort(name, "", capality, portType); }
        public void AddInputPort(string name, string className, Port.Capacity capality, Type portType)
        {
            // Create New Port
            Port inputPort = GetPortInstance(Direction.Input, capality, portType);

            // Set Port Name
            inputPort.portName = name;

            // Add Port Class if nessesery
            if (className != "") inputPort.AddToClassList(className);

            // Add New Port
            inputContainer.Add(inputPort);
        }



        public void AddOutputPort(string name, Port.Capacity capality) { AddOutputPort(name, "", capality, typeof(float)); }
        public void AddOutputPort(string name, string className, Port.Capacity capality) { AddOutputPort(name, className, capality, typeof(float)); }
        public void AddOutputPort(string name, Port.Capacity capality, Type portType) { AddOutputPort(name, "", capality, portType); }
        public void AddOutputPort(string name, string className, Port.Capacity capality, Type portType)
        {
            // Create New Port
            Port inputPort = GetPortInstance(Direction.Output, capality, portType);

            // Set Port Name
            inputPort.portName = name;

            // Add Port Class if nessesery
            if (className != "") inputPort.AddToClassList(className);

            // Add New Port
            outputContainer.Add(inputPort);
        }




        public Port GetPortInstance(Direction nodeDirection, Port.Capacity capacity)
        {
            return InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, typeof(float));
        }
        public Port GetPortInstance(Direction nodeDirection, Port.Capacity capacity, Type portType)
        {
            return InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, portType);
        }




        #endregion

        #region Validation Functions

        /// <summary>
        /// Adds a validation container to the node editor, including separate indicators 
        /// for errors and warnings, which can be dynamically updated based on validation results.
        /// </summary>
        public void AddValidationContainer()
        {
            // Create the main container for validation elements.
            VisualElement container = new VisualElement();
            container.name = "ValidationContainer";

            // Create the HelpBox for displaying errors.
            HelpBox ErrorContainer = new HelpBox("Empty Error", HelpBoxMessageType.Error);
            ErrorContainer.name = "ErrorContainer";
            ErrorContainer.style.display = DisplayStyle.None; // Initially hide the error container.
            container.Add(ErrorContainer); // Add the error container to the validation container.

            // Create the HelpBox for displaying warnings.
            HelpBox WarningContainer = new HelpBox("Empty Warning", HelpBoxMessageType.Warning);
            WarningContainer.name = "WarningContainer";
            WarningContainer.style.display = DisplayStyle.None; // Initially hide the warning container.
            container.Add(WarningContainer); // Add the warning container to the validation container.

            // Add the validation container to the title container of the node.
            NewTitleBox.Add(container);

            // Set the overflow style for the main container to ensure visibility of all elements.
            mainContainer.style.overflow = Overflow.Visible;
            NewTitleBox.style.overflow = Overflow.Visible;
        }


        /// <summary>
        /// Validates the current settings, checks for errors and warnings, and updates the visual display
        /// of error and warning messages in the editor.
        /// </summary>
        public void Validate()
        {
            // Set up validation logic, which populates ErrorList and WarningList.
            SetValidation();

            // Clear the ErrorList if error messages are disabled in the settings.
            if (!Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ShowErrors)
                ErrorList.Clear();

            // Clear the WarningList if warning messages are disabled in the settings.
            if (!Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").ShowWarnings)
                WarningList.Clear();

            //Debug.Log($"Erro Count: {ErrorList.Count}");
            //Debug.Log($"Erro Count: {ErrorList.Count}");

            // Find the HelpBox for displaying errors in the title container.
            HelpBox errorBox = NewTitleBox.Query<HelpBox>("ErrorContainer").First();
            if (errorBox != null)
            {
                // If there are no errors, hide the error HelpBox.
                if (ErrorList.Count < 1)
                {
                    errorBox.style.display = DisplayStyle.None;
                }
                else
                {
                    // If there are errors, show the error HelpBox and format the error messages.
                    errorBox.style.display = DisplayStyle.Flex;
                    string tmp = $"- ";
                    if (ErrorList.Count == 1) tmp = ""; // No bullet point for a single error.

                    for (int i = 0; i < ErrorList.Count; i++)
                    {
                        tmp += ErrorList[i]; // Add the error message.
                        if (i != ErrorList.Count - 1)
                            tmp += "\n- "; // Add a new line and bullet point for multiple errors.
                    }
                    errorBox.text = tmp; // Set the error text in the HelpBox.
                }
            }

            // Find the HelpBox for displaying warnings in the title container.
            HelpBox warningBox = NewTitleBox.Query<HelpBox>("WarningContainer").First();
            if (warningBox != null)
            {
                // If there are no warnings, hide the warning HelpBox.
                if (WarningList.Count < 1)
                {
                    warningBox.style.display = DisplayStyle.None;
                }
                else
                {
                    // If there are warnings, show the warning HelpBox and format the warning messages.
                    warningBox.style.display = DisplayStyle.Flex;
                    string tmp = $"- ";
                    if (WarningList.Count == 1) tmp = ""; // No bullet point for a single warning.

                    for (int i = 0; i < WarningList.Count; i++)
                    {
                        tmp += WarningList[i]; // Add the warning message.
                        if (i != WarningList.Count - 1)
                            tmp += "\n- "; // Add a new line and bullet point for multiple warnings.
                    }
                    warningBox.text = tmp; // Set the warning text in the HelpBox.
                }
            }
        }

        /// <summary>
        /// ...
        /// </summary>
        public virtual void SetValidation() { }

        #endregion

        #region Other functions
        /// <summary>
        /// Loads stored data into fields in the editor
        /// </summary>
        public virtual void LoadValueInToField() { }

        /// <summary>
        /// Refreshes the contents of the fields associated with the location
        /// </summary>
        public virtual void ReloadLanguage() { }

        public virtual void ExportToCSV(List<string> target) { }
        public virtual void ImportFromCSV(List<string> target) { }


        /// <summary>
        /// Function that changes the style of the nodes
        /// </summary>
        /// <param name=“name”></param>
        public void UpdateTheme(string name)
        {
            if (styleSheets[styleSheets.count - 1].name != "Node") styleSheets.Remove(styleSheets[styleSheets.count - 1]);
            styleSheets.Add(Resources.Load<StyleSheet>($"Themes/{name}Theme"));
        }
        #endregion

        public void GenerateBetterTitle(string Title, string Desc)
        {
            GenerateBetterTitle(Title, Desc, "Icon/Node");
        }

        public void GenerateBetterTitle(string Title, string Desc, string IconPath)
        {
            Box TItleBox = new Box();
            TItleBox.AddToClassList("MAT_Title");

            Box container = new Box();
            container.style.flexDirection = FlexDirection.Row;
            container.style.backgroundColor = new Color(0, 0, 0, 0);
            container.style.alignItems = Align.Center;

            Box Icon = new Box();
            Icon.AddToClassList("NodeIcon");
            Icon.style.backgroundImage = Resources.Load<Texture2D>(IconPath);

            /// TMP
            //Icon.style.display = DisplayStyle.None;
            /// TMP 


            Box TextContainer = new Box();
            TextContainer.AddToClassList("TextContainer");

            Label TitleField = new Label(Title);
            TitleField.AddToClassList("TitleText");
            Label DescField = new Label(Desc);
            DescField.AddToClassList("DescText");
            Button button = new Button()
            {
                text = "+ Add Condition"
            };

            container.Add(Icon);
            TextContainer.Add(TitleField);
            TextContainer.Add(DescField);
            container.Add(TextContainer);
            TItleBox.Add(container);
            //TItleBox.Add(button);
            mainContainer.Insert(1, TItleBox);

            NewTitleBox = TItleBox;
        }

        public virtual void UpdateNodeUI() { }
    }
}