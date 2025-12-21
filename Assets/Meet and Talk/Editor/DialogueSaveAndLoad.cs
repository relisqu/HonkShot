using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using MeetAndTalk.Nodes;

namespace MeetAndTalk.Editor
{
#if UNITY_EDITOR

    public class DialogueSaveAndLoad
    {
        private List<Edge> edges => graphView.edges.ToList();
        private List<BaseNode> nodes => graphView.nodes.ToList().Where(node => node is BaseNode).Cast<BaseNode>().ToList();

        public DialogueGraphView graphView;

        public DialogueSaveAndLoad(DialogueGraphView _graphView)
        {
            graphView = _graphView;
        }

        public void Save(DialogueContainerSO _dialogueContainerSO)
        {
            SaveEdges(_dialogueContainerSO);
            SaveNodes(_dialogueContainerSO);

            EditorUtility.SetDirty(_dialogueContainerSO);
            AssetDatabase.SaveAssets();
        }
        public void Load(DialogueContainerSO _dialogueContainerSO)
        {
            ClearGraph();
            GenerateNodes(_dialogueContainerSO);
            ConnectNodes(_dialogueContainerSO);
        }

        #region Save

        public void SaveEdges(DialogueContainerSO _dialogueContainerSO)
        {
            _dialogueContainerSO.NodeLinkDatas.Clear();

            Edge[] connectedEdges = edges.Where(edge => edge.input.node != null).ToArray();
            for (int i = 0; i < connectedEdges.Count(); i++)
            {
                BaseNode outputNode = (BaseNode)connectedEdges[i].output.node;
                BaseNode inputNode = connectedEdges[i].input.node as BaseNode;

                _dialogueContainerSO.NodeLinkDatas.Add(new NodeLinkData
                {
                    BaseNodeGuid = outputNode.nodeGuid,
                    TargetNodeGuid = inputNode.nodeGuid
                });
            }
        }

        private void SaveNodes(DialogueContainerSO _dialogueContainerSO)
        {
            _dialogueContainerSO.DialogueChoiceNodeDatas.Clear();
            _dialogueContainerSO.DialogueNodeDatas.Clear();
            _dialogueContainerSO.EndNodeDatas.Clear();
            _dialogueContainerSO.StartNodeDatas.Clear();

            nodes.ForEach(node =>
            {
                switch (node)
                {
                    case DialogueChoiceNode dialogueChoiceNode:
                        _dialogueContainerSO.DialogueChoiceNodeDatas.Add(dialogueChoiceNode.SaveNodeData(edges));
                        break;
                    case DialogueNode dialogueNode:
                        _dialogueContainerSO.DialogueNodeDatas.Add(dialogueNode.SaveNodeData());
                        break;
                    case StartNode startNode:
                        _dialogueContainerSO.StartNodeDatas.Add(startNode.SaveNodeData());
                        break;
                    case EndNode endNode:
                        _dialogueContainerSO.EndNodeDatas.Add(endNode.SaveNodeData());
                        break;
                    default:
                        break;
                }
            });
        }

        #endregion

        #region Load

        private void ClearGraph()
        {
            edges.ForEach(edge => graphView.RemoveElement(edge));

            foreach (BaseNode node in nodes)
            {
                graphView.RemoveElement(node);
            }
        }

        private void GenerateNodes(DialogueContainerSO _dialogueContainer)
        {
            // Generate all StartNode
            foreach (StartNodeData node in _dialogueContainer.StartNodeDatas)
            {
                graphView.AddElement(StartNode.GenerateNode(node, graphView.editorWindow, graphView));
            }

            // Generate all EndNode
            foreach (EndNodeData node in _dialogueContainer.EndNodeDatas)
            {
                graphView.AddElement(EndNode.GenerateNode(node, graphView.editorWindow, graphView));
            }

            // Generate all DialogueNode
            foreach (DialogueNodeData node in _dialogueContainer.DialogueNodeDatas)
            {
                graphView.AddElement(DialogueNode.GenerateNode(node, graphView.editorWindow, graphView));
            }

            // Generate Dialogue Choice Node
            foreach (DialogueChoiceNodeData node in _dialogueContainer.DialogueChoiceNodeDatas)
            {
                graphView.AddElement(DialogueChoiceNode.GenerateNode(node, graphView.editorWindow, graphView));
            }
        }

        private void ConnectNodes(DialogueContainerSO _dialogueContainer)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                List<NodeLinkData> connections = _dialogueContainer.NodeLinkDatas.Where(edge => edge.BaseNodeGuid == nodes[i].nodeGuid).ToList();

                for (int j = 0; j < connections.Count; j++)
                {
                    string targetNodeGuid = connections[j].TargetNodeGuid;
                    BaseNode targetNode = nodes.First(node => node.nodeGuid == targetNodeGuid);

                    if (!(nodes[i] is DialogueChoiceNode))
                    {
                        LinkNodesTogether(nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);
                    }
                }
            }

            // ?

            List<DialogueChoiceNode> dialogueNodes = nodes.FindAll(node => node is DialogueChoiceNode).Cast<DialogueChoiceNode>().ToList();
            foreach (DialogueChoiceNode dialogueNode in dialogueNodes)
            {
                foreach (DialogueNodePort nodePort in dialogueNode.dialogueNodePorts)
                {
                    if (nodePort.InputGuid != string.Empty)
                    {
                        BaseNode targetNode = nodes.First(Node => Node.nodeGuid == nodePort.InputGuid);
                        LinkNodesTogether(nodePort.MyPort, (Port)targetNode.inputContainer[0]);
                    }
                }
            }
        }

        private void LinkNodesTogether(Port _outputPort, Port _inputPort)
        {
            Edge tempEdge = new Edge()
            {
                output = _outputPort,
                input = _inputPort
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            graphView.Add(tempEdge);
        }

        #endregion
    }

#endif
}
