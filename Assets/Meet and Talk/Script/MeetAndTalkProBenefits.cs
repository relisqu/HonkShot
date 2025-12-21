#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class MeetAndTalkProBenefits : EditorWindow
{
    [MenuItem("Meet and Talk/Benefits of Meet and Talk Pro")]
    public static void ShowWindow()
    {
        MeetAndTalkProBenefits wnd = GetWindow<MeetAndTalkProBenefits>(true, "Benefits of Meet and Talk Pro");
        wnd.position = new Rect(710, 165, 300, 835);
        wnd.minSize = new Vector2(300, 835);
        wnd.maxSize = new Vector2(300, 835);
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        root.style.paddingLeft = 10;
        root.style.paddingRight = 10;
        root.style.paddingTop = 10;
        root.style.paddingBottom = 10;

        Label title = new Label("Benefits of Meet and Talk Pro")
        {
            style = { unityTextAlign = TextAnchor.MiddleCenter, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold }
        };
        root.Add(title);

        VisualElement featureList = new VisualElement();
        featureList.style.flexDirection = FlexDirection.Column;
        root.Add(featureList);

        AddFeature(featureList, "Start Node", true, true);
        AddFeature(featureList, "Dialogue Node", true, true);
        AddFeature(featureList, "Choice Node", true, true);
        AddFeature(featureList, "End Node", true, true);
        AddFeature(featureList, "Skip Button", true, true);
        AddFeature(featureList, "Random Start", true, true);
        AddFeature(featureList, "Localization", true, true);
        AddFeature(featureList, "Audio in Dialogue", true, true);
        AddFeature(featureList, "Auto Save", true, true);
        AddFeature(featureList, "Global Value", true, true);
        AddFeature(featureList, "Dynamic UI Change", true, true);

        AddFeature(featureList, "Event Node", true, false);
        AddFeature(featureList, "Timer Choice Node", true, false);
        AddFeature(featureList, "Random Node", true, false);
        AddFeature(featureList, "Comment Node", true, false);
        AddFeature(featureList, "IF Node", true, false);
        AddFeature(featureList, "Type Writing Animation", true, false);
        AddFeature(featureList, "Custom Inspectors", true, false);
        AddFeature(featureList, "Start By ID", true, false);
        AddFeature(featureList, "Character Avatar", true, false);
        AddFeature(featureList, "Character Emotions", true, false);
        AddFeature(featureList, "Global Value in UI", true, false);
        AddFeature(featureList, "Global Value as Character Name", true, false);
        AddFeature(featureList, "Import / Export Text File", true, false);
        AddFeature(featureList, "One-Click Dialogue Translation", true, false);
        AddFeature(featureList, "Dialogue Save Option", true, false);

        Button proButton = new Button(() => Application.OpenURL("https://u3d.as/30sy"))
        {
            text = "Meet and Talk - Pro Version\nAsset Store"
        };
        proButton.style.height = 60;
        root.Add(proButton);
    }

    private void AddFeature(VisualElement parent, string featureName, bool pro, bool free)
    {
        VisualElement row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.justifyContent = Justify.SpaceBetween;
        row.style.marginBottom = 5;

        Label nameLabel = new Label(featureName);
        nameLabel.style.flexGrow = 1;

        Label proLabel = new Label(pro ? "✔" : "✖")
        {
            style = { unityTextAlign = TextAnchor.MiddleCenter, width = 30, color = pro ? Color.green : Color.red }
        };

        Label freeLabel = new Label(free ? "✔" : "✖")
        {
            style = { unityTextAlign = TextAnchor.MiddleCenter, width = 30, color = free ? Color.green : Color.red }
        };

        row.Add(nameLabel);
        row.Add(proLabel);
        row.Add(freeLabel);
        parent.Add(row);
    }
}
#endif
