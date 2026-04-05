using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedTMPText : MonoBehaviour
{
    [SerializeField] private string key;
    [SerializeField] private TMP_Text targetText;

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (targetText == null)
            return;

        if (LocalizationManager.Instance == null)
        {
            Debug.LogWarning($"LocalizationManager.Instance is null on {name}");
            return;
        }

        targetText.text = LocalizationManager.Instance.Get(key);
    }
}