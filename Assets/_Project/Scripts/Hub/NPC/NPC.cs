using UnityEngine;
using UnityEngine.Events;

public class NPC : MonoBehaviour, IInteractable
{
    public UnityEvent interactionEvent;

    public void OnInteracted()
    {
        Debug.Log("magic brewin here; as of now no systems for talking/level start are talked about; event as placeholder here");
        interactionEvent?.Invoke();
    }
}
