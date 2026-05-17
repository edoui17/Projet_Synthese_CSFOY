namespace IslandSurvivor.Scenes.NPC.Agressive;

using System.Collections.Generic;
using Godot;
using IslandSurvivor.Logic.Entities;
using Core.Interfaces.Stats;
using IslandSurvivor.Logic.Entities;

public abstract partial class MeleeEnemyBase : EnemyBase
{
    protected Area2D m_hitboxAreaRight;
    protected Area2D m_hitboxAreaLeft;
    protected IslandSurvivor.Nodes.Combat.AttackController? m_attackController;

    public override void _Ready()
    {
        base._Ready();

        m_hitboxAreaRight = GetNodeOrNull<Area2D>("HitboxAreaRight");
        m_hitboxAreaLeft = GetNodeOrNull<Area2D>("HitboxAreaLeft");

        m_attackController = GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>("AttackController");
        if (m_attackController != null)
        {
            m_attackController.Stats = Stats;
            m_attackController.Faction = IslandSurvivor.Enums.EntityFaction.Enemy;
            m_attackController.AttackSprite = m_animatedSprite;
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

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_agressorController.CurrentState == NpcStates.DEAD) return;

        base._PhysicsProcess(p_delta);
    }

    protected override void HandleAttackState()
    {
        if (m_attackController != null && m_attackController.CanAttack && m_targetPlayer != null)
        {
            float distanceToPlayer = GlobalPosition.DistanceTo(m_targetPlayer.GlobalPosition);

            // Assume melee range is around 50 units
            if (distanceToPlayer <= 50f)
            {
                string direction = (m_animatedSprite != null && m_animatedSprite.FlipH) ? "Left" : "Right";
                m_attackController.TryAttack(direction);
            }
        }
    }

    protected virtual void OnAttackStarted()
    {
        // Custom logic for when attack starts, such as playing animation or sound.
        if (m_animatedSprite != null)
        {
            m_animatedSprite.Play("Attack");
            m_animatedSprite.Frame = 0;
        }
    }
}
