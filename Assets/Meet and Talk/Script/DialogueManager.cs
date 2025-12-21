using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using MeetAndTalk.GlobalValue;
using MeetAndTalk.Localization;

namespace MeetAndTalk
{
    public class DialogueManager : DialogueGetData
    {
        public static DialogueManager Instance;

        public LocalizationManager localizationManager;
        [Tooltip("Audio Source responsible for playing Dialogue Sounds")] public AudioSource dialogueAudioSource;
        [Tooltip("Audio Source responsible for playing Background Music")] public AudioSource musicAudioSource;
        [Tooltip("Default UI displayed until swapped")] public DialogueUIManager MainUI;

        public UnityEvent StartDialogueEvent;
        public UnityEvent EndDialogueEvent;

        // Private Value
        [HideInInspector] public DialogueUIManager dialogueUIManager;
        private List<Coroutine> activeCoroutines = new List<Coroutine>();

        private BaseNodeData currentDialogueNodeData;
        private BaseNodeData lastDialogueNodeData;
        public List<BaseNodeData> listOfOpenedNodes = new List<BaseNodeData>();
        public List<EventNodeData> listOfOpenedEvents = new List<EventNodeData>();

        private TimerChoiceNodeData tmpSavedTimeChoice;
        private AdvancedTimeChoiceNodeData tmpSavedAdvancedTimeChoice;
        private DialogueNodeData tmpSavedDialogue;
        private DialogueChoiceNodeData tmpSavedChoice;

        float Timer;

        private void Awake()
        {
            Instance = this;

            // Setup UI
            DialogueUIManager[] all = FindObjectsOfType<DialogueUIManager>();
            foreach (DialogueUIManager ui in all) { ui.gameObject.SetActive(false); }

            DialogueUIManager.Instance = MainUI;
            dialogueUIManager = DialogueUIManager.Instance;

            dialogueAudioSource = GetComponent<AudioSource>();

            // Music 
            musicAudioSource.clip = null;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }

        private void Update()
        {
            Timer -= Time.deltaTime;
            if (Timer > 0) dialogueUIManager.TimerSlider.value = Timer;
        }



        /// <summary>
        /// Pozwala na zmiane aktualnego UI Dialogu
        /// </summary>
        /// <param name="UI"></param>
        public void ChangeUI(DialogueUIManager UI)
        {
            // Setup UI
            if (UI != null) DialogueUIManager.Instance = UI;
            else Debug.LogError("DialogueUIManager.UI Object jest Pusty!");
        }

        /// <summary>
        /// Pozwala na zmiane aktualnego UI Dialogu
        /// </summary>
        /// <param name="UI"></param>
        public void ChangeUI(string UI)
        {
            List<DialogueUIManager> dialogueElements = new List<DialogueUIManager>(FindObjectsOfType<DialogueUIManager>(true));
            ChangeUI(dialogueElements.Find(d => d.ID == UI));
        }



        public void SetupDialogue(DialogueContainerSO dialogue)
        {
            if (dialogue != null) dialogueContainer = dialogue;
            else Debug.LogError("DialogueContainerSO.dialogue Object jest Pusty!");
        }

        public void StartDialogue(DialogueContainerSO dialogue) { StartDialogue(dialogue, ""); }
        public void StartDialogue() { StartDialogue(null, ""); }
        public void StartDialogue(DialogueContainerSO DialogueSO, string StartID)
        {
            StartID = "";

            // Reset Saved Nodes
            listOfOpenedNodes.Clear();
            listOfOpenedEvents.Clear();


            // Update Dialogue UI
            dialogueUIManager = DialogueUIManager.Instance;
            // Setup Dialogue (if not empty)
            if (DialogueSO != null) { SetupDialogue(DialogueSO); }
            // Error: No Setup Dialogue
            if (dialogueContainer == null) { Debug.LogError("Error: Dialogue Container SO is not assigned!"); }

            // Check ID
            if (dialogueContainer.StartNodeDatas.Count == 0) { Debug.LogError("Error: No Start Node in Dialogue Container!"); }

            BaseNodeData _start = null;
            if (StartID != "")
            {
                // IF FInd ID assign Data
                foreach (StartNodeData data in dialogueContainer.StartNodeDatas)
                {
                    if (data.startID == StartID) _start = data;
                }
            }
            if (_start == null)
            {
                _start = dialogueContainer.StartNodeDatas[Random.Range(0, dialogueContainer.StartNodeDatas.Count)];
            }

            // Pro Feature: Load Saved Dialogue
            string GUID = PlayerPrefs.GetString($"{dialogueContainer.name}_Progress");
            BaseNodeData _savedStart = null;
            bool ChangedFromSave = false;

            if (GUID != "" && dialogueContainer.AllowDialogueSave)
            {
                // Dialogue Is Ended
                if (GUID == "ENDED")
                {
                    // Ignore Dialogue
                    if (dialogueContainer.BlockingReopeningDialogue)
                    {
                        return;
                    }

                    // Normal Start (Start Node)
                    else { CheckNodeType(GetNextNode(_start)); }
                }
                // Dialogue is in Progress
                else
                {
                    _savedStart = GetNodeByGuid(GUID);
                    ChangedFromSave = true;
                }
            }
            // Start Dialoguw
            if (ChangedFromSave)
            {
                CheckNodeType(_savedStart);
            }
            else
            {
                // w/o Load From Save
                CheckNodeType(GetNextNode(_start));
            }

            // Enable UI
            dialogueUIManager.dialogueCanvas.SetActive(true);
            StartDialogueEvent.Invoke();

        }


        public void CheckNodeType(BaseNodeData _baseNodeData)
        {
            switch (_baseNodeData)
            {
                case StartNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case DialogueNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case DialogueChoiceNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case EndNodeData nodeData:
                    RunNode(nodeData);
                    break;
                default:
                    break;
            }
        }



        private void RunNode(StartNodeData _nodeData)
        {
            string GUID = _nodeData.NodeGuid;

            // Reset Audio
            dialogueAudioSource.Stop();

            CheckNodeType(GetNextNode(_nodeData));
        }
        private void RunNode(DialogueNodeData _nodeData)
        {
            // DIalogue Progress
            string GUID = _nodeData.NodeGuid;
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", GUID);

            // Dialogue History
            listOfOpenedNodes.Add(_nodeData);
            lastDialogueNodeData = currentDialogueNodeData;
            currentDialogueNodeData = _nodeData;

            // Display Dialogue Text
            dialogueUIManager.ResetText(_nodeData.TextType.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);
            dialogueUIManager.SkipButton.SetActive(true);

            // Display Portraits 
            //dialogueUIManager.SetupPortraits(_nodeData.Character, _nodeData.PortraitPosition, _nodeData.Emotion,
            //    _nodeData.SecoundCharacter, _nodeData.SecoundPortraitPosition, _nodeData.SecoundEmotion);

            // Doesn't generate buttons
            MakeButtons(new List<DialogueNodePort>());

            // Play Audio
            if (_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType != null) dialogueAudioSource.PlayOneShot(_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);

            // Select
            tmpSavedDialogue = _nodeData;

            // Stop All Coroutines
            StopAllTrackedCoroutines();

            // Gen to next Node
            IEnumerator tmp() { yield return new WaitForSeconds(_nodeData.Duration); DialogueNode_NextNode(); }
            if (_nodeData.Duration != 0) StartTrackedCoroutine(tmp()); ;
        }
        private void RunNode(DialogueChoiceNodeData _nodeData)
        {

            // Dialogue History
            listOfOpenedNodes.Add(_nodeData);
            lastDialogueNodeData = currentDialogueNodeData;
            currentDialogueNodeData = _nodeData;

            string GUID = _nodeData.NodeGuid;
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", GUID);

            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            // Normal Multiline
            if (dialogueUIManager.showSeparateName && dialogueUIManager.nameTextBox != null) { dialogueUIManager.ResetText(""); dialogueUIManager.SetSeparateName($"<color={_nodeData.Character.HexColor()}>{_nodeData.Character.characterName.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}</color>"); }
            // Normal Inline
            else if (_nodeData.Character != null) dialogueUIManager.ResetText($"<color={_nodeData.Character.HexColor()}>{_nodeData.Character.characterName.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}: </color>");
            // Last Change
            else dialogueUIManager.ResetText("");

            dialogueUIManager.ResetText($"{_nodeData.TextType.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}");

            dialogueUIManager.SkipButton.SetActive(true);
            MakeButtons(new List<DialogueNodePort>());

            tmpSavedChoice = _nodeData;

            StopAllTrackedCoroutines();

            IEnumerator tmp() { yield return new WaitForSeconds(_nodeData.Duration); ChoiceNode_GenerateChoice(); }
            StartTrackedCoroutine(tmp()); ;

            if (_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType != null) dialogueAudioSource.PlayOneShot(_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);
        }
        private void RunNode(EndNodeData _nodeData)
        {
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", "ENDED");

            switch (_nodeData.EndNodeType)
            {
                case EndNodeType.End:
                    dialogueUIManager.dialogueCanvas.SetActive(false);
                    EndDialogueEvent.Invoke();
                    break;
                case EndNodeType.ReturnToStart:
                    CheckNodeType(GetNextNode(dialogueContainer.StartNodeDatas[Random.Range(0, dialogueContainer.StartNodeDatas.Count)]));
                    break;
                case EndNodeType.NextDialogue:
                    StartDialogue(_nodeData.Dialogue, "");
                    break;
                default:
                    break;
            }
        }




        private void MakeButtons(List<DialogueNodePort> _nodePorts)
        {
            List<string> texts = new List<string>();
            List<UnityAction> unityActions = new List<UnityAction>();
            List<AdvancedChoiceType> choiceTypes = new List<AdvancedChoiceType>();

            foreach (DialogueNodePort nodePort in _nodePorts)
            {
                texts.Add(nodePort.TextLanguage.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);
                UnityAction tempAction = null;
                tempAction += () =>
                {
                    if (dialogueContainer.ResetSavedNodeOnChoice) listOfOpenedNodes.Clear();
                    CheckNodeType(GetNodeByGuid(nodePort.InputGuid));
                };
                choiceTypes.Add(AdvancedChoiceType.Normal);
                unityActions.Add(tempAction);
            }

            dialogueUIManager.SetButtons(texts, unityActions, choiceTypes, false);
        }


        void DialogueNode_NextNode() { CheckNodeType(GetNextNode(tmpSavedDialogue)); }
        void ChoiceNode_GenerateChoice()
        {
            MakeButtons(tmpSavedChoice.DialogueNodePorts);
            dialogueUIManager.SkipButton.SetActive(false);
        }

        #region Improve Coroutine
        private void StopAllTrackedCoroutines()
        {
            foreach (var coroutine in activeCoroutines)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }
            activeCoroutines.Clear();
        }

        private Coroutine StartTrackedCoroutine(IEnumerator coroutine)
        {
            Coroutine newCoroutine = StartCoroutine(coroutine);
            activeCoroutines.Add(newCoroutine);
            return newCoroutine;
        }
        #endregion

        public void SkipDialogue()
        {

            // Reset Audio
            dialogueAudioSource.Stop();

            StopAllTrackedCoroutines();

            switch (currentDialogueNodeData)
            {
                case DialogueNodeData nodeData:
                    DialogueNode_NextNode();
                    break;
                case DialogueChoiceNodeData nodeData:
                    ChoiceNode_GenerateChoice();
                    break;
                default:
                    break;
            }
        }
        public void ForceEndDialog()
        {
            // Reset Audio
            dialogueAudioSource.Stop();

            dialogueUIManager.dialogueCanvas.SetActive(false);
            EndDialogueEvent.Invoke();

            StopAllTrackedCoroutines();

            // Reset Audio
            dialogueAudioSource.Stop();
        }

        public void GoToPreviousNode()
        {
            if (listOfOpenedNodes.Count > 1)
            {
                // 
                StopAllTrackedCoroutines();

                // Reset Audio
                dialogueAudioSource.Stop();

                // 
                listOfOpenedNodes.RemoveAt(listOfOpenedNodes.Count - 1);
                string GUID = listOfOpenedNodes[listOfOpenedNodes.Count - 1].NodeGuid;
                listOfOpenedNodes.RemoveAt(listOfOpenedNodes.Count - 1);
                CheckNodeType(GetNodeByGuid(GUID));
            }
        }
    }
}
