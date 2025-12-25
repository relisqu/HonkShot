
using TMPro;
using UnityEngine;

public class DialogueNodeWindow : MonoBehaviour
{
    [Header("Core UI Elements")]
    [SerializeField] private RectTransform rootRect;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Dialogue Content")]
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    //Dialogue portrait - TODO?


    [Header("Choices Layout")]
    [SerializeField] private RectTransform choicesParent;
    [SerializeField] private GameObject choiceButtonPrefab;
    

    
}
