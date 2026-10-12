using System;
using UnityEngine;

public enum HeistPhase
{
    Stealth,
    Loud,
    Escaping,
    Failed,
    Success
}

public class HeistManager : MonoBehaviour
{
    public static HeistManager Instance { get; private set; }

    [Header("Heist Settings")]
    [SerializeField] private int targetLootRequired = 100000;
    
    public HeistPhase CurrentPhase { get; private set; } = HeistPhase.Stealth;
    public int SecuredLootValue { get; private set; } = 0;
    public bool IsAlarmTripped => CurrentPhase != HeistPhase.Stealth;

    public static event Action<HeistPhase> OnPhaseChanged;
    public static event Action<int> OnLootSecured;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SecureLoot(int amount)
    {
        SecuredLootValue += amount;
        OnLootSecured?.Invoke(SecuredLootValue);
    }

    public void TripAlarm()
    {
        if (CurrentPhase == HeistPhase.Stealth)
        {
            SetPhase(HeistPhase.Loud);
        }
    }

    public void SetPhase(HeistPhase newPhase)
    {
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
    }

    public bool HasMetEscapeQuota()
    {
        return SecuredLootValue >= targetLootRequired;
    }
}
