using UnityEngine;

public enum SpellType
{
    Offensive,
    CrowdControl,
    Utility,
    Mobility
}

[CreateAssetMenu(fileName = "NewSpellData", menuName = "Heist/Spell Data")]
public class SpellData : ScriptableObject
{
    [Header("Display")]
    public string spellName = "Arcane Bolt";
    public SpellType spellType = SpellType.Offensive;

    [Header("Resource & Timing")]
    public float manaCost = 20f;
    public float cooldown = 0.5f;

    [Header("Impact Properties")]
    public float range = 40f;
    public float primaryValue = 30f; // Damage or duration
    public float effectRadius = 0f;  // Set > 0 for area effects
}
