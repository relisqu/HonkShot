using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using MeetAndTalk.Nodes;

namespace MeetAndTalk.Editor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueEditorWindow editorWindow;
        private DialogueGraphView graphView;

        public void Configure(DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Dialogue Node"),0),

            new SearchTreeGroupEntry(new GUIContent("Base Dialogue Node",EditorGUIUtility.FindTexture("d_FilterByType")),1),

            AddNodeSearchToGroup("Start Node",new StartNode(),"Icon/NodeIcon/Start"),
            AddNodeSearchToGroup("Dialogue Node",new DialogueNode(),"Icon/NodeIcon/Dialogue"),
            AddNodeSearchToGroup("Dialogue Choice Node",new DialogueChoiceNode(),"Icon/NodeIcon/Choice"),
            AddNodeSearchToGroup("End Node",new EndNode(),"Icon/NodeIcon/End")
        };

            return tree;
        }

        private SearchTreeEntry AddNodeSearchToGroup(string _name, BaseNode _baseNode, string IconName)
        {
            Texture2D _icon = Resources.Load<Texture2D>(IconName);
            SearchTreeEntry tmp = new SearchTreeEntry(new GUIContent(_name, _icon))
            {
                level = 2,
                userData = _baseNode
            };

            return tmp;
        }

        private SearchTreeEntry AddNodeSearch(string _name, BaseNode _baseNode, string IconName)
        {
            Texture2D _icon = EditorGUIUtility.FindTexture(IconName) as Texture2D;
            SearchTreeEntry tmp = new SearchTreeEntry(new GUIContent(_name, _icon))
            {
                level = 1,
                userData = _baseNode
            };

            return tmp;
        }

        public bool OnSelectEntry(SearchTreeEntry _searchTreeEntry, SearchWindowContext _context)
        {
            Vector2 mousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo
                (
                editorWindow.rootVisualElement.parent, _context.screenMousePosition - editorWindow.position.position
                );
            Vector2 graphMousePosition = graphView.contentViewContainer.WorldToLocal(mousePosition);

            return CheckForNodeType(_searchTreeEntry, graphMousePosition);
        }

        private bool CheckForNodeType(SearchTreeEntry _searchTreeEntry, Vector2 _pos)
        {
            switch (_searchTreeEntry.userData)
            {
                case StartNode node:
                    graphView.AddElement(StartNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
                case DialogueNode node:
                    graphView.AddElement(DialogueNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
                case DialogueChoiceNode node:
                    graphView.AddElement(DialogueChoiceNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
                case EndNode node:
                    graphView.AddElement(EndNode.CreateNewGraphNode(_pos, editorWindow, graphView));
                    return true;
            }
            return false;
        }
    }
}