using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using MeetAndTalk.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MeetAndTalk.Nodes
{
    [System.Serializable]
    public class EndNode : BaseNode
    {
        // Node Value
        private EndNodeType endNodeType = EndNodeType.End;
        public string startID;
        public DialogueContainerSO dialogue;
        public EndNodeType EndNodeType { get => endNodeType; set => endNodeType = value; }
        public HelpBox ProWarning;

        // Node Fields
        private EnumField enumField;
        private TextField idField;
        private Label dialogLabel;
        private ObjectField dialogField;

        public EndNode() { }

        /// <summary>
        /// Constructor for the EndNode class, initializes the node with default properties, 
        /// UI elements, and validation capabilities.
        /// </summary>
        /// <param name="_position">The initial position of the node in the editor graph.</param>
        /// <param name="_editorWindow">The instance of the editor window managing this node.</param>
        /// <param name="_graphView">The graph view where this node is displayed.</param>
        public EndNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            // Assign references to the editor window and graph view for later use.
            editorWindow = _editorWindow;
            graphView = _graphView;

            // Configure the base properties of the node.
            title = "Dialogue End";                                          // Set the node title.

            SetPosition(new Rect(_position, defualtNodeSize));      // Position the node in the graph view.
            nodeGuid = Guid.NewGuid().ToString();                   // Generate a unique identifier for the node.

            // Add Better Title
            GenerateBetterTitle("Dialogue End", "End Dialogue Action", "Icon/NodeIcon/End");

            // Add an output port to the node with a single connection capacity.
            AddInputPort("Input", Port.Capacity.Multi);

            // Create and configure custom fields for the node.
            // Add a label for the "EndNodeType" dropdown field
            Label enumLabel = new Label("Dialogue End Type");
            enumLabel.AddToClassList("label_name");         // Add class for styling
            enumLabel.AddToClassList("Label");              // Add additional class for general styling             // Add additional class for general styling
            extensionContainer.Add(enumLabel);                   // Add the label to the node's main container

            ProWarning = new HelpBox("Next Dialogue Option is Pro Version Exclusive\nIt works as \"END\" option", HelpBoxMessageType.Warning);
            ProWarning.AddToClassList("InNodeBox");
            mainContainer.Add(ProWarning);

            // Add the EndNodeType EnumField for selecting the type of end node
            enumField = new EnumField() { value = endNodeType };    // Initialize the EnumField with the current EndNodeType
            enumField.Init(endNodeType);                            // Initialize the field with the endNodeType value
            enumField.RegisterValueChangedCallback((value) =>
            {
                endNodeType = (EndNodeType)value.newValue;          // Update the endNodeType when the selection changes
                UpdateFieldDisplay();
            });
            enumField.SetValueWithoutNotify(endNodeType);           // Set the initial value without triggering a callback
            mainContainer.Add(enumField);



            // Add the EnumField to the node's main container

            // Add a label for the "Next Dialogue" field
            dialogLabel = new Label("Next Dialogue");
            dialogLabel.AddToClassList("label_name");       // Add class for styling
            dialogLabel.AddToClassList("Label");            // Add additional class for general styling
            //extensionContainer.Add(dialogLabel);                 // Add the label to the node's main container

            // Add an ObjectField for selecting the next dialogue container (DialogueContainerSO)
            dialogField = new ObjectField()
            {
                objectType = typeof(DialogueContainerSO),   // Specify the type of object allowed (DialogueContainerSO)
                allowSceneObjects = false,                  // Disallow selecting scene objects
                value = null,                               // Set the initial value to null
            };
            dialogField.RegisterValueChangedCallback(value =>
            {
                dialogue = value.newValue as DialogueContainerSO;   // Update the dialogue object when the selection changes
            });
            dialogField.SetValueWithoutNotify(dialogue);            // Set the initial value without triggering a callback
            dialogField.AddToClassList("EndDialogue");              // Add class for styling the object field
            dialogField.tooltip = "Next Dialogue is available in the Pro Version\r\n" + "This feature and many others are not available in the free version of Meet and Talk";
            dialogField.SetEnabled(false);
            mainContainer.Add(dialogField);                         // Add the ObjectField to the node's main container


            // Add an ID field for specifying the start node's identifier.
            idField = new TextField("Start ID");            // Create a text input field labeled "Start ID".
            idField.RegisterValueChangedCallback(value =>
            {
                startID = value.newValue;                   // Update the internal startID when the field value changes.
            });
            idField.SetValueWithoutNotify(startID);         // Set the field's initial value without triggering the callback.
            idField.AddToClassList("canCollapse");
            idField.tooltip = "Start ID is available in the Pro Version\r\n" + "This feature and many others are not available in the free version of Meet and Talk";
            idField.SetEnabled(false);
            mainContainer.Add(idField);                     // Add the ID field to the node's main container.

            // Refresh the node's state and UI.
            RefreshExpandedState();         // Update the expanded/collapsed state of the node.
            RefreshPorts();                 // Ensure all ports are correctly configured and displayed.
            AddValidationContainer();       // Add containers for error and warning messages to the node.
            UpdateFieldDisplay();
        }

        /// <summary>
        /// Sets the value of the ID field without triggering any change callbacks.
        /// </summary>
        public override void LoadValueInToField()
        {
            enumField.SetValueWithoutNotify(endNodeType);
            dialogField.SetValueWithoutNotify(dialogue);
            idField.SetValueWithoutNotify(startID);

            UpdateFieldDisplay();
        }

        /// <summary>
        /// Factory method to create a new EndNode instance and initialize its properties.
        /// </summary>
        /// <param name="_position">The position of the new node in the graph editor.</param>
        /// <param name="_editorWindow">The editor window managing the graph.</param>
        /// <param name="_graphView">The graph view where the node will be added.</param>
        /// <returns>A new EndNode instance.</returns>
        public static EndNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            EndNode tmp = new EndNode(_position, _editorWindow, _graphView);
            tmp.name = "End";
            tmp.ReloadLanguage();

            return tmp; // Return the newly created node.
        }

        /// <summary>
        /// Generates a EndNode based on saved data, restoring its state and properties.
        /// </summary>
        /// <param name="data">The serialized data to use for generating the node.</param>
        /// <param name="editor">The editor window managing the graph.</param>
        /// <param name="graph">The graph view where the node will be added.</param>
        /// <returns>A restored EndNode instance.</returns>
        public static EndNode GenerateNode(EndNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            // Generate a new EndNode using its factory method.
            EndNode newNode = EndNode.CreateNewGraphNode(data.Position, editor, graph);

            // Restore the node's unique identifier and start ID.
            newNode.nodeGuid = data.NodeGuid;
            newNode.EndNodeType = data.EndNodeType;
            newNode.startID = data.startID;
            newNode.dialogue = data.Dialogue;

            // Load the restored values into the corresponding fields.
            newNode.LoadValueInToField();

            return newNode; // Return the generated node.
        }

        /// <summary>
        /// Serializes the node's state into a EndNodeData object for saving.
        /// </summary>
        /// <returns>A EndNodeData instance containing the node's serialized state.</returns>
        public EndNodeData SaveNodeData()
        {
            EndNodeData nodeData = new EndNodeData
            {
                NodeGuid = nodeGuid,                    // Save the node's unique identifier.
                Position = GetPosition().position,      // Save the node's position in the graph.
                EndNodeType = EndNodeType,              // Save the node's EndNodeType enum.
                startID = startID,                      // Save the node's start ID.
                Dialogue = dialogue
            };
            return nodeData; // Return the serialized data.
        }

        /// <summary>
        /// Validates the node's connections and updates error and warning lists.
        /// </summary>
        public override void SetValidation()
        {
            // Reset validation lists for errors and warnings.
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            // Warning: Check if the output port is connected to another node.
            Port input = inputContainer.Query<Port>().First();
            if (!input.connected) warning.Add("Node cannot be called");

            // Error: Nie przypisane Dialogue Container SO
            if (endNodeType == EndNodeType.NextDialogue && dialogue == null)
                error.Add("Dialogue SO is Empty!");

            // Assign validation results to the node's error and warning lists.
            ErrorList = error;
            WarningList = warning;
        }

        void UpdateFieldDisplay()
        {
            if (endNodeType == EndNodeType.ReturnToStart)
            {
                idField.style.display = DisplayStyle.Flex;
                dialogField.style.display = DisplayStyle.None;
                ProWarning.style.display = DisplayStyle.None;
            }
            else if (endNodeType == EndNodeType.NextDialogue)
            {
                idField.style.display = DisplayStyle.Flex;
                dialogField.style.display = DisplayStyle.Flex;
                ProWarning.style.display = DisplayStyle.Flex;
            }
            else
            {
                idField.style.display = DisplayStyle.None;
                dialogField.style.display = DisplayStyle.None;
                ProWarning.style.display = DisplayStyle.None;
            }
        }
    }
}
