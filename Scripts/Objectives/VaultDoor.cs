using UnityEngine;

public enum VaultState
{
    Locked,
    Drilling,
    DrillJammed,
    Unlocked,
    Open
}

public class VaultDoor : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private Transform doorHinge;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;

    [Header("Stealth Requirements")]
    [SerializeField] private bool requiresKeycard = true;
    public bool PlayerHasKeycard { get; set; } = false;

    [Header("Loud / Drill Settings")]
    [SerializeField] private float drillDuration = 60f;
    [SerializeField] private float jamCheckInterval = 5f;
    [SerializeField] [Range(0f, 1f)] private float jamChancePerCheck = 0.25f;

    public VaultState CurrentState { get; private set; } = VaultState.Locked;
    public float DrillTimeRemaining { get; private set; }

    private float nextJamCheckTime;
    private Quaternion targetRotation;
    private bool isSwingingOpen = false;

    private void Start()
    {
        DrillTimeRemaining = drillDuration;
        if (doorHinge != null)
        {
            targetRotation = doorHinge.localRotation;
        }

        // If the alarm trips while still locked, keycard access locks down automatically
        HeistManager.OnPhaseChanged += HandlePhaseChanged;
    }

    private void OnDestroy()
    {
        HeistManager.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void Update()
    {
        if (CurrentState == VaultState.Drilling)
        {
            UpdateDrillProgress();
        }

        if (isSwingingOpen && doorHinge != null)
        {
            doorHinge.localRotation = Quaternion.Slerp(doorHinge.localRotation, targetRotation, Time.deltaTime * openSpeed);
            if (Quaternion.Angle(doorHinge.localRotation, targetRotation) < 0.5f)
            {
                isSwingingOpen = false;
                CurrentState = VaultState.Open;
            }
        }
    }

    public string GetInteractionPrompt()
    {
        switch (CurrentState)
        {
            case VaultState.Locked:
                if (!HeistManager.Instance.IsAlarmTripped)
                {
                    return PlayerHasKeycard 
                        ? "Swipe Keycard to Unlock [E]" 
                        : "Vault Locked - Requires Keycard";
                }
                return "Mount Thermal Breach Drill [E]";

            case VaultState.Drilling:
                return $"Breaching in Progress ({Mathf.CeilToInt(DrillTimeRemaining)}s remaining)";

            case VaultState.DrillJammed:
                return "Drill Jammed! Press [E] to Restart";

            case VaultState.Unlocked:
                return "Open Vault Door [E]";

            case VaultState.Open:
            default:
                return string.Empty;
        }
    }

    public void Interact(PlayerInteraction interactor)
    {
        switch (CurrentState)
        {
            case VaultState.Locked:
                if (!HeistManager.Instance.IsAlarmTripped)
                {
                    if (PlayerHasKeycard || !requiresKeycard)
                    {
                        UnlockDoorSilently();
                    }
                }
                else
                {
                    StartDrilling();
                }
                break;

            case VaultState.DrillJammed:
                RestartDrill();
                break;

            case VaultState.Unlocked:
                OpenDoor();
                break;
        }
    }

    private void UnlockDoorSilently()
    {
        CurrentState = VaultState.Unlocked;
        OpenDoor();
    }

    private void StartDrilling()
    {
        CurrentState = VaultState.Drilling;
        nextJamCheckTime = Time.time + jamCheckInterval;
        HeistManager.Instance.TripAlarm();
    }

    private void RestartDrill()
    {
        CurrentState = VaultState.Drilling;
        nextJamCheckTime = Time.time + jamCheckInterval;
    }

    private void UpdateDrillProgress()
    {
        DrillTimeRemaining -= Time.deltaTime;

        if (Time.time >= nextJamCheckTime)
        {
            nextJamCheckTime = Time.time + jamCheckInterval;
            if (Random.value < jamChancePerCheck)
            {
                CurrentState = VaultState.DrillJammed;
                return;
            }
        }

        if (DrillTimeRemaining <= 0f)
        {
            CurrentState = VaultState.Unlocked;
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        if (doorHinge != null)
        {
            targetRotation = doorHinge.localRotation * Quaternion.Euler(0f, openAngle, 0f);
            isSwingingOpen = true;
        }
        else
        {
            CurrentState = VaultState.Open;
        }
    }

    private void HandlePhaseChanged(HeistPhase phase)
    {
        if (phase == HeistPhase.Loud && CurrentState == VaultState.Locked)
        {
            // Electronic keycard access locks down under active alarm
            requiresKeycard = false;
        }
    }
}
