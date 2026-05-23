namespace IslandSurvivor.Scenes.NPC.Aggressive;

using Godot;
using IslandSurvivor.Globals;

public partial class MeleeAggressiveNpcBase : AggressiveNpcBase
{
    [ExportGroup("Animations")]
    [Export] public string AttackAnimationName { get; set; } = "Attack";

    protected Area2D m_hitboxAreaRight;
    protected Area2D m_hitboxAreaLeft;

    public override void _Ready()
    {
        base._Ready();

        m_hitboxAreaRight = GetNodeOrNull<Area2D>("HitboxAreaRight");
        m_hitboxAreaLeft = GetNodeOrNull<Area2D>("HitboxAreaLeft");

        if (m_attackController != null)
        {
            m_attackController.Stats = Stats;
            m_attackController.Faction = IslandSurvivor.Enums.EntityFaction.Enemy;
            m_attackController.ActionFrame = 2; // Impact frame for Melee

            if (m_hitboxAreaRight != null) m_attackController.RegisterArea("Right", m_hitboxAreaRight);
            if (m_hitboxAreaLeft != null) m_attackController.RegisterArea("Left", m_hitboxAreaLeft);

            m_attackController.AttackStarted += OnAttackStarted;
        }
        else
        {
            GD.PushWarning($"{Name}: AttackController not found.");
        }
    }

    protected virtual void OnAttackStarted()
    {
    }
}