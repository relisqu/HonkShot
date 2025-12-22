using UnityEngine;
using UnityEngine.SceneManagement;

public class GateToGameplay : MonoBehaviour, IInteractable
{
    public void OnInteracted()
    {
        Debug.Log("Interacted");
        SceneManager.LoadScene("GameplayScene", LoadSceneMode.Single);
    }
}
