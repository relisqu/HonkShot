
using UnityEngine;
using UnityEngine.InputSystem;

public class HubPlayerController : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;
    
    [SerializeField]
    HubPlayerPawn playerPawn;


    void Awake()
    {
        if (playerPawn == null) 
            playerPawn = GetComponent<HubPlayerPawn>();
    }

    public void OnMouseClick(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("Click occured");

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100f, Color.green, 2f);

            int layerMask = 1 << LayerMask.NameToLayer("Default");
            RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, 100f, layerMask);
            
            if (hits.Length == 0)
            {
                Debug.Log("Crit miss lol");
                return;
            }
            
            Debug.Log($"Hit {hits.Length} colliders");
            
            IInteractable hitInteractible = null;
            Vector2 destination = hits[0].point;
            
            foreach (RaycastHit2D hit in hits)
            {
                Debug.Log($"Hit: {hit.collider.gameObject.name}");
                IInteractable interactable = hit.collider.gameObject.GetComponent<IInteractable>();
                if (interactable != null) 
                {
                    Debug.Log($"Found interactable: {hit.collider.gameObject.name}");
                    hitInteractible = interactable;
                    destination = hit.point;
                    break;
                }
            }
            

            // decide what we doin exactly
            if (hitInteractible != null)
            {
                playerPawn.MoveToInteractable(destination, hitInteractible);
            }
            else if (destination != null) //shitty fallback but still
            {
                playerPawn.MoveToPosition(destination);
            }
        }
    }
}
