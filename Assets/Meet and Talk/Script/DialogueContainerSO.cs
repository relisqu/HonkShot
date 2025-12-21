using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MeetAndTalk.GlobalValue;
using MeetAndTalk.Localization;
using MeetAndTalk.Event;
using static MeetAndTalk.DialogueGetData;

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System.IO;
using System;
#endif


namespace MeetAndTalk
{
    [CreateAssetMenu(menuName = "Dialogue/New Dialogue")]
    [System.Serializable]
    public class DialogueContainerSO : ScriptableObject
    {
        // Pro Feature: Load Saved Dialogue
        public bool AllowDialogueSave = false;
        public bool BlockingReopeningDialogue = false;
        public bool ResetSavedNodeOnChoice = true;
        public bool LimitChoiceOptionPerNode = false;
        public int MaxChoiceOptionPerNode = 8;

        public List<NodeLinkData> NodeLinkDatas = new List<NodeLinkData>();

        public List<DialogueChoiceNodeData> DialogueChoiceNodeDatas = new List<DialogueChoiceNodeData>();
        public List<DialogueNodeData> DialogueNodeDatas = new List<DialogueNodeData>();
        public List<TimerChoiceNodeData> TimerChoiceNodeDatas = new List<TimerChoiceNodeData>();
        public List<EndNodeData> EndNodeDatas = new List<EndNodeData>();
        public List<EventNodeData> EventNodeDatas = new List<EventNodeData>();
        public List<StartNodeData> StartNodeDatas = new List<StartNodeData>();
        public List<RandomNodeData> RandomNodeDatas = new List<RandomNodeData>();
        public List<CommandNodeData> CommandNodeDatas = new List<CommandNodeData>();
        public List<IfNodeData> IfNodeDatas = new List<IfNodeData>();
        public List<AdvancedIFNodeData> AdvancedIFNodeDatas = new List<AdvancedIFNodeData>();
        public List<AdvancedChoiceNodeData> AdvancedChoiceNodeDatas = new List<AdvancedChoiceNodeData>();
        public List<AdvancedTimeChoiceNodeData> AdvancedTimeChoiceNodeDatas = new List<AdvancedTimeChoiceNodeData>();
        public List<ResetSavedNodeData> ResetSavedNodeDatas = new List<ResetSavedNodeData>();
        public List<MusicNodeData> MusicNodeDatas = new List<MusicNodeData>();

        public List<BaseNodeData> AllNodes
        {
            get
            {
                AllowDialogueSave = false;
                BlockingReopeningDialogue = false;

                List<BaseNodeData> tmp = new List<BaseNodeData>();
                tmp.AddRange(DialogueNodeDatas);
                tmp.AddRange(DialogueChoiceNodeDatas);
                tmp.AddRange(TimerChoiceNodeDatas);
                tmp.AddRange(EndNodeDatas);
                tmp.AddRange(EventNodeDatas);
                tmp.AddRange(StartNodeDatas);
                tmp.AddRange(RandomNodeDatas);
                tmp.AddRange(CommandNodeDatas);
                tmp.AddRange(IfNodeDatas);
                tmp.AddRange(AdvancedIFNodeDatas);
                tmp.AddRange(AdvancedChoiceNodeDatas);
                tmp.AddRange(AdvancedTimeChoiceNodeDatas);
                tmp.AddRange(ResetSavedNodeDatas);
                tmp.AddRange(MusicNodeDatas);

                return tmp;
            }
        }
    }


    [System.Serializable]
    public class NodeLinkData
    {
        public string BaseNodeGuid;
        public string TargetNodeGuid;
    }

    [System.Serializable]
    public class BaseNodeData
    {
        public string NodeGuid;
        public Vector2 Position;
    }

    [System.Serializable]
    public class DialogueChoiceNodeData : BaseNodeData
    {
        // Character
        public DialogueCharacterSO Character;
        public PortraitPosition PortraitPosition;
        public string Emotion;

        // Secound Character
        public DialogueCharacterSO SecoundCharacter;
        public PortraitPosition SecoundPortraitPosition;
        public string SecoundEmotion;

        // Dialogue Content
        public List<LanguageGeneric<AudioClip>> AudioClips;
        public List<LanguageGeneric<string>> TextType;

        // Display Settings
        public float Duration;

        public List<DialogueNodePort> DialogueNodePorts;
    }

    [System.Serializable]
    public class AdvancedChoiceNodeData : BaseNodeData
    {
        public List<AdvancedDialogueNodePort> DialogueNodePorts;
    }

    [System.Serializable]
    public class TimerChoiceNodeData : BaseNodeData
    {
        // Character
        public DialogueCharacterSO Character;
        public PortraitPosition PortraitPosition;
        public string Emotion;

        // Secound Character
        public DialogueCharacterSO SecoundCharacter;
        public PortraitPosition SecoundPortraitPosition;
        public string SecoundEmotion;

        // Dialogue Content
        public List<LanguageGeneric<AudioClip>> AudioClips;
        public List<LanguageGeneric<string>> TextType;

        // Display Settings
        public float Duration;
        public bool ShowTimer = true;
        public float time;

        public List<DialogueNodePort> DialogueNodePorts;
    }

    [System.Serializable]
    public class AdvancedTimeChoiceNodeData : BaseNodeData
    {
        public List<AdvancedDialogueNodePort> DialogueNodePorts;
        public bool ShowTimer = true;
        public float time;
    }

    [System.Serializable]
    public class RandomNodeData : BaseNodeData
    {
        // No Extra Data
    }

    [System.Serializable]
    public class DialogueNodeData : BaseNodeData
    {
        // Character
        public DialogueCharacterSO Character;
        public PortraitPosition PortraitPosition;
        public string Emotion;

        // Secound Character
        public DialogueCharacterSO SecoundCharacter;
        public PortraitPosition SecoundPortraitPosition;
        public string SecoundEmotion;

        // Dialogue Content
        public List<LanguageGeneric<AudioClip>> AudioClips;
        public List<LanguageGeneric<string>> TextType;

        // Display Settings
        public float Duration;
    }

    [System.Serializable]
    public class EndNodeData : BaseNodeData
    {
        public EndNodeType EndNodeType;
        public DialogueContainerSO Dialogue;
        public string startID;
    }

    [System.Serializable]
    public class StartNodeData : BaseNodeData
    {
        public string startID;
    }


    [System.Serializable]
    public class EventNodeData : BaseNodeData
    {
        public List<EventScriptableObjectData> EventScriptableObjects;
    }
    [System.Serializable]
    public class EventScriptableObjectData
    {
        public DialogueEventSO DialogueEventSO;
    }

    [System.Serializable]
    public class CommandNodeData : BaseNodeData
    {
        public string commmand;
    }

    [System.Serializable]
    public class IfNodeData : BaseNodeData
    {
        public string ValueName;
        public GlobalValueIFOperations Operations;
        public string OperationValue;

        public string TrueGUID;
        public string FalseGUID;
    }
    [System.Serializable]
    public class AdvancedIFNodeData : BaseNodeData
    {
        public string TrueGUID;
        public string FalseGUID;
        public bool TrueWhenAllCondition;
        public List<ConditionInputPort> Conditions;
    }

    [System.Serializable]
    public class ResetSavedNodeData : BaseNodeData
    {

    }

    [System.Serializable]
    public class MusicNodeData : BaseNodeData
    {
        public List<LanguageGeneric<AudioClip>> AudioClips;
        public float SwitchTime;
    }



    [System.Serializable]
    public class LanguageGeneric<T>
    {
        public LocalizationEnum languageEnum;
        public T LanguageGenericType;
    }

    [System.Serializable]
    public class DialogueNodePort
    {
        public string PortGuid; //NOWE
        public string InputGuid;
        public string OutputGuid;
#if UNITY_EDITOR
        [HideInInspector] public Port MyPort;
#endif
        public TextField TextField;
        public List<LanguageGeneric<string>> TextLanguage = new List<LanguageGeneric<string>>();
    }

    [System.Serializable]
    public class AdvancedDialogueNodePort : DialogueNodePort
    {
        // Node POrt Base Value
        public AdvancedChoiceType ChoiceType = AdvancedChoiceType.Normal;
        public ConditionClass Condition;
        public float Delay = 0;
    }

    [System.Serializable]
    public class ConditionInputPort
    {
        public string PortGuid; //NOWE
        public string InputGuid;
        public string OutputGuid;
#if UNITY_EDITOR
        [HideInInspector] public Port MyPort;
#endif
        public ConditionClass Operation = new ConditionClass();
    }


    [System.Serializable]
    public enum EndNodeType
    {
        End = 0,
        ReturnToStart = 3,
        NextDialogue = 4
    }

    public enum AdvancedChoiceType
    {
        Normal = 0,
        Key = 1
    }

    [System.Serializable]
    public class ChoiceTypeButton
    {
        public AdvancedChoiceType Type;
        public GameObject BtnPrefab;
    }

    [System.Serializable]
    public class PortraitUIClass
    {
        public PortraitPosition Position;
        public GameObject PortraitImage;
        public List<GameObject> HideIfPortraitEmpty;
    }
}