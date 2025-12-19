using MeetAndTalk.GlobalValue;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace MeetAndTalk
{
    public class DialogueGetData : MonoBehaviour
    {
        [HideInInspector] public DialogueContainerSO dialogueContainer;

        protected BaseNodeData GetNodeByGuid(string _targetNodeGuid)
        {
            return dialogueContainer.AllNodes.Find(node => node.NodeGuid == _targetNodeGuid);
        }

        protected BaseNodeData GetNodeByNodePort(DialogueNodePort _nodePort)
        {
            return dialogueContainer.AllNodes.Find(node => node.NodeGuid == _nodePort.InputGuid);
        }

        //protected BaseNodeData GetNextNode(BaseNodeData _baseNodeData) { return GetNextNode()}
        protected BaseNodeData GetNextNode(BaseNodeData _baseNodeData)
        {
            // ZnajdŸ wszystkie linki, które pasuj¹ do podanego wêz³a na podstawie BaseNodeGuid
            var matchingLinks = dialogueContainer.NodeLinkDatas
                .Where(link => link.BaseNodeGuid == _baseNodeData.NodeGuid)
                .ToList();

            // SprawdŸ, czy znaleziono jakiekolwiek dopasowania
            if (matchingLinks.Count == 0)
            {
                return null; // Brak pasuj¹cych wêz³ów, zwróæ null
            }

            // Jeœli jest wiêcej ni¿ jeden dopasowany link, losuj jeden z nich
            NodeLinkData selectedLink = matchingLinks.Count == 1
                ? matchingLinks[0] // Jeœli jest tylko jeden, wybierz go
                : matchingLinks[UnityEngine.Random.Range(0, matchingLinks.Count)]; // W przeciwnym razie losuj

            // U¿yj TargetNodeGuid z wybranego linku, aby pobraæ odpowiedni wêze³
            return GetNodeByGuid(selectedLink.TargetNodeGuid);
        }



        /// <summary>
        /// Retrieves the next node from the dialogue container based on the provided AdvancedTimeChoiceNodeData.
        /// Filters out links already associated with the node's dialogue ports to determine the next node.
        /// </summary>
        /// <param name="_node">The current AdvancedTimeChoiceNodeData whose next node is to be determined.</param>
        /// <returns>The BaseNodeData representing the next node in the dialogue flow.</returns>
        protected BaseNodeData GetNextNode(AdvancedTimeChoiceNodeData _node)
        {
            // Get all node links associated with the given node's GUID.
            var matchingLinks = dialogueContainer.NodeLinkDatas
                .Where(link => link.BaseNodeGuid == _node.NodeGuid)
                .ToList();

            // Variable to hold the selected link after filtering.
            NodeLinkData selectedLink = null;

            // Iterate through the dialogue node ports of the current node.
            foreach (var choice in _node.DialogueNodePorts)
            {
                // Remove links that are already connected to the node's choices.
                matchingLinks.Remove(matchingLinks.Find(linkData => linkData.TargetNodeGuid == choice.InputGuid));
            }

            // Select the first remaining link, if any, as the next node link.
            selectedLink = matchingLinks.FirstOrDefault();

            // Retrieve and return the node associated with the selected link's target GUID.
            return GetNodeByGuid(selectedLink.TargetNodeGuid);
        }



        /// <summary>
        /// Retrieves the next node from the dialogue container based on the provided TimerChoiceNodeData.
        /// Filters out links already associated with the node's dialogue ports to determine the next node.
        /// </summary>
        /// <param name="_node">The current TimerChoiceNodeData whose next node is to be determined.</param>
        /// <returns>The BaseNodeData representing the next node in the dialogue flow.</returns>
        protected BaseNodeData GetNextNode(TimerChoiceNodeData _node)
        {
            // Get all node links associated with the given node's GUID.
            var matchingLinks = dialogueContainer.NodeLinkDatas
                .Where(link => link.BaseNodeGuid == _node.NodeGuid)
                .ToList();

            // Variable to hold the selected link after filtering.
            NodeLinkData selectedLink = null;

            // Iterate through the dialogue node ports of the current node.
            foreach (var choice in _node.DialogueNodePorts)
            {
                // Remove links that are already connected to the node's choices.
                matchingLinks.Remove(matchingLinks.Find(linkData => linkData.TargetNodeGuid == choice.InputGuid));
            }

            // Select the first remaining link, if any, as the next node link.
            selectedLink = matchingLinks.FirstOrDefault();

            // Retrieve and return the node associated with the selected link's target GUID.
            return GetNodeByGuid(selectedLink.TargetNodeGuid);
        }



        /// <summary>
        /// Retrieves the next node from the dialogue container based on the provided AdvancedChoiceNodeData.
        /// Filters out links already associated with the node's dialogue ports to determine the next node.
        /// </summary>
        /// <param name="_node">The current AdvancedChoiceNodeData whose next node is to be determined.</param>
        /// <returns>The BaseNodeData representing the next node in the dialogue flow.</returns>
        protected BaseNodeData GetNextNode(AdvancedChoiceNodeData _node)
        {
            // Get all node links associated with the given node's GUID.
            var matchingLinks = dialogueContainer.NodeLinkDatas
                .Where(link => link.BaseNodeGuid == _node.NodeGuid)
                .ToList();

            // Variable to hold the selected link after filtering.
            NodeLinkData selectedLink = null;

            // Iterate through the dialogue node ports of the current node.
            foreach (var choice in _node.DialogueNodePorts)
            {
                // Remove links that are already connected to the node's choices.
                matchingLinks.Remove(matchingLinks.Find(linkData => linkData.TargetNodeGuid == choice.InputGuid));
            }

            // Select the first remaining link, if any, as the next node link.
            selectedLink = matchingLinks.FirstOrDefault();

            // Retrieve and return the node associated with the selected link's target GUID.
            return GetNodeByGuid(selectedLink.TargetNodeGuid);
        }





        [System.Serializable]
        public class ConditionClass
        {
            public string ValueName = "";
            public string Operator = "";
            public string Value = "";


            public VisualElement CreateVisualElement()
            {
                TextField operationValueField;
                DropdownField operationField;



                GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
                manager.LoadFile();

                List<string> valueNames = new List<string>();
                valueNames.AddRange(manager.IntValues.Select(intValue => intValue.ValueName));
                valueNames.AddRange(manager.FloatValues.Select(floatValue => floatValue.ValueName));
                valueNames.AddRange(manager.BoolValues.Select(boolValue => boolValue.ValueName));




                // G³ówny kontener dla elementów UI
                var container = new VisualElement();
                container.AddToClassList("condition-field");
                container.style.flexDirection = FlexDirection.Row;

                float width = 105;

                List<string> OptionList = new List<string>() { "Always True" };
                OptionList.AddRange(valueNames.Distinct().ToList());

                DropdownField valueNameField = new DropdownField(OptionList, 0);
                valueNameField.style.width = width;
                valueNameField.SetValueWithoutNotify(ValueName);
                valueNameField.AddToClassList("first");
                container.Add(valueNameField);

                // EnumField do wyboru operacji
                List<string> values = new List<string>() { "==", "!=", ">", "<", "<=", ">=" };
                operationField = new DropdownField(values, values.IndexOf(Operator));
                //operationField.Init(Operation);
                operationField.RegisterValueChangedCallback(evt =>
                {
                    Operator = evt.newValue;
                });
                operationField.style.width = 25;
                operationField.AddToClassList("middle");
                container.Add(operationField);

                // Pole tekstowe do edycji OperationValue
                operationValueField = new TextField();
                operationValueField.SetValueWithoutNotify(Value);
                operationValueField.RegisterValueChangedCallback(evt =>
                {
                    Value = evt.newValue;
                });
                operationValueField.style.width = width;
                operationValueField.AddToClassList("last");
                container.Add(operationValueField);


                valueNameField.RegisterValueChangedCallback(value =>
                {
                    ValueName = value.newValue;

                    // SprawdŸ, czy ValueName znajduje siê w manager.IntValues lub manager.FloatValues
                    bool isValueNameInIntValues = manager.IntValues.Any(intValue => intValue.ValueName == ValueName);
                    bool isValueNameInFloatValues = manager.FloatValues.Any(floatValue => floatValue.ValueName == ValueName);
                    bool isValueNameInStringValues = !isValueNameInIntValues && !isValueNameInFloatValues;

                    // Ustaw widocznoœæ AvatarPositionField w zale¿noœci od warunków
                    if (isValueNameInIntValues || isValueNameInFloatValues)
                    {
                        valueNameField.style.width = width;
                        valueNameField.style.marginRight = 1;
                        operationField.style.width = 25;
                        operationField.style.display = DisplayStyle.Flex;
                        operationValueField.style.width = width;
                        operationValueField.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        valueNameField.style.width = 239;
                        valueNameField.style.marginRight = 3;
                        operationField.style.display = DisplayStyle.None;
                        operationValueField.style.display = DisplayStyle.None;
                    }

                    operationField.style.display = isValueNameInIntValues || isValueNameInFloatValues ? DisplayStyle.Flex : DisplayStyle.None;
                    operationValueField.style.display = isValueNameInIntValues || isValueNameInFloatValues ? DisplayStyle.Flex : DisplayStyle.None;
                });



                return container;
            }

            public bool isValidated()
            {
                if (ValueName == "Always True") return true;
                if (ValueName == "" || Operator == "" || Value == "") return false;

                return true;
            }
        }
    }
}