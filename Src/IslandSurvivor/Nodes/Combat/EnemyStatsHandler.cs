using Godot;
using IslandSurvivor.Nodes;
using Core.Managers;

namespace IslandSurvivor.Nodes;

/// <summary>
/// [Gameplay][Statistique][Spawning]
/// Decorator component that applies dynamic scaling and Elite multipliers
/// over the base generic NpcBase StatManager.
/// </summary>
public partial class EnemyStatsHandler : Node
{
    [ExportGroup("Base References")]
    [Export] public StatManager Stats { get; set; } = null!;

    [ExportGroup("Elite Multipliers")]
    [Export] public float EliteHealthMultiplier { get; set; } = 2.0f;
    [Export] public float EliteDamageMultiplier { get; set; } = 1.5f;
    [Export] public float EliteScaleMultiplier { get; set; } = 1.25f;
    [Export] public Color EliteGlowColor { get; set; } = new Color(0.8f, 1.2f, 1.5f, 1f); // Blue/White Glow

    private bool m_isElite = false;
    public bool IsElite => m_isElite;

    public void InitializeStats(float p_threatScore, bool p_isElite)
    {
        if (Stats == null)
        {
            GD.PushWarning($"[EnemyStatsHandler] No StatManager assigned on {GetParent().Name}. Scaling aborted.");
            return;
        }

        m_isElite = p_isElite;

        float currentMaxHealth = Stats.MaxHealth;
        float currentBaseAttack = Stats.BaseAttackValue;

        float newMaxHealth = currentMaxHealth * p_threatScore;
        float newBaseAttack = currentBaseAttack * p_threatScore;

        float newIdleSpeed = 0f;
        float newChaseSpeed = 0f;

        if (GetParent() is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggressiveNpc)
        {
            newIdleSpeed = aggressiveNpc.IdleSpeed * p_threatScore;
            newChaseSpeed = aggressiveNpc.ChaseSpeed * p_threatScore;
        }

        if (m_isElite)
        {
            newMaxHealth *= EliteHealthMultiplier;
            newBaseAttack *= EliteDamageMultiplier;

            // Visual feedback for elites
            if (GetParent() is Node2D parent2D)
            {
                var sprite = parent2D.GetNodeOrNull<Sprite2D>("Sprite2D");
                if (sprite != null)
                {
                    sprite.Scale *= EliteScaleMultiplier; // Slightly larger
                    sprite.Modulate = EliteGlowColor; // Blue/White glow
                }
            }
        }

        Stats.MaxHealth = newMaxHealth;
        Stats.BaseAttackValue = newBaseAttack;

        // Ensure current health is topped up to the new max
        Stats.SetCurrentValue(StatType.Health, newMaxHealth);

        if (GetParent() is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggressiveNpcFinal)
        {
            aggressiveNpcFinal.IdleSpeed = newIdleSpeed;
            aggressiveNpcFinal.ChaseSpeed = newChaseSpeed;
        }

        GD.Print($"[EnemyStatsHandler] {GetParent().Name} initialized. Threat: {p_threatScore:F2} | Elite: {p_isElite} | HP: {newMaxHealth:F1} | ATK: {newBaseAttack:F1}");
    }
}
