using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Text.RegularExpressions;
using MeetAndTalk.GlobalValue;
using System;
using System.Linq;

namespace MeetAndTalk
{
    public class DialogueUIManager : MonoBehaviour
    {
        public static DialogueUIManager Instance;
        public string ID;


        [Header("Dialogue UI")]
        public bool showSeparateName = false;
        public TextMeshProUGUI nameTextBox;
        public TextMeshProUGUI textBox;
        [Space()]
        public GameObject dialogueCanvas;
        public Slider TimerSlider;
        public GameObject SkipButton;
        public GameObject GoBackButton;

        [Header("Dynamic Dialogue UI")]
        public List<ChoiceTypeButton> BtnPrefabs = new List<ChoiceTypeButton>();
        public GameObject ButtonContainer;

        public List<PortraitUIClass> Portraits = new List<PortraitUIClass>();
        public UnityEvent StartDialogueEvent;
        public UnityEvent EndDialogueEvent;

        [HideInInspector] public string prefixText;
        [HideInInspector] public string fullText;
        private string currentText = "";
        private int characterIndex = 0;
        private float lastTypingTime;

        private List<Button> buttons = new List<Button>();
        private List<TextMeshProUGUI> buttonsTexts = new List<TextMeshProUGUI>();


        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void OnValidate()
        {
            foreach (AdvancedChoiceType type in Enum.GetValues(typeof(AdvancedChoiceType)))
            {
                bool exists = BtnPrefabs.Exists(btn => btn.Type == type);
                if (!exists)
                {
                    ChoiceTypeButton btn = new ChoiceTypeButton();
                    btn.Type = type;
                    BtnPrefabs.Add(btn);
                }
            }
        }

        private void Update()
        {
            textBox.text = prefixText + fullText;
            if (DialogueManager.Instance.listOfOpenedNodes.Count > 1) { GoBackButton.SetActive(true); } else { GoBackButton.SetActive(false); }
        }

        public void ResetText(string prefix)
        {
            currentText = prefix;
            prefixText = prefix;
            characterIndex = 0;
        }

        public void SetSeparateName(string name)
        {
            nameTextBox.text = name;
        }

        public void SetButtons(List<string> _texts, List<UnityAction> _unityActions, List<AdvancedChoiceType> _type, bool showTimer)
        {
            foreach (Transform child in ButtonContainer.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            for (int i = 0; i < _texts.Count; i++)
            {
                GameObject btn = null;

                Debug.Log(_type[i]);

                ChoiceTypeButton tmpButton = BtnPrefabs.FirstOrDefault(d => d.Type == _type[i]);
                GameObject tmp = tmpButton != null ? tmpButton.BtnPrefab : null;

                btn = Instantiate(tmp, ButtonContainer.transform);
                btn.transform.Find("Text").GetComponent<TMP_Text>().text = _texts[i];
                btn.gameObject.SetActive(true);
                btn.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                btn.GetComponent<Button>().onClick.AddListener(_unityActions[i]);
            }



        }
    }
}
