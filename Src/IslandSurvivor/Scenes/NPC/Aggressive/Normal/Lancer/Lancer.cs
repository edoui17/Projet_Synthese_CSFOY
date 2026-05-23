namespace IslandSurvivor.Scenes.NPC.Aggressive;

using Godot;
using IslandSurvivor.Logic.Entities;
using Core.Managers.Stats;
using IslandSurvivor.Globals;

public partial class Lancer : MeleeAggressiveNpcBase
{
    [Export] public float DashSpeedMultiplier { get; set; } = 5f;
    [Export] public float MinDashDistance { get; set; } = 200f;

    private Area2D? m_hitboxAreaUp;
    private Area2D? m_hitboxAreaDown;

    public override void _Ready()
    {
        AttackSoundKey = "Lancer_Attack";
        Stats = GetNodeOrNull<IslandSurvivor.Nodes.StatManager>("StatManager");
        if (Stats == null)
        {
            GD.PrintErr("Lancer node requires a StatManager child node.");
        }

        base._Ready();

        m_hitboxAreaUp = GetNodeOrNull<Area2D>("HitboxAreaUp");
        m_hitboxAreaDown = GetNodeOrNull<Area2D>("HitboxAreaDown");

        if (m_attackController != null)
        {
            m_attackController.ActionFrame = 3;
            if (m_hitboxAreaUp != null) m_attackController.RegisterArea("Up", m_hitboxAreaUp);
            if (m_hitboxAreaDown != null) m_attackController.RegisterArea("Down", m_hitboxAreaDown);
        }

        // Ensure dash hitboxes are disabled initially
        if (m_hitboxAreaUp != null) m_hitboxAreaUp.Monitoring = false;
        if (m_hitboxAreaDown != null) m_hitboxAreaDown.Monitoring = false;
        if (m_hitboxAreaRight != null) m_hitboxAreaRight.Monitoring = false;
        if (m_hitboxAreaLeft != null) m_hitboxAreaLeft.Monitoring = false;

        if (m_hitboxAreaRight != null) m_hitboxAreaRight.BodyEntered += OnDashHitboxEntered;
        if (m_hitboxAreaLeft != null) m_hitboxAreaLeft.BodyEntered += OnDashHitboxEntered;
        if (m_hitboxAreaUp != null) m_hitboxAreaUp.BodyEntered += OnDashHitboxEntered;
        if (m_hitboxAreaDown != null) m_hitboxAreaDown.BodyEntered += OnDashHitboxEntered;
    }

    public void EnableDashHitbox(Vector2 direction)
    {
        if (Mathf.Abs(direction.Y) > Mathf.Abs(direction.X))
        {
            if (direction.Y < 0 && m_hitboxAreaUp != null) m_hitboxAreaUp.SetDeferred(Area2D.PropertyName.Monitoring, true);
            else if (direction.Y > 0 && m_hitboxAreaDown != null) m_hitboxAreaDown.SetDeferred(Area2D.PropertyName.Monitoring, true);
        }
        else
        {
            if (direction.X < 0 && m_hitboxAreaLeft != null) m_hitboxAreaLeft.SetDeferred(Area2D.PropertyName.Monitoring, true);
            else if (direction.X >= 0 && m_hitboxAreaRight != null) m_hitboxAreaRight.SetDeferred(Area2D.PropertyName.Monitoring, true);
        }
    }

    public void DisableAllDashHitboxes()
    {
        if (m_hitboxAreaUp != null) m_hitboxAreaUp.SetDeferred(Area2D.PropertyName.Monitoring, false);
        if (m_hitboxAreaDown != null) m_hitboxAreaDown.SetDeferred(Area2D.PropertyName.Monitoring, false);
        if (m_hitboxAreaLeft != null) m_hitboxAreaLeft.SetDeferred(Area2D.PropertyName.Monitoring, false);
        if (m_hitboxAreaRight != null) m_hitboxAreaRight.SetDeferred(Area2D.PropertyName.Monitoring, false);
    }

    private void OnDashHitboxEntered(Node2D p_body)
    {
        if (p_body.IsInGroup("Player") || p_body.Name == "Player")
        {
            if (p_body is Core.Interfaces.Stats.IDamageable damageable)
            {
                float baseDamage = Stats?.BaseAttackValue ?? 10f;
                float attackStat = Stats?.GetCurrentValue(Core.Managers.Stats.StatType.Attack) ?? 0f;
                int finalDamage = IslandSurvivor.Logic.CombatMath.CalculateDamage(baseDamage, attackStat);

                // Dash deals 150% damage
                finalDamage = (int)(finalDamage * 1.5f);

                damageable.TakeDamage(finalDamage, this);
            }

            // Interrupt the dash upon hitting player
            m_stateMachine?.ForceTransition("RecoveryState");
        }
    }
}
