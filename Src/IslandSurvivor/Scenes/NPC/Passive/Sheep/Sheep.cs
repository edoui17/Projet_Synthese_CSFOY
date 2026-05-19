using Godot;
using System;
using Core.Domain;
using IslandSurvivor.Logic.Entities;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes.Movement;
using Core.Interfaces;
using Core.Interfaces.Stats;
using IslandSurvivor.Nodes;
using Core.Managers.Stats;

public partial class Sheep : CharacterBody2D, INpc, IDamageable
{
    [Export] public StatManager Stats { get; set; } = null!;

    [Export] public string NpcType { get; set; } = "Passive";
    [Export] public float IdleSpeed { get; set; } = 30.0f;
    [Export] public float FleeSpeed { get; set; } = 120.0f;

    private PassiveController m_passiveController = null!;
    [Export] private MovementController m_movementController = null!;
    [Export] private AnimatedSprite2D m_sprite = null!;
    private bool m_wasKilledByPlayer = false;

    public string CurrentState => m_passiveController?.CurrentState ?? NpcStates.IDLE;

    private object? m_lastAttacker;

    public override void _Ready()
    {
        m_passiveController = new PassiveController();

        if (Stats != null)
        {
            Stats.Connect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
        }
    }

    private void OnStatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)
    {
        if ((StatType)p_statType == StatType.Health && p_currentValue <= 0)
        {
            if (Stats != null)
            {
                Stats.Disconnect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
            }
            HandleDeath(m_lastAttacker);
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_passiveController.CurrentState == NpcStates.DEAD) return;

        m_passiveController.Update((float)p_delta);

        Vector2 direction = m_passiveController.CurrentDirection;
        float targetSpeed = IdleSpeed;

        if (m_passiveController.CurrentState == NpcStates.FLEE)
        {
            targetSpeed = FleeSpeed;
        }

        // Flip sprite based on movement direction and update animation
        if (m_sprite != null)
        {
            if (direction.X != 0)
            {
                m_sprite.FlipH = direction.X < 0;
            }

            if (m_passiveController.CurrentState == NpcStates.FLEE)
            {
                if (m_sprite.Animation != "FLEE") m_sprite.Play("FLEE");
            }
            else
            {
                if (Velocity.LengthSquared() > 0 || direction.LengthSquared() > 0)
                {
                    // Sheep doesn't have a distinct moving animation right now, using IDLE or FLEE.
                    if (m_sprite.Animation != "IDLE") m_sprite.Play("IDLE");
                }
                else
                {
                    if (m_sprite.Animation != "IDLE") m_sprite.Play("IDLE");
                }
            }
        }

        if (m_movementController != null)
        {
            m_movementController.Move(direction, targetSpeed);
        }
        else
        {
            Velocity = direction * targetSpeed;
            MoveAndSlide();
        }

        // Obstacle avoidance in IDLE state
        if (m_passiveController.CurrentState == NpcStates.IDLE && GetSlideCollisionCount() > 0)
        {
            m_passiveController.ForceNewDirection();
        }
    }

    public void TakeDamage(int p_amount, object p_attacker)
    {
        if (m_passiveController.CurrentState == NpcStates.DEAD) return;

        m_lastAttacker = p_attacker;

        if (Stats != null)
        {
            // Just apply damage logically here since we don't have a direct Stats.TakeDamage method visible
            float currentHp = Stats.GetCurrentValue(StatType.Health);
            Stats.SetCurrentValue(StatType.Health, currentHp - p_amount);
        }

        bool isDead = Stats == null || Stats.GetCurrentValue(StatType.Health) <= 0;

        if (p_attacker is Node2D attackerNode)
        {
            if (isDead && attackerNode.IsInGroup("Player"))
            {
                m_wasKilledByPlayer = true;
            }

            if (!isDead)
            {
                // Let the controller compute flee direction
                m_passiveController.StartFleeing(GlobalPosition, attackerNode.GlobalPosition);

                // Visual feedback
                IslandSurvivor.Extensions.NodeExtensions.PlayHitFlash(this);
            }
        }
    }

    private void HandleDeath(object? p_attacker = null)
    {
        m_passiveController.SetDead();

        if (IslandSurvivor.Globals.ServiceRegistry.Instance != null)
        {
            IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus.Publish(new Core.Events.EnemyKilledEvent(Name, NpcType, 0f));
        }

        if (m_wasKilledByPlayer)
        {
            Random random = new();
            int meatAmount = random.Next(1, 4); // 1 to 3 meat

            float luck = 0f;
            if (p_attacker is Node GodotAttacker)
            {
                StatManager attackerStats = GodotAttacker.GetNodeOrNull<StatManager>("StatManager");
                if (attackerStats != null)
                {
                    luck = attackerStats.GetCurrentValue(StatType.Luck);
                }
            }

            float bonusChance = luck * 0.05f;
            int bonusQuantity = (int)bonusChance;
            float fractionalChance = bonusChance - bonusQuantity;

            if (random.NextDouble() < fractionalChance)
            {
                bonusQuantity++;
            }

            meatAmount += bonusQuantity;

            ResourceItem meatResource = new ResourceItem("meat_01", "Viande", "Meat", "res://Assets/TinySwords/TinySwords(Update010)/Deco/17.png");

            if (SignalManager.Instance != null)
            {
                SignalManager.Instance.EmitMaterialDestroyed(this, meatResource, meatAmount);
                GD.Print($"Sheep died. Sent {meatAmount} meat to inventory.");
            }
            else
            {
                GD.PrintErr("SignalManager is not available.");
            }
        }

        QueueFree();
    }
}

