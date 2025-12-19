using UnityEditor;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using static UnityEditor.PlayerSettings;
using MeetAndTalk.GlobalValue;

namespace MeetAndTalk
{
    public static class DialogueInspector
    {



#if UNITY_EDITOR

        public static void DrawScript<T>(VisualElement root, string name, UnityEngine.Object target, Texture icon) where T : UnityEngine.Object
        {
            // Główny kontener dla pola skryptu (HelpBox)
            var container = new HelpBox();
            container.AddToClassList("DrawScript");

            // Kontener dla ikony i etykiety
            var iconLabelContainer = new VisualElement();
            iconLabelContainer.AddToClassList("icon-label-container");

            // Ikona (32x32)
            if (icon != null)
            {
                var iconElement = new Image();
                iconElement.image = icon;
                iconElement.style.width = 32;
                iconElement.style.height = 32;
                iconElement.style.minHeight = 32;
                iconElement.style.minWidth = 32;
                iconElement.style.marginRight = 2; // Odstęp między ikoną a etykietą
                iconLabelContainer.Add(iconElement);
            }

            // Etykieta z nazwą skryptu
            var label = new Label(name);
            label.AddToClassList("DrawScript-Label");
            iconLabelContainer.Add(label);

            container.Add(iconLabelContainer);

            VisualElement separator = new VisualElement();
            separator.style.flexGrow = 1;
            container.Add(separator);

            // Przycisk do edycji skryptu
            var editButton = new Button(() => EditScript(target));
            editButton.AddToClassList("EditButton");
            Image icon_btn = new Image();
            icon_btn.image = EditorGUIUtility.IconContent("editicon.sml").image;          //editicon.sml
            editButton.Add(icon_btn);
            container.Add(editButton);

            void EditScript(UnityEngine.Object target)
            {
                if (target is MonoBehaviour monoBehaviourTarget)
                {
                    MonoScript monoScript = MonoScript.FromMonoBehaviour(monoBehaviourTarget);
                    if (monoScript != null)
                    {
                        AssetDatabase.OpenAsset(monoScript);
                    }
                }
                else if (target is ScriptableObject scriptableObjectTarget)
                {
                    MonoScript monoScript = MonoScript.FromScriptableObject(scriptableObjectTarget);
                    if (monoScript != null)
                    {
                        AssetDatabase.OpenAsset(monoScript);
                    }
                }
            }

            //container.Add(scriptField);
            root.Add(container);
        }

        public static void LoadStyle(VisualElement root)
        {
            StyleSheet style = null;

            if (style == null)
            {
                style = (StyleSheet)Resources.Load("DialogueInspectorStyle");
            }

            if (style != null)
            {
                root.styleSheets.Add(style);
            }
            else
            {
                Debug.LogError("KURWA");
            }
        }

        public static VisualElement CreateInfoBox(string text, Texture icon, DialogueInspectorHintType type)
        {
            // Główny kontener dla InfoBox
            var infoBox = new VisualElement();
            infoBox.AddToClassList("InfoBox");

            HelpBox IconBox = new HelpBox();
            IconBox.AddToClassList("IconBox");

            var Icon = new Image();
            Icon.image = icon;
            Icon.AddToClassList("Icon");
            IconBox.Add(Icon);

            HelpBox ContentBox = new HelpBox();
            ContentBox.text = text;
            ContentBox.AddToClassList("ContentBox");

            infoBox.Add(IconBox);
            infoBox.Add(ContentBox);

            if (type == DialogueInspectorHintType.Normal)
            {
                //IconBox.AddToClassList("WarningHint");
            }
            else if (type == DialogueInspectorHintType.Warning)
            {
                IconBox.AddToClassList("WarningHint");
                ContentBox.AddToClassList("WarningHint");
            }
            else if (type == DialogueInspectorHintType.Error)
            {
                IconBox.AddToClassList("ErrorHint");
                ContentBox.AddToClassList("ErrorHint");
            }
            else if (type == DialogueInspectorHintType.Success)
            {
                IconBox.AddToClassList("SuccessHint");
                ContentBox.AddToClassList("SuccessHint");
            }

            return infoBox;
        }

        public static VisualElement Group(VisualElement root, string title)
        {
            return Group(root, title, "", null);
        }

        public static VisualElement Group(VisualElement root, string title, string desc)
        {
            return Group(root, title, desc, null);
        }

        public static VisualElement Group(VisualElement root, string title, Texture icon)
        {
            return Group(root, title, "", icon);
        }

        public static VisualElement Group(VisualElement main, string title, string desc, Texture icon)
        {
            var root = new VisualElement();
            root.AddToClassList("Group");
            var mainBox = new HelpBox();
            mainBox.AddToClassList("Header");

            var IconBox = new HelpBox();
            IconBox.AddToClassList("IconBox");

            if (icon != null)
            {
                var iconContent = icon;
                if (iconContent != null)
                {
                    var Icon = new Image { image = iconContent };
                    IconBox.Add(Icon);
                    Icon.AddToClassList("Icon");
                    mainBox.Add(IconBox);
                }
            }

            // Kontener na tekst (tytuł + opcjonalny opis)
            var textContainer = new VisualElement();
            textContainer.AddToClassList("textContainer");
            mainBox.Add(textContainer);

            // Tytuł
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("Title");
            textContainer.Add(titleLabel);

            // Opcjonalny opis
            if (!string.IsNullOrEmpty(desc))
            {
                var descLabel = new Label(desc);
                descLabel.AddToClassList("Desc");
                textContainer.Add(descLabel);
            }

            var container = new HelpBox();
            container.AddToClassList("container");

            root.Add(mainBox);
            root.Add(container);
            main.Add(root);

            return container;
        }

        public static void AddSuffix(VisualElement target, string text)
        {
            Label suffix = new Label(text);
            suffix.AddToClassList("SuffixElement");
            if (!string.IsNullOrEmpty(text)) target.Add(suffix);
        }

        public static VisualElement MinMaxFloat(string name, string min, string max)
        {
            VisualElement field = new VisualElement();
            field.style.flexDirection = FlexDirection.Row;

            Label label = new Label(name);
            label.style.width = EditorGUIUtility.labelWidth + 2;
            field.Add(label);

            var minField = new FloatField() { bindingPath = min };
            minField.style.flexGrow = 1;
            var maxField = new FloatField() { bindingPath = max };
            maxField.style.flexGrow = 1;

            field.Add(minField);
            field.Add(maxField);

            if (minField.value > maxField.value) { minField.value = maxField.value; }
            else if (maxField.value < minField.value) maxField.value = minField.value;

            return field;
        }

        public static VisualElement ProgressBar(string name, float value, float max, InspectorColors color)
        {
            HelpBox background = new HelpBox();
            background.AddToClassList("ProgressBar");

            Label text = new Label($"{name} [{value}/{max}]");
            text.AddToClassList("ProgressLabel");

            HelpBox progress = new HelpBox();
            progress.AddToClassList("Fill");
            progress.AddToClassList($"Color{color.ToString()}");

            float percentage = Mathf.Clamp01(value / max);
            progress.style.width = Length.Percent(percentage * 100);

            background.Add(progress);
            background.Add(text);
            return background;
        }

        public static VisualElement FoldoutGroup(VisualElement root, string title)
        {
            return FoldoutGroup(root, title, "", null);
        }

        public static VisualElement FoldoutGroup(VisualElement root, string title, string desc)
        {
            return FoldoutGroup(root, title, desc, null);
        }

        public static VisualElement FoldoutGroup(VisualElement root, string title, Texture icon)
        {
            return FoldoutGroup(root, title, "", icon);
        }
        public static VisualElement FoldoutGroup(VisualElement main, string title, string desc, Texture icon)
        {
            bool toggle = false;

            var root = new VisualElement();
            root.AddToClassList("Group");
            var mainBox = new HelpBox();
            mainBox.AddToClassList("Header");

            var IconBox = new HelpBox();
            IconBox.AddToClassList("IconBox");

            if (icon != null)
            {
                var iconContent = icon;
                if (iconContent != null)
                {
                    var Icon = new Image { image = iconContent };
                    IconBox.Add(Icon);
                    Icon.AddToClassList("Icon");
                    mainBox.Add(IconBox);
                }
            }

            // Kontener na tekst (tytuł + opcjonalny opis)
            var textContainer = new VisualElement();
            textContainer.AddToClassList("textContainer");
            mainBox.Add(textContainer);

            // Tytuł
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("Title");
            textContainer.Add(titleLabel);

            // Opcjonalny opis
            if (!string.IsNullOrEmpty(desc))
            {
                var descLabel = new Label(desc);
                descLabel.AddToClassList("Desc");
                textContainer.Add(descLabel);
            }

            var container = new HelpBox();
            container.style.display = DisplayStyle.None;
            Image icon_btn = new Image();

            var editButton = new Button(() => ToggleGroup());
            editButton.AddToClassList("FoldoutButton");
            icon_btn.image = EditorGUIUtility.IconContent("IN foldout").image;          //editicon.sml
            icon_btn.style.rotate = new Rotate(new Angle(180));
            editButton.Add(icon_btn);
            editButton.style.paddingBottom = 0;
            editButton.style.paddingTop = 0;
            editButton.style.marginBottom = 0;
            editButton.style.marginTop = 0;
            editButton.style.borderRightWidth = 0;
            editButton.style.borderTopWidth = 0;
            editButton.style.borderBottomWidth = 0;
            mainBox.Add(editButton);

            container.AddToClassList("container");
            container.style.display = DisplayStyle.None;

            void ToggleGroup()
            {
                toggle = !toggle;

                icon_btn.image = toggle ? EditorGUIUtility.IconContent("IN foldout on").image : EditorGUIUtility.IconContent("IN foldout").image;
                icon_btn.style.rotate = toggle ? new Rotate(new Angle(0)) : new Rotate(new Angle(180));
                container.style.display = toggle ? DisplayStyle.Flex : DisplayStyle.None;
            }

            root.Add(mainBox);
            root.Add(container);
            main.Add(root);

            return container;
        }


        public static VisualElement CreateListView<T>(SerializedProperty property, VisualElement container, string title, bool isFoldable, bool showCounter, bool ShowAddRemoveButton = true)
        {
            return CreateListView<T>(property, container, title, "", null, isFoldable, showCounter, ShowAddRemoveButton);
        }
        public static VisualElement CreateListView<T>(SerializedProperty property, VisualElement container, string title, string desc, bool isFoldable, bool showCounter, bool ShowAddRemoveButton = true)
        {
            return CreateListView<T>(property, container, title, desc, null, isFoldable, showCounter, ShowAddRemoveButton);
        }
        public static VisualElement CreateListView<T>(SerializedProperty property, VisualElement container, string title, Texture icon, bool isFoldable, bool showCounter, bool ShowAddRemoveButton = true)
        {
            return CreateListView<T>(property, container, title, "", icon, isFoldable, showCounter, ShowAddRemoveButton);
        }
        public static VisualElement CreateListView<T>(SerializedProperty property, VisualElement container, string title, string desc, Texture icon, bool isFoldable, bool showCounter, bool ShowAddRemoveButton = true)
        {
            List<SerializedProperty> items = new List<SerializedProperty>();
            IntegerField countField = null;
            VisualElement listContainer;
            bool toggle = false;

            if (!isFoldable) toggle = true;

            var listView = new ListView
            {
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                headerTitle = title,
                showAddRemoveFooter = ShowAddRemoveButton,
                reorderMode = ListViewReorderMode.Animated,
                showBorder = true,
            };
            void RefreshList()
            {
                items = new List<SerializedProperty>();
                for (int i = 0; i < property.arraySize; i++)
                {
                    items.Add(property.GetArrayElementAtIndex(i));
                }
                listView.itemsSource = items;
                countField.value = items.Count;
                listView.Rebuild();
            }

            // Item Functionality
            listView.makeItem = () => new PropertyField();
            listView.bindItem = (element, index) =>
            {
                if (index < property.arraySize)
                {
                    var itemProperty = property.GetArrayElementAtIndex(index);
                    var field = element as PropertyField;
                    field.BindProperty(itemProperty);

                    // Odświeżaj listę tylko po zakończeniu edycji (utracie fokusu)
                    field.RegisterCallback<FocusOutEvent>(_ =>
                    {
                        EditorApplication.delayCall += () => RefreshList();
                    });
                }
            };

            // Update Functionality
            listView.itemsAdded += indices =>
            {
                foreach (var index in indices)
                {
                    property.InsertArrayElementAtIndex(index);
                }
                property.serializedObject.ApplyModifiedProperties();
            };
            listView.itemsRemoved += indices =>
            {
                foreach (var index in indices)
                {
                    property.DeleteArrayElementAtIndex(index);
                }
                property.serializedObject.ApplyModifiedProperties();
            };


            // Create GUI
            var root = new VisualElement();
            root.AddToClassList("Group");
            var mainBox = new HelpBox();
            mainBox.AddToClassList("Header");

            var IconBox = new HelpBox();
            IconBox.AddToClassList("IconBox");

            if (icon != null)
            {
                var iconContent = icon;
                if (iconContent != null)
                {
                    var Icon = new Image { image = iconContent };
                    IconBox.Add(Icon);
                    Icon.AddToClassList("Icon");
                    mainBox.Add(IconBox);
                }
            }

            // Kontener na tekst (tytuł + opcjonalny opis)
            var textContainer = new VisualElement();
            textContainer.AddToClassList("textContainer");
            mainBox.Add(textContainer);

            // Tytuł
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("Title");
            textContainer.Add(titleLabel);

            // Opcjonalny opis
            if (!string.IsNullOrEmpty(desc))
            {
                var descLabel = new Label(desc);
                descLabel.AddToClassList("Desc");
                textContainer.Add(descLabel);
            }

            root.Add(mainBox);
            container.Add(root);

            countField = new IntegerField() { value = items.Count };
            countField.SetEnabled(false);
            //countField.RegisterValueChangedCallback(evt => UpdateItemCount(evt.newValue));
            countField.AddToClassList("Counter");
            if (showCounter) mainBox.Add(countField);

            listContainer = new VisualElement();

            Image icon_btn = new Image();
            var editButton = new Button(() => ToggleGroup());
            editButton.AddToClassList("FoldoutButton");
            icon_btn.image = toggle ? EditorGUIUtility.IconContent("IN foldout on").image : EditorGUIUtility.IconContent("IN foldout").image;
            icon_btn.style.rotate = toggle ? new Rotate(new Angle(0)) : new Rotate(new Angle(180));
            editButton.Add(icon_btn);
            editButton.style.paddingBottom = 0;
            editButton.style.paddingTop = 0;
            editButton.style.marginBottom = 0;
            editButton.style.marginTop = 0;
            editButton.style.borderRightWidth = 0;
            editButton.style.borderTopWidth = 0;
            editButton.style.borderBottomWidth = 0;
            if (isFoldable) mainBox.Add(editButton);

            void ToggleGroup()
            {
                toggle = !toggle;

                icon_btn.image = toggle ? EditorGUIUtility.IconContent("IN foldout on").image : EditorGUIUtility.IconContent("IN foldout").image;
                icon_btn.style.rotate = toggle ? new Rotate(new Angle(0)) : new Rotate(new Angle(180));
                listContainer.style.display = toggle ? DisplayStyle.Flex : DisplayStyle.None;
            }
            if (!toggle) listContainer.style.display = DisplayStyle.None;
            listView.AddToClassList("ReorderableList");

            // Refresh GUI Content
            //property.serializedObject.Update();
            //listView.TrackSerializedObjectValue(property.serializedObject, _ => RefreshList());
            //listView.TrackPropertyValue(property, _ => RefreshList());
            RefreshList();

            listContainer.Add(listView);

            return listContainer;
        }

        public static VisualElement NodeGUID(string GUID, Vector2 pos)
        {
            VisualElement root = new VisualElement { style = { paddingLeft = 3 } };

            var guidRow = CreateRow(("GUID: ", GUID));
            AddContextMenu(root, GUID);

            root.Add(guidRow);
            root.Add(CreateRow(("X: ", pos.x.ToString()), ("Y: ", pos.y.ToString())));

            return root;


            // Help Classes
            VisualElement CreateRow(params (string label, string value)[] entries)
            {
                VisualElement container = new VisualElement { style = { flexDirection = FlexDirection.Row } };

                foreach (var (labelText, valueText) in entries)
                {
                    container.Add(CreateLabel(labelText, true));
                    container.Add(CreateLabel(valueText, false));
                }

                return container;
            }
            Label CreateLabel(string text, bool isBold)
            {
                return new Label(text)
                {
                    style =
        {
            fontSize = 10,
            unityFontStyleAndWeight = isBold ? FontStyle.Bold : FontStyle.Normal,
            minWidth = isBold ? 0 : 40
        }
                };
            }
            void AddContextMenu(VisualElement element, string GUID)
            {
                element.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    evt.menu.AppendAction("Copy GUID", _ => EditorGUIUtility.systemCopyBuffer = GUID);
                }));
            }
        }
        public static VisualElement ChoiceGUIDList(string Port, string Input, string Output)
        {
            VisualElement root = new VisualElement { style = { paddingLeft = 3 } };

            var PortGUID = CreateRow(("Port GUID:   ", Port));
            root.Add(PortGUID);

            root.Add(CreateRow(("Input GUID:  ", Input)));

            root.Add(CreateRow(("Output GUID: ", Output)));

            AddContextMenu(root, Port, Input, Output);

            return root;


            // Help Classes
            VisualElement CreateRow(params (string label, string value)[] entries)
            {
                VisualElement container = new VisualElement { style = { flexDirection = FlexDirection.Row } };

                foreach (var (labelText, valueText) in entries)
                {
                    container.Add(CreateLabel(labelText, true));
                    container.Add(CreateLabel(valueText, false));
                }

                return container;
            }
            Label CreateLabel(string text, bool isBold)
            {
                return new Label(text)
                {
                    style =
        {
            fontSize = 10,
            unityFontStyleAndWeight = isBold ? FontStyle.Bold : FontStyle.Normal,
            minWidth = isBold ? 0 : 40
        }
                };
            }
            void AddContextMenu(VisualElement element, string Port, string Input, string Output)
            {
                element.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    evt.menu.AppendAction("Copy Port GUID", _ => EditorGUIUtility.systemCopyBuffer = Port);
                    evt.menu.AppendAction("Copy Input GUID", _ => EditorGUIUtility.systemCopyBuffer = Input);
                    evt.menu.AppendAction("Copy Output GUID", _ => EditorGUIUtility.systemCopyBuffer = Output);
                }));
            }
        }
        public static VisualElement BranchGUIDList(string Port, string True, string False, Vector2 pos)
        {
            VisualElement root = new VisualElement { style = { paddingLeft = 3 } };

            var PortGUID = CreateRow(("Port GUID:   ", Port));
            root.Add(PortGUID);
            root.Add(CreateRow(("True GUID:  ", True)));
            root.Add(CreateRow(("False GUID: ", False)));
            root.Add(CreateRow(("X: ", pos.x.ToString()), ("Y: ", pos.y.ToString())));

            AddContextMenu(root, Port, True, False);

            return root;


            // Help Classes
            VisualElement CreateRow(params (string label, string value)[] entries)
            {
                VisualElement container = new VisualElement { style = { flexDirection = FlexDirection.Row } };

                foreach (var (labelText, valueText) in entries)
                {
                    container.Add(CreateLabel(labelText, true));
                    container.Add(CreateLabel(valueText, false));
                }

                return container;
            }
            Label CreateLabel(string text, bool isBold)
            {
                return new Label(text)
                {
                    style =
        {
            fontSize = 10,
            unityFontStyleAndWeight = isBold ? FontStyle.Bold : FontStyle.Normal,
            minWidth = isBold ? 0 : 40
        }
                };
            }
            void AddContextMenu(VisualElement element, string Port, string Input, string Output)
            {
                element.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    evt.menu.AppendAction("Copy Port GUID", _ => EditorGUIUtility.systemCopyBuffer = Port);
                    evt.menu.AppendAction("Copy True GUID", _ => EditorGUIUtility.systemCopyBuffer = Input);
                    evt.menu.AppendAction("Copy False GUID", _ => EditorGUIUtility.systemCopyBuffer = Output);
                }));
            }
        }

        public static VisualElement GlobalValueSelector(SerializedProperty property)
        {
            VisualElement root = new VisualElement();

            /* Load Values */
            List<string> tmp = new List<string>();
            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");

            if (manager != null)
            {
                manager.LoadFile();
                foreach (var value in manager.IntValues) tmp.Add(value.ValueName);
                foreach (var value in manager.FloatValues) tmp.Add(value.ValueName);
                foreach (var value in manager.BoolValues) tmp.Add(value.ValueName);
                foreach (var value in manager.StringValues) tmp.Add(value.ValueName);
            }

            /* Ensure property value exists in the list */
            if (string.IsNullOrEmpty(property.stringValue) || !tmp.Contains(property.stringValue))
            {
                tmp.Insert(0, "None"); // Placeholder for unassigned values
                property.stringValue = "None";
                property.serializedObject.ApplyModifiedProperties();
            }

            /* Create dropdown */
            PopupField<string> GlobalValueName = new PopupField<string>("Global Value", tmp, property.stringValue);
            GlobalValueName.tooltip = "Select a global value";

            /* Update property when selection changes */
            GlobalValueName.RegisterValueChangedCallback(evt =>
            {
                property.stringValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            root.Add(GlobalValueName);
            return root;
        }







        public enum InspectorColors
        {
            Defualt = 0,
            Red = 1,
            Orange = 2,
            Yellow = 3,
            Green = 4,
            LightBlue = 5,
            Blue = 6,
            Violet = 7,
            Pink = 8
        }

        public enum DialogueInspectorHintType
        {
            Normal,
            Warning,
            Error,
            Success
        }







        public static Texture GetTinyIcon(string IconName)
        {
            Texture tmp = Resources.Load($"TinyIcons/{IconName}") as Texture;

            if (tmp == null)
            {
                tmp = Resources.Load(IconName) as Texture;
                if (tmp == null)
                {
                    tmp = EditorGUIUtility.IconContent(IconName).image;
                }
            }

            return tmp;
        }




#endif
    }

}