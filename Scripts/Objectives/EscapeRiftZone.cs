using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EscapeRiftZone : MonoBehaviour
{
    [Header("Rift Activation Settings")]
    [SerializeField] private bool requireMinimumLoot = true;
    [SerializeField] private float channelTimeToEscape = 3.5f; // Seconds required inside rift
    [SerializeField] private Transform riftVFXAnchor;

    [Header("Audio & Feedback")]
    [SerializeField] private AudioSource riftHumAudio;

    public float CurrentChannelProgress { get; private set; } = 0f;
    public bool IsPlayerInside { get; private set; } = false;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsPlayerInside = true;
            if (riftHumAudio != null && !riftHumAudio.isPlaying)
            {
                riftHumAudio.Play();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsPlayerInside = false;
            CurrentChannelProgress = 0f; // Reset stabilization if stepped out
            if (riftHumAudio != null)
            {
                riftHumAudio.Stop();
            }
        }
    }

    private void Update()
    {
        if (!IsPlayerInside) return;

        if (requireMinimumLoot && !HeistManager.Instance.HasMetEscapeQuota())
        {
            // Loot quota not met - rift remains inert
            return;
        }

        CurrentChannelProgress += Time.deltaTime;

        if (CurrentChannelProgress >= channelTimeToEscape)
        {
            TriggerPortalEscape();
        }
    }

    private void TriggerPortalEscape()
    {
        IsPlayerInside = false;
        HeistManager.Instance.SetPhase(HeistPhase.Success);
        Debug.Log($"PORTAL STABILIZED: Escaped into the Ether with ${HeistManager.Instance.SecuredLootValue:N0}!");
    }
}
