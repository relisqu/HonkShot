using UnityEngine;
using UnityEditor;
using MeetAndTalk;
using MeetAndTalk.Event;
using MeetAndTalk.Localization;
using MeetAndTalk.Settings;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using MeetAndTalk.Demo;
using System.ComponentModel;
using JetBrains.Annotations;
//using OpenCover.Framework.Model; вообще чё за хуйня вообще ебать
using static MeetAndTalk.DialogueGetData;
using MeetAndTalk.GlobalValue;

#if UNITY_EDITOR

/* Updated Editors */

[CustomEditor(typeof(DemoInteraction)), CanEditMultipleObjects]
public class DemoInteractionEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        DemoInteraction script = (DemoInteraction)target;
        DialogueInspector.LoadStyle(root);
        DialogueInspector.DrawScript<DemoInteraction>(root, "Demo Script: Interaction", target, DialogueInspector.GetTinyIcon("Icon/MT_Demo"));

        VisualElement container = DialogueInspector.Group(root, "Script Settings");
        container.Add(new TextField("Interaction Text") { bindingPath = "InteractionText" });
        root.Add(new PropertyField(serializedObject.FindProperty("OnInteraction"), "On Interaction"));
        return root;
    }
}

[CustomEditor(typeof(DemoPlayer)), CanEditMultipleObjects]
public class DemoPlayerEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        DemoPlayer script = (DemoPlayer)target;
        DialogueInspector.LoadStyle(root);
        DialogueInspector.DrawScript<DemoPlayer>(root, "Demo Script: Player", target, DialogueInspector.GetTinyIcon("Icon/MT_Demo"));

        VisualElement movement = DialogueInspector.Group(root, "Movement Settings");
        movement.Add(new FloatField("Walk Speed") { bindingPath = "walkSpeed" });
        movement.Add(new FloatField("Run Speed") { bindingPath = "runSpeed" });
        movement.Add(new FloatField("Jump Power") { bindingPath = "jumpPower" });
        movement.Add(new FloatField("Gravity Power") { bindingPath = "gravity" });

        VisualElement camera = DialogueInspector.Group(root, "Camera Settings");
        camera.Add(new ObjectField("Player Camera") { objectType = typeof(Camera), bindingPath = "playerCamera" });
        camera.Add(new FloatField("Camera Rotation Speed") { bindingPath = "lookSpeed" });
        camera.Add(new FloatField("Camera X Rotation Limit") { bindingPath = "lookXLimit" });

        VisualElement localization = DialogueInspector.Group(root, "Localization Settings");
        localization.Add(new ObjectField("Language Text") { objectType = typeof(TMPro.TMP_Text), bindingPath = "Lanuage" });

        VisualElement interaction = DialogueInspector.Group(root, "Interaction Settings");
        interaction.Add(new FloatField("Interaction Range") { bindingPath = "InteractionRange" });
        interaction.Add(new ObjectField("Interaction UI") { objectType = typeof(GameObject), bindingPath = "InteractionUI" });
        interaction.Add(new ObjectField("Interaction Text") { objectType = typeof(TMPro.TMP_Text), bindingPath = "InteractionText" });

        VisualElement debug = DialogueInspector.Group(root, "Debug Value's");
        debug.Add(new Toggle("PLayer can Move?") { bindingPath = "canMove" });
        debug.Add(new Toggle("Player can Interact?") { bindingPath = "Interactable" });

        return root;
    }
}

[CustomEditor(typeof(triggerArea)), CanEditMultipleObjects]
public class triggerAreaEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        triggerArea script = (triggerArea)target;
        DialogueInspector.LoadStyle(root);
        DialogueInspector.DrawScript<triggerArea>(root, "Demo Script: Trigger Area", target, DialogueInspector.GetTinyIcon("Icon/MT_Demo"));

        root.Add(new PropertyField(serializedObject.FindProperty("OnEnter"), "On Enter"));
        root.Add(new PropertyField(serializedObject.FindProperty("OnExit"), "On Exit"));

        return root;
    }
}

[CustomEditor(typeof(DialogueContainerSO))]
public class DialogueContainerEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        DialogueContainerSO script = (DialogueContainerSO)target;
        DialogueInspector.LoadStyle(root);
        DialogueInspector.DrawScript<DialogueContainerSO>(root, "Dialogue Container", target, DialogueInspector.GetTinyIcon("Icon/MT_Demo"));


        VisualElement BaseSettings = DialogueInspector.FoldoutGroup(root, "Base Settings", DialogueInspector.GetTinyIcon("Icon/MT_Settings_Gizmo"));


        var allowDialogueSave = new Toggle("Allow Dialogue Save") { bindingPath = "AllowDialogueSave" };
        allowDialogueSave.SetEnabled(false);
        BaseSettings.Add(allowDialogueSave);

        var blockingReopeningDialogue = new Toggle("Blocking Reopening Dialogue") { bindingPath = "BlockingReopeningDialogue" };
        blockingReopeningDialogue.SetEnabled(false);
        BaseSettings.Add(blockingReopeningDialogue);

        BaseSettings.Add(new Toggle("Reset Saved Node On Choice") { bindingPath = "ResetSavedNodeOnChoice" });
        BaseSettings.Add(new Toggle("Limit Choice Option Per Node") { bindingPath = "LimitChoiceOptionPerNode" });
        BaseSettings.Add(new IntegerField("Max Choice Option Per Node") { bindingPath = "MaxChoiceOptionPerNode" });

        //root.Add(new DialogueInspector.CustomListView<NodeLinkData>(script.NodeLinkDatas, "Connection Link", "de", DialogueInspector.GetTinyIcon("Icon/MT_Demo"), 50));
        //VisualElement Connections = DialogueInspector.FoldoutGroup(root, "Connections Settings", DialogueInspector.GetTinyIcon("Icon/MT_Settings_Gizmo"));

        root.Add(DialogueInspector.CreateListView<NodeLinkData>(serializedObject.FindProperty("NodeLinkDatas"), root, "Connection Link", "Dialogue Starting Point", DialogueInspector.GetTinyIcon("Icon/MT_Demo"),true,true));

        VisualElement BaseNodes = DialogueInspector.Group(root, "Base Nodes");

        BaseNodes.Add(DialogueInspector.CreateListView<StartNodeData>(serializedObject.FindProperty("StartNodeDatas"), BaseNodes, "Dialogue Start", "Dialogue Starting Point", DialogueInspector.GetTinyIcon("Icon/MT_Node_Start_Gizmo"), true, true));
        BaseNodes.Add(DialogueInspector.CreateListView<DialogueNodeData>(serializedObject.FindProperty("DialogueNodeDatas"), BaseNodes, "Dialogue", "Base Node to display Dialogue", DialogueInspector.GetTinyIcon("Icon/MT_Node_Dialogue_Gizmo"), true, true));
        BaseNodes.Add(DialogueInspector.CreateListView<DialogueChoiceNodeData>(serializedObject.FindProperty("DialogueChoiceNodeDatas"), BaseNodes, "Dialogue Choice", "Dialogue with Rensponces", DialogueInspector.GetTinyIcon("Icon/MT_Node_Choice_Gizmo"), true, true));
        BaseNodes.Add(DialogueInspector.CreateListView<AdvancedChoiceNodeData>(serializedObject.FindProperty("AdvancedChoiceNodeDatas"), BaseNodes, "Advance Choice", "Advanced Responces", DialogueInspector.GetTinyIcon("Icon/MT_Node_Advance_Choice"), true, true));
        BaseNodes.Add(DialogueInspector.CreateListView<TimerChoiceNodeData>(serializedObject.FindProperty("TimerChoiceNodeDatas"), BaseNodes, "Dialogue Time Choice", "Dualogue with Time Limited Responces", DialogueInspector.GetTinyIcon("Icon/MT_Node_Timer_Gizmo"), true, true));
        BaseNodes.Add(DialogueInspector.CreateListView<AdvancedTimeChoiceNodeData>(serializedObject.FindProperty("AdvancedTimeChoiceNodeDatas"), BaseNodes, "Advanced Time Choice", "Advanced Limited Tieme Response", DialogueInspector.GetTinyIcon("Icon/MT_Node_Advance_TimeChoice"), true, true));
        BaseNodes.Add(DialogueInspector.CreateListView<EndNodeData>(serializedObject.FindProperty("EndNodeDatas"), BaseNodes, "Dialogue End", "End Dialogue Action", DialogueInspector.GetTinyIcon("Icon/MT_Node_End_Gizmo"), true, true));

        VisualElement TechnicalNodes = DialogueInspector.Group(root, "Technical Nodes");

        TechnicalNodes.Add(DialogueInspector.CreateListView<EventNodeData>(serializedObject.FindProperty("EventNodeDatas"), TechnicalNodes, "Event Invoker", "Triggers Global and Local Events", DialogueInspector.GetTinyIcon("Icon/MT_Node_Event_Gizmo"), true, true));
        TechnicalNodes.Add(DialogueInspector.CreateListView<RandomNodeData>(serializedObject.FindProperty("RandomNodeDatas"), TechnicalNodes, "Random Output", "Randomizes next Node", DialogueInspector.GetTinyIcon("Icon/MT_Node_Random_Gizmo"), true, true));
        TechnicalNodes.Add(DialogueInspector.CreateListView<IfNodeData>(serializedObject.FindProperty("IfNodeDatas"), TechnicalNodes, "Simple Branch", "Branch with a single Condition", DialogueInspector.GetTinyIcon("Icon/MT_Node_IF_Gizmo"), true, true));
        TechnicalNodes.Add(DialogueInspector.CreateListView<AdvancedIFNodeData>(serializedObject.FindProperty("AdvancedIFNodeDatas"), TechnicalNodes, "Advanced Branch", "Branch with a multiple Condition", DialogueInspector.GetTinyIcon("Icon/MT_Node_Advance_IF"), true, true));
        TechnicalNodes.Add(DialogueInspector.CreateListView<ResetSavedNodeData>(serializedObject.FindProperty("ResetSavedNodeDatas"), TechnicalNodes, "Reset Saved Node", "Disable \"Go Back\" Function", DialogueInspector.GetTinyIcon("Icon/MT_Node_ResetSavedNode"), true, true));
        TechnicalNodes.Add(DialogueInspector.CreateListView<MusicNodeData>(serializedObject.FindProperty("MusicNodeDatas"), TechnicalNodes, "Change Music", "Set Background Music", DialogueInspector.GetTinyIcon("Icon/MT_Node_Music_Gizmo"), true, true));

        VisualElement DecorationNodes = DialogueInspector.Group(root, "Decoration Nodes");
        DecorationNodes.Add(DialogueInspector.CreateListView<CommandNodeData>(serializedObject.FindProperty("CommandNodeDatas"), DecorationNodes, "Editor Notes", "\"Sticky Note\" for sving notes in the Editor", DialogueInspector.GetTinyIcon("Icon/MT_Node_Comment_Gizmo"), true, true));

        return root;
    }
}

[CustomEditor(typeof(DialogueCharacterSO))]
public class DialogueCharacterSOEditor : Editor
{
    bool Portrait = true;
    VisualElement GlobalName, NormalName;

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        DialogueCharacterSO script = (DialogueCharacterSO)target;
        DialogueInspector.LoadStyle(root);
        DialogueInspector.DrawScript<DialogueCharacterSO>(root, "Dialogue Character SO", target, DialogueInspector.GetTinyIcon("Icon/MT_Character_Gizmo"));

        VisualElement nameBox = DialogueInspector.Group(root, "Name Settings");

        GlobalName = new VisualElement();
        NormalName = new VisualElement();

        for(int i = 0; i < script.characterName.Count; i++)
        {
            NormalName.Add(new PropertyField(serializedObject.FindProperty("characterName").GetArrayElementAtIndex(i), "Global Value Name"));
        }

        nameBox.Add(GlobalName);
        nameBox.Add(NormalName);

        UpdateUI();

        PropertyField textColor = new PropertyField(serializedObject.FindProperty("textColor"), "Character Name Color");
        textColor.tooltip = "...";
        nameBox.Add(textColor);

        return root;

        void UpdateUI()
        {

                GlobalName.style.display = DisplayStyle.None;
                NormalName.style.display = DisplayStyle.Flex;
            
        }
    }
}


[CustomEditor(typeof(LocalizationDropdown)), CanEditMultipleObjects]
public class LocalizationDropdownEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        LocalizationDropdown script = (LocalizationDropdown)target;
        DialogueInspector.LoadStyle(root);
        DialogueInspector.DrawScript<LocalizationDropdown>(root, "Localization Dropdown", target, DialogueInspector.GetTinyIcon("Icon/MT_Localization_Gizmo"));

        root.Add(DialogueInspector.CreateInfoBox("Test", EditorGUIUtility.IconContent("console.infoicon").image, DialogueInspector.DialogueInspectorHintType.Normal));

        return root;
    }
}


[CustomEditor(typeof(LocalizedTextMeshProText)), CanEditMultipleObjects]
public class LocalizedTextMeshProTextEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        LocalizedTextMeshProText script = (LocalizedTextMeshProText)target;
        DialogueInspector.LoadStyle(root);
        DialogueInspector.DrawScript<LocalizedTextMeshProText>(root, "Localization TextMeshPro Text", target, DialogueInspector.GetTinyIcon("Icon/MT_Localization_Gizmo"));

        root.Add(DialogueInspector.CreateListView<LanguageGeneric<string>>(serializedObject.FindProperty("localizations"), root, "Localizations", false, false, false));
        root.Add(DialogueInspector.CreateInfoBox("Test", EditorGUIUtility.IconContent("console.infoicon").image, DialogueInspector.DialogueInspectorHintType.Normal));

        return root;
    }
}


[CustomEditor(typeof(DialogueGetData)), CanEditMultipleObjects]
public class DialogueGetDataEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        DialogueGetData script = (DialogueGetData)target;
        DialogueInspector.LoadStyle(root);
        DialogueInspector.DrawScript<DialogueGetData>(root, "Dialogue Get Data", target, DialogueInspector.GetTinyIcon("Icon/MT_Get_Data"));



        return root;
    }
}


[CustomEditor(typeof(DialogueManager)), CanEditMultipleObjects]
public class DialogueManagerEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        DialogueManager script = (DialogueManager)target;
        DialogueInspector.LoadStyle(root);
        DialogueInspector.DrawScript<DialogueManager>(root, "Dialogue Manager", target, DialogueInspector.GetTinyIcon("Icon/MT_Manager"));

        VisualElement settings = DialogueInspector.Group(root, "Base Settings");

        PropertyField localizationManager = new PropertyField(serializedObject.FindProperty("localizationManager"), "Localization Manager");
        localizationManager.tooltip = "XXX";
        settings.Add(localizationManager);
        PropertyField MainUI = new PropertyField(serializedObject.FindProperty("MainUI"), "Defualt UI");
        MainUI.tooltip = "XXX";
        settings.Add(MainUI);

        VisualElement audio = DialogueInspector.Group(root, "Audio Settings");

        PropertyField dialogueAudioSource = new PropertyField(serializedObject.FindProperty("dialogueAudioSource"), "Voice Audio Source");
        dialogueAudioSource.tooltip = "XXX";
        audio.Add(dialogueAudioSource);
        PropertyField musicAudioSource = new PropertyField(serializedObject.FindProperty("musicAudioSource"), "Music Audio Source");
        musicAudioSource.tooltip = "XXX";
        audio.Add(musicAudioSource);

        PropertyField StartDialogueEvent = new PropertyField(serializedObject.FindProperty("StartDialogueEvent"), "Start Dialogue Event");
        StartDialogueEvent.tooltip = "XXX";
        root.Add(StartDialogueEvent);

        PropertyField EndDialogueEvent = new PropertyField(serializedObject.FindProperty("EndDialogueEvent"), "End Dialogue Event");
        EndDialogueEvent.tooltip = "XXX";
        root.Add(EndDialogueEvent);

        return root;
    }
}


[CustomEditor(typeof(DialogueUIManager)), CanEditMultipleObjects]
public class DialogueUIManagerEditor : Editor
{
    VisualElement IDBox, NameTextBox, DialogTextBox, ButtonBox, CanvasBox, TimerBox, SkipBox, GoBackBox;

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        DialogueUIManager script = (DialogueUIManager)target;
        DialogueInspector.LoadStyle(root);
        DialogueInspector.DrawScript<DialogueUIManager>(root, "Dialogue UI Manager", target, DialogueInspector.GetTinyIcon("Icon/MT_UI_Manager"));

        serializedObject.Update();

        // ID Settings
        VisualElement idSettings = DialogueInspector.Group(root, "ID Settings");
        SerializedProperty idProperty = serializedObject.FindProperty("ID");
        IDBox = DialogueInspector.CreateInfoBox("Not assigned ID", EditorGUIUtility.IconContent("console.erroricon").image, DialogueInspector.DialogueInspectorHintType.Error);
        idSettings.Add(IDBox);
        PropertyField ID = new PropertyField(idProperty, "ID");
        idSettings.Add(ID);
        ID.RegisterValueChangeCallback(evt => UpdateVisibility(IDBox, !string.IsNullOrEmpty(idProperty.stringValue)));

        // Type Writing Settings
        VisualElement TypeWriting = DialogueInspector.Group(root, "Type Writing Settings");
        TypeWriting.Add(new PropertyField(serializedObject.FindProperty("EnableTypeWriting"), "Enable Type Writing"));
        TypeWriting.Add(new PropertyField(serializedObject.FindProperty("typingSpeed"), "Typing Speed"));

        // Dialogue Text Settings
        VisualElement TextGroup = DialogueInspector.Group(root, "Dialogue Text Settings");
        TextGroup.Add(new PropertyField(serializedObject.FindProperty("showSeparateName"), "Show Separate Name"));
        TextGroup.Add(new PropertyField(serializedObject.FindProperty("clearNameColor"), "Remove Color Name"));

        SerializedProperty nameTextProperty = serializedObject.FindProperty("nameTextBox");
        NameTextBox = DialogueInspector.CreateInfoBox("Not assigned Name Text Field", EditorGUIUtility.IconContent("console.erroricon").image, DialogueInspector.DialogueInspectorHintType.Error);
        //TextGroup.Add(NameTextBox);
        PropertyField nameTextBox = new PropertyField(nameTextProperty, "Name Text Field");
        TextGroup.Add(nameTextBox);
        nameTextBox.RegisterValueChangeCallback(evt => UpdateVisibility(NameTextBox, nameTextProperty.objectReferenceValue != null));

        SerializedProperty dialogTextProperty = serializedObject.FindProperty("textBox");
        DialogTextBox = DialogueInspector.CreateInfoBox("Not assigned Dialog Text Field", EditorGUIUtility.IconContent("console.erroricon").image, DialogueInspector.DialogueInspectorHintType.Error);
        TextGroup.Add(DialogTextBox);
        PropertyField textBox = new PropertyField(dialogTextProperty, "Dialog Text Field");
        TextGroup.Add(textBox);
        textBox.RegisterValueChangeCallback(evt => UpdateVisibility(DialogTextBox, dialogTextProperty.objectReferenceValue != null));

        // UI Settings
        VisualElement UIGroup = DialogueInspector.Group(root, "UI Settings");
        UIGroup.Add(DialogueInspector.CreateListView<PortraitUIClass>(serializedObject.FindProperty("BtnPrefabs"), UIGroup, "Button List", false, false, false));

        SerializedProperty dialogueCanvasProperty = serializedObject.FindProperty("dialogueCanvas");
        CanvasBox = DialogueInspector.CreateInfoBox("Not assigned Dialogue Canvas", EditorGUIUtility.IconContent("console.erroricon").image, DialogueInspector.DialogueInspectorHintType.Error);
        UIGroup.Add(CanvasBox);
        PropertyField dialogueCanvas = new PropertyField(dialogueCanvasProperty, "Dialogue Canvas");
        UIGroup.Add(dialogueCanvas);
        dialogueCanvas.RegisterValueChangeCallback(evt => UpdateVisibility(CanvasBox, dialogueCanvasProperty.objectReferenceValue != null));

        SerializedProperty timerSliderProperty = serializedObject.FindProperty("TimerSlider");
        TimerBox = DialogueInspector.CreateInfoBox("Not assigned Timer Slider", EditorGUIUtility.IconContent("console.erroricon").image, DialogueInspector.DialogueInspectorHintType.Error);
        UIGroup.Add(TimerBox);
        PropertyField TimerSlider = new PropertyField(timerSliderProperty, "Timer Slider");
        UIGroup.Add(TimerSlider);
        TimerSlider.RegisterValueChangeCallback(evt => UpdateVisibility(TimerBox, timerSliderProperty.objectReferenceValue != null));

        SerializedProperty skipButtonProperty = serializedObject.FindProperty("SkipButton");
        SkipBox = DialogueInspector.CreateInfoBox("Not assigned Skip Button", EditorGUIUtility.IconContent("console.erroricon").image, DialogueInspector.DialogueInspectorHintType.Error);
        UIGroup.Add(SkipBox);
        PropertyField SkipButton = new PropertyField(skipButtonProperty, "Skip Button");
        UIGroup.Add(SkipButton);
        SkipButton.RegisterValueChangeCallback(evt => UpdateVisibility(SkipBox, skipButtonProperty.objectReferenceValue != null));

        SerializedProperty goBackButtonProperty = serializedObject.FindProperty("GoBackButton");
        GoBackBox = DialogueInspector.CreateInfoBox("Not assigned GoBack Button", EditorGUIUtility.IconContent("console.erroricon").image, DialogueInspector.DialogueInspectorHintType.Error);
        UIGroup.Add(GoBackBox);
        PropertyField GoBackButton = new PropertyField(goBackButtonProperty, "GoBack Button");
        UIGroup.Add(GoBackButton);
        GoBackButton.RegisterValueChangeCallback(evt => UpdateVisibility(GoBackBox, goBackButtonProperty.objectReferenceValue != null));


        // Initial update
        UpdateVisibility(IDBox, !string.IsNullOrEmpty(idProperty.stringValue));
        UpdateVisibility(NameTextBox, nameTextProperty.objectReferenceValue != null);
        UpdateVisibility(DialogTextBox, dialogTextProperty.objectReferenceValue != null);
        UpdateVisibility(CanvasBox, dialogueCanvasProperty.objectReferenceValue != null);
        UpdateVisibility(TimerBox, timerSliderProperty.objectReferenceValue != null);
        UpdateVisibility(SkipBox, skipButtonProperty.objectReferenceValue != null);
        UpdateVisibility(GoBackBox, goBackButtonProperty.objectReferenceValue != null);

        return root;
    }

    private void UpdateVisibility(VisualElement element, bool isAssigned)
    {
        element.style.display = isAssigned ? DisplayStyle.None : DisplayStyle.Flex;
    }
}

[CustomEditor(typeof(GlobalValueInUI)), CanEditMultipleObjects]
public class GlobalValueInUIEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        GlobalValueInUI script = (GlobalValueInUI)target;
        DialogueInspector.LoadStyle(root);
        DialogueInspector.DrawScript<GlobalValueInUI>(root, "Global Value: TextMeshPro", target, DialogueInspector.GetTinyIcon("Icon/MT_GlobalValue_Gizmo"));


        root.Add(DialogueInspector.GlobalValueSelector(serializedObject.FindProperty("valueName")));

        PropertyField EndDialogueEvent = new PropertyField(serializedObject.FindProperty("Prefix"), "Prefix");
        EndDialogueEvent.tooltip = "XXX";
        root.Add(EndDialogueEvent);

        PropertyField Suffix = new PropertyField(serializedObject.FindProperty("Suffix"), "Suffix");
        Suffix.tooltip = "XXX";
        root.Add(Suffix);

        return root;
    }
}

[CustomEditor(typeof(Demo_MainMenu)), CanEditMultipleObjects]
public class Demo_MainMenuEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        Demo_MainMenu script = (Demo_MainMenu)target;
        DialogueInspector.LoadStyle(root);
        DialogueInspector.DrawScript<Demo_MainMenu>(root, "Demo Script: Main Menu", target, DialogueInspector.GetTinyIcon("Icon/MT_Demo"));

        PropertyField EndDialogueEvent = new PropertyField(serializedObject.FindProperty("NameInput"), "Name Input");
        EndDialogueEvent.tooltip = "XXX";
        root.Add(EndDialogueEvent);
        PropertyField NameGlobalValue = new PropertyField(serializedObject.FindProperty("NameGlobalValue"), "Name Input");
        NameGlobalValue.tooltip = "XXX";
        root.Add(NameGlobalValue);

        return root;
    }
}


[CustomPropertyDrawer(typeof(ChoiceTypeButton), true)]
public class ChoiceTypeButtonDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);

        root.style.flexDirection = FlexDirection.Row;

        PropertyField languageEnum = new PropertyField(property.FindPropertyRelative("Type"), "");
        languageEnum.style.width = 75;
        root.Add(languageEnum);

        var genericProperty = property.FindPropertyRelative("BtnPrefab");

        PropertyField LanguageGenericType = new PropertyField(genericProperty, "");
        LanguageGenericType.style.flexGrow = 1;
        LanguageGenericType.style.marginLeft = -3;
        root.Add(LanguageGenericType);


        return root;
    }
}


[CustomPropertyDrawer(typeof(PortraitUIClass), true)]
public class PortraitUIClassDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);


        PropertyField Position = new PropertyField(property.FindPropertyRelative("Position"), "Portrait Position");
        Position.tooltip = "XXX";
        root.Add(Position);

        PropertyField PortraitImage = new PropertyField(property.FindPropertyRelative("PortraitImage"), "Portrait Image");
        PortraitImage.tooltip = "XXX";
        root.Add(PortraitImage);

        root.Add(DialogueInspector.CreateListView<GameObject>(property.FindPropertyRelative("HideIfPortraitEmpty"), root, "Hide IF Portrait is Empty", true, false, true));



        return root;
    }
}


#region Node Related Inspectors
[CustomPropertyDrawer(typeof(NodeLinkData))]
public class NodeLinkDataDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);

        root.Add(new PropertyField(property.FindPropertyRelative("BaseNodeGuid"), "Base Node"));
        root.Add(new PropertyField(property.FindPropertyRelative("TargetNodeGuid"), "Target Node"));

        return root;
    }
}

[CustomPropertyDrawer(typeof(EventScriptableObjectData))]
public class EventScriptableObjectDataDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);

        root.Add(new PropertyField(property.FindPropertyRelative("DialogueEventSO"), "Event SO"));

        return root;
    }
}

[CustomPropertyDrawer(typeof(LanguageGeneric<>),true)]
public class LanguageGenericDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);

        root.style.flexDirection = FlexDirection.Row;

        PropertyField languageEnum = new PropertyField(property.FindPropertyRelative("languageEnum"), "");
        languageEnum.style.width = 75;
        root.Add(languageEnum);

        var genericProperty = property.FindPropertyRelative("LanguageGenericType");

        if (genericProperty.propertyType == SerializedPropertyType.String)
        {
            TextField textField = new TextField { multiline = true };
            textField.style.flexGrow = 1;
            textField.style.marginLeft = -3;
            textField.bindingPath = genericProperty.propertyPath;
            root.Add(textField);
        }
        else
        {
            PropertyField LanguageGenericType = new PropertyField(genericProperty, "");
            LanguageGenericType.style.flexGrow = 1;
            LanguageGenericType.style.marginLeft = -3;
            root.Add(LanguageGenericType);
        }

        return root;
    }
}

[CustomPropertyDrawer(typeof(DialogueNodePort))]
public class DialogueNodePortDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);

        root.Add(DialogueInspector.ChoiceGUIDList(property.FindPropertyRelative("PortGuid").stringValue, property.FindPropertyRelative("InputGuid").stringValue, property.FindPropertyRelative("OutputGuid").stringValue));
        root.Add(DialogueInspector.CreateListView<LanguageGeneric<string>>(property.FindPropertyRelative("TextLanguage"), root, "Choice Texts", false, false, false));

        return root;
    }
}

[CustomPropertyDrawer(typeof(AdvancedDialogueNodePort))]
public class AdvancedDialogueNodePortDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);


        root.Add(DialogueInspector.ChoiceGUIDList(property.FindPropertyRelative("PortGuid").stringValue, property.FindPropertyRelative("InputGuid").stringValue, property.FindPropertyRelative("OutputGuid").stringValue));

        PropertyField ChoiceType = new PropertyField(property.FindPropertyRelative("ChoiceType"), "Choice Type");
        ChoiceType.tooltip = "Allows for defining the response type, making it possible to highlight key dialogue choices relevant to the story and various special behaviors, such as unique character reactions or alternative dialogue paths.";
        root.Add(ChoiceType);

        root.Add(DialogueInspector.CreateListView<LanguageGeneric<string>>(property.FindPropertyRelative("TextLanguage"), root, "Choice Texts", false, false, false));

        PropertyField Condition = new PropertyField(property.FindPropertyRelative("Condition"), "Condition");
        Condition.tooltip = "Defines the condition that must be met for the response to appear in the game.";
        root.Add(Condition);

        return root;
    }
}

[CustomPropertyDrawer(typeof(ConditionInputPort))]
public class ConditionInputPortDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);


        root.Add(DialogueInspector.ChoiceGUIDList(property.FindPropertyRelative("PortGuid").stringValue, property.FindPropertyRelative("InputGuid").stringValue, property.FindPropertyRelative("OutputGuid").stringValue));

        PropertyField Operation = new PropertyField(property.FindPropertyRelative("Operation"), "Operation");
        Operation.tooltip = "...";
        root.Add(Operation);

        return root;
    }
}

[CustomPropertyDrawer(typeof(ConditionClass))]
public class ConditionClassDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);
        root.style.flexDirection = FlexDirection.Row;

        PropertyField ValueName = new PropertyField(property.FindPropertyRelative("ValueName"), "");
        ValueName.tooltip = "...";
        ValueName.style.width = 120;
        root.Add(ValueName);

        PropertyField Operator = new PropertyField(property.FindPropertyRelative("Operator"), "");
        Operator.tooltip = "...";
        Operator.style.width = 25;
        Operator.style.marginLeft = -3;
        root.Add(Operator);

        PropertyField Value = new PropertyField(property.FindPropertyRelative("Value"), "");
        Value.tooltip = "...";
        Value.style.flexGrow = 1;
        Value.style.marginLeft = -3;
        root.Add(Value);

        return root;
    }
}
#endregion

#region Base Nodes Inspectors
[CustomPropertyDrawer(typeof(StartNodeData)), CanEditMultipleObjects]
public class StartNodeDataEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);

        root.Add(DialogueInspector.NodeGUID(property.FindPropertyRelative("NodeGuid").stringValue, property.FindPropertyRelative("Position").vector2Value));

        PropertyField startID = new PropertyField(property.FindPropertyRelative("startID"), "Start ID");
        startID.tooltip = "A value used to define an ID that can be used to start the dialogue from the appropriate point.";
        root.Add(startID);

        return root;
    }
}

[CustomPropertyDrawer(typeof(EndNodeData)), CanEditMultipleObjects]
public class EndNodeDataEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);

        root.Add(DialogueInspector.NodeGUID(property.FindPropertyRelative("NodeGuid").stringValue, property.FindPropertyRelative("Position").vector2Value));

        PropertyField EndNodeType = new PropertyField(property.FindPropertyRelative("EndNodeType"), "Dialogue End Type");
        EndNodeType.tooltip = "Defines the behavior that will be executed when the dialogue ends.";
        root.Add(EndNodeType);

        PropertyField Dialogue = new PropertyField(property.FindPropertyRelative("Dialogue"), "Dialogue End Type");
        Dialogue.tooltip = "Defines the next dialogue that will start when the current dialogue ends.";
        root.Add(Dialogue);

        PropertyField startID = new PropertyField(property.FindPropertyRelative("startID"), "Dialogue End Type");
        startID.tooltip = "An optional field that defines an ID used to find the appropriate starting point. If the ID exists in the next dialogue, the dialogue will start from that ID. If not, or if the field is empty, a random starting point will be used.";
        root.Add(startID);

        return root;
    }
}

[CustomPropertyDrawer(typeof(DialogueNodeData)), CanEditMultipleObjects]
public class DialogueNodeDataEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);

        root.Add(DialogueInspector.NodeGUID(property.FindPropertyRelative("NodeGuid").stringValue, property.FindPropertyRelative("Position").vector2Value));

        VisualElement characterGroup = DialogueInspector.Group(root, "Main Character");

        PropertyField Character = new PropertyField(property.FindPropertyRelative("Character"), "Character SO");
        Character.tooltip = "Character used to define the person speaking in the dialogue.";
        characterGroup.Add(Character);
        PropertyField PortraitPosition = new PropertyField(property.FindPropertyRelative("PortraitPosition"), "Portrait Position");
        PortraitPosition.tooltip = "Specifies the position where the main character's avatar will be displayed.";
        characterGroup.Add(PortraitPosition);
        PropertyField Emotion = new PropertyField(property.FindPropertyRelative("Emotion"), "Emotion");
        Emotion.tooltip = "Allows selecting one of the emotions available for the main character.";
        characterGroup.Add(Emotion);


        VisualElement altCharacterGroup = DialogueInspector.Group(root, "Secound Character");

        PropertyField SecoundCharacter = new PropertyField(property.FindPropertyRelative("SecoundCharacter"), "Dialogue");
        SecoundCharacter.tooltip = "Defines an additional character that can be displayed in the dialogue.";
        altCharacterGroup.Add(SecoundCharacter);
        PropertyField SecoundPortraitPosition = new PropertyField(property.FindPropertyRelative("SecoundPortraitPosition"), "Portrait Position");
        SecoundPortraitPosition.tooltip = "Specifies the position where the additional character's avatar will be displayed.";
        altCharacterGroup.Add(SecoundPortraitPosition);
        PropertyField SecoundEmotion = new PropertyField(property.FindPropertyRelative("SecoundEmotion"), "Emotion");
        SecoundEmotion.tooltip = "Allows selecting one of the emotions available for the additional character.";
        altCharacterGroup.Add(SecoundEmotion);

        root.Add(DialogueInspector.CreateListView<LanguageGeneric<string>>(property.FindPropertyRelative("TextType"), root, "Dialogue Content", false, false, false));
        root.Add(DialogueInspector.CreateListView<LanguageGeneric<AudioClip>>(property.FindPropertyRelative("AudioClips"), root, "Audio Clips", false, false, false));

        PropertyField Duration = new PropertyField(property.FindPropertyRelative("Duration"), "Display Time");
        Duration.tooltip = "Determines the time after which the dialogue moves to the next node. If set to 0, manual progression to the next node is required.";
        root.Add(Duration);

        return root;
    }
}

[CustomPropertyDrawer(typeof(DialogueChoiceNodeData)), CanEditMultipleObjects]
public class DialogueChoiceNodeDataEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);

        root.Add(DialogueInspector.NodeGUID(property.FindPropertyRelative("NodeGuid").stringValue, property.FindPropertyRelative("Position").vector2Value));

        VisualElement characterGroup = DialogueInspector.Group(root, "Main Character");

        PropertyField Character = new PropertyField(property.FindPropertyRelative("Character"), "Character SO");
        Character.tooltip = "Character used to define the person speaking in the dialogue.";
        characterGroup.Add(Character);
        PropertyField PortraitPosition = new PropertyField(property.FindPropertyRelative("PortraitPosition"), "Portrait Position");
        PortraitPosition.tooltip = "Specifies the position where the main character's avatar will be displayed.";
        characterGroup.Add(PortraitPosition);
        PropertyField Emotion = new PropertyField(property.FindPropertyRelative("Emotion"), "Emotion");
        Emotion.tooltip = "Allows selecting one of the emotions available for the main character.";
        characterGroup.Add(Emotion);


        VisualElement altCharacterGroup = DialogueInspector.Group(root, "Secound Character");

        PropertyField SecoundCharacter = new PropertyField(property.FindPropertyRelative("SecoundCharacter"), "Dialogue");
        SecoundCharacter.tooltip = "Defines an additional character that can be displayed in the dialogue.";
        altCharacterGroup.Add(SecoundCharacter);
        PropertyField SecoundPortraitPosition = new PropertyField(property.FindPropertyRelative("SecoundPortraitPosition"), "Portrait Position");
        SecoundPortraitPosition.tooltip = "Specifies the position where the additional character's avatar will be displayed.";
        altCharacterGroup.Add(SecoundPortraitPosition);
        PropertyField SecoundEmotion = new PropertyField(property.FindPropertyRelative("SecoundEmotion"), "Emotion");
        SecoundEmotion.tooltip = "Allows selecting one of the emotions available for the additional character.";
        altCharacterGroup.Add(SecoundEmotion);

        root.Add(DialogueInspector.CreateListView<LanguageGeneric<string>>(property.FindPropertyRelative("TextType"), root, "Dialogue Content", false, false, false));
        root.Add(DialogueInspector.CreateListView<LanguageGeneric<AudioClip>>(property.FindPropertyRelative("AudioClips"), root, "Audio Clips", false, false, false));

        root.Add(DialogueInspector.CreateListView<DialogueNodePort>(property.FindPropertyRelative("DialogueNodePorts"), root, "Choices", false, false));

        PropertyField Duration = new PropertyField(property.FindPropertyRelative("Duration"), "Display Time");
        Duration.tooltip = "Defines the time delay between displaying the dialogue text and showing the available choices. This allows for better control over the conversation flow.";
        root.Add(Duration);

        return root;
    }
}
#endregion









[CustomPropertyDrawer(typeof(EmotionClass))]
public class EmotionClassDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);

        PropertyField EmotionName = new PropertyField(property.FindPropertyRelative("EmotionName"), "Emotion Name");
        EmotionName.tooltip = "...";
        root.Add(EmotionName);
        PropertyField Portrait = new PropertyField(property.FindPropertyRelative("Portrait"), "Portrait");
        Portrait.tooltip = "...";
        root.Add(Portrait);

        return root;
    }
}


[CustomPropertyDrawer(typeof(OrientationPortrait))]
public class OrientationPortraitDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        DialogueInspector.LoadStyle(root);

        PropertyField SpriteImage = new PropertyField(property.FindPropertyRelative("SpriteImage"), "Sprite Portrait");
        SpriteImage.tooltip = "...";
        root.Add(SpriteImage);

        return root;
    }
}



#endif

