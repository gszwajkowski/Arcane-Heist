using UnityEngine;

public class LootObject : MonoBehaviour, IInteractable
{
    [Header("Loot Configuration")]
    [SerializeField] private string itemName = "Cash Stack";
    [SerializeField] private int cashValue = 15000;
    [SerializeField] private bool destroyOnPickup = true;

    public string GetInteractionPrompt()
    {
        return $"Take {itemName} (${cashValue:N0}) [E]";
    }

    public void Interact(PlayerInteraction interactor)
    {
        HeistManager.Instance.SecureLoot(cashValue);

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }
}
