using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using MeetAndTalk.Editor;
using MeetAndTalk.Localization;

namespace MeetAndTalk.Nodes
{
    public class DialogueChoiceNode : BaseNode
    {
        private List<LanguageGeneric<string>> texts = new List<LanguageGeneric<string>>();
        private List<LanguageGeneric<AudioClip>> audioClip = new List<LanguageGeneric<AudioClip>>();
        private float durationShow = 5;

        public List<DialogueNodePort> dialogueNodePorts = new List<DialogueNodePort>();

        public List<LanguageGeneric<string>> Texts { get => texts; set => texts = value; }
        public List<LanguageGeneric<AudioClip>> AudioClip { get => audioClip; set => audioClip = value; }
        public float DurationShow { get => durationShow; set => durationShow = value; }

        private TextField texts_Field;
        private ObjectField audioClips_Field;
        private FloatField duration_Field;

        // New Emotion System
        public DialogueCharacterSO character = ScriptableObject.CreateInstance<DialogueCharacterSO>();
        private ObjectField character_Field;
        public PortraitPosition PortraitPosition;
        public EnumField PortrainPositionField;
        public string Emotion;
        public PopupField<string> EmotionField;
        public List<string> EmotionList = new List<string>();

        public DialogueCharacterSO secoundCharacter = ScriptableObject.CreateInstance<DialogueCharacterSO>();
        private ObjectField secoundCharacter_Field;
        public PortraitPosition secoundPortraitPosition;
        public EnumField secoundPortrainPositionField;
        public string secoundEmotion;
        public PopupField<string> secoundEmotionField;
        public List<string> secoundEmotionList = new List<string>();

        public Button button;

        public DialogueChoiceNode()
        {

        }

        public DialogueChoiceNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;

            title = "Dialogue Choice";
            SetPosition(new Rect(_position, defualtNodeSize));
            nodeGuid = Guid.NewGuid().ToString();

            AddInputPort("Input ", Port.Capacity.Multi);

            // Add Better Title
            GenerateBetterTitle("Dialogue Choice", "Dialogue with Responses", "Icon/NodeIcon/Choice");

            AddValidationContainer();

            foreach (LocalizationEnum language in (LocalizationEnum[])Enum.GetValues(typeof(LocalizationEnum)))
            {
                texts.Add(new LanguageGeneric<string>
                {
                    languageEnum = language,
                    LanguageGenericType = ""
                });
                AudioClip.Add(new LanguageGeneric<AudioClip>
                {
                    languageEnum = language,
                    LanguageGenericType = null
                });
            }

            // CATEGORY LABEL: CHARACTER SETTINGS
            Label characterLabel = new Label("Character Settings");
            characterLabel.AddToClassList("label_texts");
            characterLabel.AddToClassList("Label");
            mainContainer.Add(characterLabel);

            // CHARACTER FIELD
            character_Field = new ObjectField("Character")
            {
                objectType = typeof(DialogueCharacterSO),
                allowSceneObjects = false,
            };
            character_Field.RegisterValueChangedCallback(value =>
            {
                character = value.newValue as DialogueCharacterSO;
            });
            character_Field.SetValueWithoutNotify(character);
            mainContainer.Add(character_Field);

            // PORTRAIT ENUM FIELD
            PortrainPositionField = new EnumField("Portrait Pos.") { value = PortraitPosition };
            PortrainPositionField.Init(PortraitPosition);
            PortrainPositionField.RegisterValueChangedCallback(value =>
            {
                PortraitPosition = (PortraitPosition)value.newValue;
                if (PortraitPosition == PortraitPosition.None) { EmotionField.SetEnabled(false); }
                else { EmotionField.SetEnabled(true); }
            });
            PortrainPositionField.SetValueWithoutNotify(PortraitPosition);
            PortrainPositionField.AddToClassList("Emotions");
            PortrainPositionField.tooltip = "Porrtrait Pos. is available in the Pro Version\r\n" + "This feature and many others are not available in the free version of Meet and Talk";
            PortrainPositionField.SetEnabled(false);
            mainContainer.Add(PortrainPositionField);

            // EMOTION FIELD
            EmotionField = new PopupField<string>("Emotion", EmotionList, 0);
            EmotionField.RegisterValueChangedCallback(value =>
            {
                Emotion = value.newValue;
            });
            EmotionField.AddToClassList("Emotions");



            // CHARACTER FIELD
            secoundCharacter_Field = new ObjectField("Alt. Character")
            {
                objectType = typeof(DialogueCharacterSO),
                allowSceneObjects = false,
            };
            secoundCharacter_Field.RegisterValueChangedCallback(value =>
            {
                secoundCharacter = value.newValue as DialogueCharacterSO;
            });
            secoundCharacter_Field.SetValueWithoutNotify(secoundCharacter);
            secoundCharacter_Field.tooltip = "Alt. Character is available in the Pro Version\r\n" + "This feature and many others are not available in the free version of Meet and Talk";
            secoundCharacter_Field.SetEnabled(false);
            mainContainer.Add(secoundCharacter_Field);

            // PORTRAIT ENUM FIELD
            secoundPortrainPositionField = new EnumField("Portrait Pos.") { value = secoundPortraitPosition };
            secoundPortrainPositionField.Init(PortraitPosition);
            secoundPortrainPositionField.RegisterValueChangedCallback(value =>
            {
                secoundPortraitPosition = (PortraitPosition)value.newValue;
                if (secoundPortraitPosition == PortraitPosition.None) { secoundEmotionField.SetEnabled(false); }
                else { secoundEmotionField.SetEnabled(true); }
            });
            secoundPortrainPositionField.SetValueWithoutNotify(secoundPortraitPosition);
            secoundPortrainPositionField.AddToClassList("Emotions");
            secoundPortrainPositionField.tooltip = "Alt. Portrait Pos. is available in the Pro Version\r\n" + "This feature and many others are not available in the free version of Meet and Talk";
            secoundPortrainPositionField.SetEnabled(false);
            mainContainer.Add(secoundPortrainPositionField);

            // EMOTION FIELD
            secoundEmotionField = new PopupField<string>("Emotion", secoundEmotionList, 0);
            secoundEmotionField.RegisterValueChangedCallback(value =>
            {
                secoundEmotion = value.newValue;
            });
            secoundEmotionField.AddToClassList("Emotions");


            /* TEXT BOX */
            VisualElement labelContainer = new VisualElement();
            labelContainer.AddToClassList("label-container");
            Image icon = new Image();
            icon.image = Resources.Load<Texture2D>("Icon/Editor/Translate"); // Œcie¿ka do pliku w Resources
            icon.AddToClassList("icon");
            Label label_texts = new Label("Dialogue Content");
            label_texts.AddToClassList("label_texts");
            labelContainer.Add(icon);
            labelContainer.Add(label_texts);
            mainContainer.Add(labelContainer);

            texts_Field = new TextField("");
            texts_Field.RegisterValueChangedCallback(value =>
            {
                texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue;
            });
            texts_Field.SetValueWithoutNotify(texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            texts_Field.multiline = true;

            texts_Field.AddToClassList("TextBox");
            mainContainer.Add(texts_Field);
            /* AUDIO CLIPS */
            audioClips_Field = new ObjectField()
            {
                objectType = typeof(AudioClip),
                allowSceneObjects = false,
                value = audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType,
            };
            audioClips_Field.RegisterValueChangedCallback(value =>
            {
                audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue as AudioClip;
            });
            audioClips_Field.SetValueWithoutNotify(audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            mainContainer.Add(audioClips_Field);


            /* Character Label */
            Label settingsLabel = new Label("Display Settings");
            settingsLabel.AddToClassList("label_texts");
            settingsLabel.AddToClassList("Label");
            mainContainer.Add(settingsLabel);
            /* Duration NAME */
            duration_Field = new FloatField("Display Time");
            duration_Field.RegisterValueChangedCallback(value =>
            {
                durationShow = value.newValue;
            });
            duration_Field.SetValueWithoutNotify(durationShow);

            duration_Field.AddToClassList("TextDuration");
            mainContainer.Add(duration_Field);

            button = new Button()
            {
                text = "+ Add Choice Option"
            };
            button.clicked += () =>
            {
                AddChoicePort(this);
            };

            NewTitleBox.Add(button);

            if (PortraitPosition == PortraitPosition.None) { EmotionField.SetEnabled(false); }
            else { EmotionField.SetEnabled(true); }
            if (secoundPortraitPosition == PortraitPosition.None) { secoundEmotionField.SetEnabled(false); }
            else { secoundEmotionField.SetEnabled(true); }

        }

        public override void ReloadLanguage()
        {
            texts_Field.RegisterValueChangedCallback(value =>
            {
                texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue;
            });
            texts_Field.SetValueWithoutNotify(texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);

            audioClips_Field.RegisterValueChangedCallback(value =>
            {
                audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue as AudioClip;
            });
            audioClips_Field.SetValueWithoutNotify(audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);

            foreach (DialogueNodePort nodePort in dialogueNodePorts)
            {
                nodePort.TextField.RegisterValueChangedCallback(value =>
                {
                    nodePort.TextLanguage.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue;
                });
                nodePort.TextField.SetValueWithoutNotify(nodePort.TextLanguage.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            }
        }

        public override void LoadValueInToField()
        {
            texts_Field.SetValueWithoutNotify(texts.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            audioClips_Field.SetValueWithoutNotify(audioClip.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            duration_Field.SetValueWithoutNotify(durationShow);

            character_Field.SetValueWithoutNotify(character);
            PortrainPositionField.SetValueWithoutNotify(PortraitPosition);
            EmotionField.SetValueWithoutNotify(Emotion);

            secoundCharacter_Field.SetValueWithoutNotify(secoundCharacter);
            secoundPortrainPositionField.SetValueWithoutNotify(secoundPortraitPosition);
            secoundEmotionField.SetValueWithoutNotify(secoundEmotion);
        }

        public Port AddChoicePort(BaseNode _basenote, DialogueNodePort _dialogueNodePort = null)
        {
            if ((editorWindow.currentDialogueContainer.LimitChoiceOptionPerNode && editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode > dialogueNodePorts.Count) || !editorWindow.currentDialogueContainer.LimitChoiceOptionPerNode)
            {

                Port port = GetPortInstance(Direction.Output, Port.Capacity.Single);

                string outputPortName = "";
                int outputPortCount = _basenote.outputContainer.Query("connector").ToList().Count();
                if (outputPortCount < 9) { outputPortName = $"Choice 0{outputPortCount + 1}"; }
                else { outputPortName = $"Choice {outputPortCount + 1}"; }

                DialogueNodePort dialogueNodePort = new DialogueNodePort();
                dialogueNodePort.PortGuid = Guid.NewGuid().ToString(); //NOWE

                foreach (LocalizationEnum language in (LocalizationEnum[])Enum.GetValues(typeof(LocalizationEnum)))
                {
                    dialogueNodePort.TextLanguage.Add(new LanguageGeneric<string>()
                    {
                        languageEnum = language,
                        LanguageGenericType = outputPortName
                    });
                }

                if (_dialogueNodePort != null)
                {
                    dialogueNodePort.InputGuid = _dialogueNodePort.InputGuid;
                    dialogueNodePort.OutputGuid = _dialogueNodePort.OutputGuid;

                    if (_dialogueNodePort.PortGuid == "") { _dialogueNodePort.PortGuid = Guid.NewGuid().ToString(); } //NOWE
                    dialogueNodePort.PortGuid = _dialogueNodePort.PortGuid; //NOWE

                    foreach (LanguageGeneric<string> languageGeneric in _dialogueNodePort.TextLanguage)
                    {
                        dialogueNodePort.TextLanguage.Find(language => language.languageEnum == languageGeneric.languageEnum).LanguageGenericType = languageGeneric.LanguageGenericType;
                    }
                }

                dialogueNodePort.TextField = new TextField();
                dialogueNodePort.TextField.RegisterValueChangedCallback(value =>
                {
                    dialogueNodePort.TextLanguage.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue;
                });
                dialogueNodePort.TextField.SetValueWithoutNotify(dialogueNodePort.TextLanguage.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);

                texts_Field.multiline = true;
                dialogueNodePort.TextField.AddToClassList("ChoiceLabel");
                port.contentContainer.Add(dialogueNodePort.TextField);

                Image icon = new Image();
                icon.image = Resources.Load<Texture2D>("Icon/Editor/Translate"); // Œcie¿ka do pliku w Resources
                icon.AddToClassList("icon");
                icon.style.marginLeft = 0;
                icon.style.marginRight = 0;
                port.contentContainer.Add(icon);


                Button deleteButton = new Button(() => DeleteButton(_basenote, port))
                {
                    text = "X"
                };
                port.contentContainer.Add(deleteButton);

#if UNITY_EDITOR
                dialogueNodePort.MyPort = port;
#endif
                port.portName = "";

                dialogueNodePorts.Add(dialogueNodePort);

                _basenote.outputContainer.Add(port);

                _basenote.RefreshPorts();
                _basenote.RefreshExpandedState();


                // SprawdŸ czy Node ma maksymaln¹ iloœæ Choice
                if (editorWindow.currentDialogueContainer.LimitChoiceOptionPerNode && editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode == dialogueNodePorts.Count) button.SetEnabled(false);
                else button.SetEnabled(true);
                button.text = UpdateButtonText();

                return port;
            }
            // Disable Add Choice
            return null;
        }

        private void DeleteButton(BaseNode _node, Port _port)
        {
#if UNITY_EDITOR
            DialogueNodePort tmp = dialogueNodePorts.Find(port => port.MyPort == _port);
            dialogueNodePorts.Remove(tmp);
#endif

            IEnumerable<Edge> portEdge = graphView.edges.ToList().Where(edge => edge.output == _port);

            if (portEdge.Any())
            {
                Edge edge = portEdge.First();
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                graphView.RemoveElement(edge);
            }

            _node.outputContainer.Remove(_port);

            _node.RefreshPorts();
            _node.RefreshExpandedState();

            // SprawdŸ czy Node ma maksymaln¹ iloœæ Choice
            if (editorWindow.currentDialogueContainer.LimitChoiceOptionPerNode && editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode == dialogueNodePorts.Count) button.SetEnabled(false);
            else button.SetEnabled(true);
            button.text = UpdateButtonText();
        }


        public static DialogueChoiceNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            DialogueChoiceNode tmp = new DialogueChoiceNode(_position, _editorWindow, _graphView); // Create the StartNode instance.
            tmp.name = "Choice";
            tmp.ReloadLanguage();

            return tmp;
        }
        public static DialogueChoiceNode GenerateNode(DialogueChoiceNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            DialogueChoiceNode tempNode = DialogueChoiceNode.CreateNewGraphNode(data.Position, editor, graph);
            tempNode.nodeGuid = data.NodeGuid;


            foreach (LanguageGeneric<string> languageGeneric in data.TextType)
            {
                tempNode.Texts.Find(language => language.languageEnum == languageGeneric.languageEnum).LanguageGenericType = languageGeneric.LanguageGenericType;
            }
            foreach (LanguageGeneric<AudioClip> languageGeneric in data.AudioClips)
            {
                tempNode.AudioClip.Find(language => language.languageEnum == languageGeneric.languageEnum).LanguageGenericType = languageGeneric.LanguageGenericType;
            }

            foreach (DialogueNodePort nodePort in data.DialogueNodePorts)
            {
                tempNode.AddChoicePort(tempNode, nodePort);
            }

            // Character
            tempNode.character = data.Character;
            tempNode.PortraitPosition = data.PortraitPosition;
            tempNode.Emotion = data.Emotion;

            // Secound Character
            tempNode.secoundCharacter = data.SecoundCharacter;
            tempNode.secoundPortraitPosition = data.SecoundPortraitPosition;
            tempNode.secoundEmotion = data.SecoundEmotion;

            tempNode.DurationShow = data.Duration;

            tempNode.LoadValueInToField();

            return tempNode; // Return the generated node.

        }
        public DialogueChoiceNodeData SaveNodeData(List<Edge> edges)
        {
            DialogueChoiceNodeData dialogueNodeData = new DialogueChoiceNodeData
            {
                // Base Settings
                NodeGuid = nodeGuid,
                Position = GetPosition().position,

                // Character
                Character = character,
                PortraitPosition = PortraitPosition,
                Emotion = Emotion,

                // Secound Character
                SecoundCharacter = secoundCharacter,
                SecoundPortraitPosition = secoundPortraitPosition,
                SecoundEmotion = secoundEmotion,

                // Dialogue Content
                TextType = Texts,
                AudioClips = AudioClip,

                // Display Settings
                Duration = DurationShow,
                DialogueNodePorts = dialogueNodePorts
            };
            foreach (DialogueNodePort nodePort in dialogueNodeData.DialogueNodePorts)
            {
                nodePort.OutputGuid = string.Empty;
                nodePort.InputGuid = string.Empty;
                foreach (Edge edge in edges)
                {
                    if (edge.output == nodePort.MyPort)
                    {
                        nodePort.OutputGuid = (edge.output.node as BaseNode).nodeGuid;
                        nodePort.InputGuid = (edge.input.node as BaseNode).nodeGuid;
                    }
                }
            }


            return dialogueNodeData;
        }

        public override void SetValidation()
        {
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            Port input = inputContainer.Query<Port>().First();
            if (!input.connected) warning.Add("Node cannot be called");
            if (dialogueNodePorts.Count < 1) error.Add("You need to add more Choice");
            else
            {
                for (int i = 0; i < dialogueNodePorts.Count; i++)
                {
                    if (!dialogueNodePorts[i].MyPort.connected) error.Add($"Choice ID:{i} does not lead to any node");
                }
            }
            for (int i = 0; i < Texts.Count; i++) { if (Texts[i].LanguageGenericType == "") warning.Add($"No Text for {Texts[i].languageEnum} Language"); }

            ErrorList = error;
            WarningList = warning;
        }

        string UpdateButtonText()
        {
            if (!editorWindow.currentDialogueContainer.LimitChoiceOptionPerNode)
                return "+ Add New Choice";
            else if (editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode > dialogueNodePorts.Count)
                return $"+ Add New Choice [{dialogueNodePorts.Count}/{editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode}]";
            else
                return $"Choice Limit [{dialogueNodePorts.Count}/{editorWindow.currentDialogueContainer.MaxChoiceOptionPerNode}]";
        }

        public override void UpdateNodeUI()
        {
            button.text = UpdateButtonText();
        }
    }
}