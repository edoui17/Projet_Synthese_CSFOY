namespace IslandSurvivor.Scenes.NPC.Agressive;

using System.Collections.Generic;
using Godot;
using Core.Interfaces.Entities;
using Core.Interfaces.Stats;
using IslandSurvivor.Logic.Entities;

public abstract partial class MeleeEnemyBase : EnemyBase
{
    protected Area2D m_hitboxAreaRight;
    protected Area2D m_hitboxAreaLeft;
    protected HashSet<IDamageable> m_playersInHitbox = new();

    public override void _Ready()
    {
        base._Ready();

        m_hitboxAreaRight = GetNodeOrNull<Area2D>("HitboxAreaRight");
        if (m_hitboxAreaRight != null)
        {
            m_hitboxAreaRight.BodyEntered += OnHitboxAreaBodyEntered;
            m_hitboxAreaRight.BodyExited += OnHitboxAreaBodyExited;
        }

        m_hitboxAreaLeft = GetNodeOrNull<Area2D>("HitboxAreaLeft");
        if (m_hitboxAreaLeft != null)
        {
            m_hitboxAreaLeft.BodyEntered += OnHitboxAreaBodyEntered;
            m_hitboxAreaLeft.BodyExited += OnHitboxAreaBodyExited;
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_agressorController.CurrentState == NpcStates.DEAD) return;

        UpdateHitboxDirection();
        base._PhysicsProcess(p_delta);
    }

    protected virtual void UpdateHitboxDirection()
    {
        if (m_animatedSprite != null)
        {
            if (m_hitboxAreaRight != null) m_hitboxAreaRight.Monitoring = !m_animatedSprite.FlipH;
            if (m_hitboxAreaLeft != null) m_hitboxAreaLeft.Monitoring = m_animatedSprite.FlipH;
        }
    }

    protected override async void HandleAttackState()
    {
        if (m_playersInHitbox.Count > 0 && m_agressorController is IAgressorController controller && controller.CanAttack())
        {
            controller.StartAttack();

            // Wind-up delay
            await ToSignal(GetTree().CreateTimer(0.4f), SceneTreeTimer.SignalName.Timeout);
            if (m_agressorController.CurrentState == NpcStates.DEAD) return;

            int damageAmount = (int)(Stats?.BaseDamage ?? 5);
            var playersToDamage = new List<IDamageable>(m_playersInHitbox);
            foreach (var player in playersToDamage)
            {
                player.TakeDamage(damageAmount, this);
            }
        }
    }

    protected virtual void OnHitboxAreaBodyEntered(Node2D p_body)
    {
        if (CurrentState == NpcStates.DEAD) return;

        if (p_body.IsInGroup("Player") && p_body is IDamageable playerDamageable)
        {
            m_playersInHitbox.Add(playerDamageable);
        }
    }

    protected virtual void OnHitboxAreaBodyExited(Node2D p_body)
    {
        if (p_body.IsInGroup("Player") && p_body is IDamageable playerDamageable)
        {
            m_playersInHitbox.Remove(playerDamageable);
        }
    }
}
