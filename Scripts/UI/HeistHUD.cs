using UnityEngine;
using UnityEngine.UI;

public class HeistHUD : MonoBehaviour
{
    [Header("UI Text References")]
    [SerializeField] private Text lootCounterText;
    [SerializeField] private Text alarmStatusText;
    [SerializeField] private Text interactionPromptText;
    [SerializeField] private Text objectiveStatusText;

    [Header("Player & Objectives")]
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private VaultDoor vaultDoor;

    private void Start()
    {
        HeistManager.OnLootSecured += UpdateLootDisplay;
        HeistManager.OnPhaseChanged += UpdatePhaseDisplay;

        UpdateLootDisplay(HeistManager.Instance.SecuredLootValue);
        UpdatePhaseDisplay(HeistManager.Instance.CurrentPhase);
    }

    private void OnDestroy()
    {
        HeistManager.OnLootSecured -= UpdateLootDisplay;
        HeistManager.OnPhaseChanged -= UpdatePhaseDisplay;
    }

    private void Update()
    {
        // Update interaction prompt in real-time
        if (interactionPromptText != null && playerInteraction != null)
        {
            interactionPromptText.text = playerInteraction.CurrentPrompt;
        }

        // Update objective timer if vault is actively drilling
        if (objectiveStatusText != null && vaultDoor != null)
        {
            if (vaultDoor.CurrentState == VaultState.Drilling)
            {
                objectiveStatusText.text = $"Breaching Vault: {Mathf.CeilToInt(vaultDoor.DrillTimeRemaining)}s";
            }
            else if (vaultDoor.CurrentState == VaultState.DrillJammed)
            {
                objectiveStatusText.text = "DRILL JAMMED - RESTART REQUIRED";
            }
            else if (vaultDoor.CurrentState == VaultState.Open)
            {
                objectiveStatusText.text = "Bag the cash and reach the escape van!";
            }
        }
    }

    private void UpdateLootDisplay(int totalLoot)
    {
        if (lootCounterText != null)
        {
            lootCounterText.text = $"Loot: ${totalLoot:N0}";
        }
    }

    private void UpdatePhaseDisplay(HeistPhase phase)
    {
        if (alarmStatusText != null)
        {
            alarmStatusText.text = phase == HeistPhase.Stealth ? "STATUS: STEALTH" : "STATUS: ALARM / LOUD";
            alarmStatusText.color = phase == HeistPhase.Stealth ? Color.cyan : Color.red;
        }
    }
}
