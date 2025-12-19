using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using MeetAndTalk.Editor;
using System;
using System.Collections.Generic;

namespace MeetAndTalk.Nodes
{
    public class StartNode : BaseNode
    {
        // Node Value
        public string startID;
        // Node Fields
        private TextField idField;

        public StartNode() { }

        /// <summary>
        /// Constructor for the StartNode class, initializes the node with default properties, 
        /// UI elements, and validation capabilities.
        /// </summary>
        /// <param name="_position">The initial position of the node in the editor graph.</param>
        /// <param name="_editorWindow">The instance of the editor window managing this node.</param>
        /// <param name="_graphView">The graph view where this node is displayed.</param>
        public StartNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            // Assign references to the editor window and graph view for later use.
            editorWindow = _editorWindow;
            graphView = _graphView;

            // Configure the base properties of the node.
            title = "Dialogue Start";
            SetPosition(new Rect(_position, defualtNodeSize)); // Position the node in the graph view.
            nodeGuid = Guid.NewGuid().ToString(); // Generate a unique identifier for the node.

            // Add an output port to the node with a single connection capacity.
            AddOutputPort("Output", Port.Capacity.Single);

            // Add Better Title
            GenerateBetterTitle("Dialogue Start", "Dialogue Starting Point", "Icon/NodeIcon/Start");

            // Create and configure custom fields for the node.
            // Add an ID field for specifying the start node's identifier.
            idField = new TextField("Start ID"); // Create a text input field labeled "Start ID".
            idField.RegisterValueChangedCallback(value =>
            {
                startID = value.newValue; // Update the internal startID wshen the field value changes.
            });
            idField.SetValueWithoutNotify(startID); // Set the field's initial value without triggering the callback.
            idField.tooltip = "Start ID is available in the Pro Version\r\n" + "This feature and many others are not available in the free version of Meet and Talk";
            idField.SetEnabled(false); 
            mainContainer.Add(idField); // Add the ID field to the node's main container.

            elementTypeColor = new Color(.2f, .59f, .29f, 0.2f);

            // Refresh the node's state and UI.
            RefreshExpandedState(); // Update the expanded/collapsed state of the node.
            RefreshPorts(); // Ensure all ports are correctly configured and displayed.
            AddValidationContainer(); // Add containers for error and warning messages to the node.
        }

        /// <summary>
        /// Sets the value of the ID field without triggering any change callbacks.
        /// </summary>
        public override void LoadValueInToField()
        {
            idField.SetValueWithoutNotify(startID); // Synchronize the ID field with the internal startID value.
        }

        /// <summary>
        /// Factory method to create a new StartNode instance and initialize its properties.
        /// </summary>
        /// <param name="_position">The position of the new node in the graph editor.</param>
        /// <param name="_editorWindow">The editor window managing the graph.</param>
        /// <param name="_graphView">The graph view where the node will be added.</param>
        /// <returns>A new StartNode instance.</returns>
        public static StartNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            StartNode tmp = new StartNode(_position, _editorWindow, _graphView); // Create the StartNode instance.
            tmp.name = "Start"; // Set the node's name.
            tmp.ReloadLanguage(); // Reload any language-related fields.

            return tmp; // Return the newly created node.
        }

        /// <summary>
        /// Generates a StartNode based on saved data, restoring its state and properties.
        /// </summary>
        /// <param name="data">The serialized data to use for generating the node.</param>
        /// <param name="editor">The editor window managing the graph.</param>
        /// <param name="graph">The graph view where the node will be added.</param>
        /// <returns>A restored StartNode instance.</returns>
        public static StartNode GenerateNode(StartNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            // Generate a new StartNode using its factory method.
            StartNode newNode = StartNode.CreateNewGraphNode(data.Position, editor, graph);

            // Restore the node's unique identifier and start ID.
            newNode.nodeGuid = data.NodeGuid;
            newNode.startID = data.startID;

            // Load the restored values into the corresponding fields.
            newNode.LoadValueInToField();

            return newNode; // Return the generated node.
        }

        /// <summary>
        /// Serializes the node's state into a StartNodeData object for saving.
        /// </summary>
        /// <returns>A StartNodeData instance containing the node's serialized state.</returns>
        public StartNodeData SaveNodeData()
        {
            StartNodeData nodeData = new StartNodeData
            {
                NodeGuid = nodeGuid, // Save the node's unique identifier.
                Position = GetPosition().position, // Save the node's position in the graph.
                startID = startID, // Save the node's start ID.
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

            // Error: Check if the output port is connected to another node.
            Port port = outputContainer.Query<Port>().First(); // Retrieve the output port.
            if (!port.connected) error.Add("Output does not lead to any node"); // Add error if no connection exists.

            // Assign validation results to the node's error and warning lists.
            ErrorList = error;
            WarningList = warning;
        }
    }

}