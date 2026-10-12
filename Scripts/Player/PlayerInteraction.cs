using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float interactionRange = 3.0f;
    [SerializeField] private LayerMask interactionLayer;

    public string CurrentPrompt { get; private set; } = string.Empty;

    private Camera playerCamera;

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Submit"))
        {
            PerformInteraction();
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayer))
        {
            IInteractable target = hit.collider.GetComponent<IInteractable>();
            if (target != null)
            {
                CurrentPrompt = target.GetInteractionPrompt();
                return;
            }
        }
        CurrentPrompt = string.Empty;
    }

    private void PerformInteraction()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayer))
        {
            IInteractable target = hit.collider.GetComponent<IInteractable>();
            target?.Interact(this);
        }
    }
}
