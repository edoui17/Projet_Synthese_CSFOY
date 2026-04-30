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
    [Export] private IslandSurvivor.Nodes.StatManager _stats;


    [Export] public string NpcType { get; set; } = "Passive";
    [Export] public float IdleSpeed { get; set; } = 30.0f;
    [Export] public float FleeSpeed { get; set; } = 120.0f;

    private PassiveController m_passiveController;
    [Export] private MovementController m_movementController;
    [Export] private Sprite2D m_sprite;
    private bool m_wasKilledByPlayer = false;

    public string CurrentState => m_passiveController?.CurrentState ?? NpcStates.IDLE;

    private DamageContext? m_lastContext;


    public float GetHealth()
    {
        return _stats?.GetCurrentValue(Core.Managers.Stats.StatType.Health) ?? 0f;
    }

    public void Heal(float p_amount)
    {
        _stats?.ModifyCurrentValue(Core.Managers.Stats.StatType.Health, p_amount);
    }

    public override void _Ready()
    {
        m_passiveController = new PassiveController();

        if (_stats != null)
        {
            _stats.SetCurrentValue(StatType.Health, 3);
            _stats.Connect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
        }
    }

    private void OnStatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)
    {
        if ((StatType)p_statType == StatType.Health && p_currentValue <= 0)
        {
            if (_stats != null)
            {
                _stats.Disconnect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
            }
            HandleDeath(m_lastContext);
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

        // Flip sprite based on movement direction
        if (m_sprite != null && direction.X != 0)
        {
            m_sprite.FlipH = direction.X < 0;
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

    public void TakeDamage(int p_amount, DamageContext p_context)
    {
        if (m_passiveController.CurrentState == NpcStates.DEAD) return;

        m_lastContext = p_context;

        if (_stats != null)
        {
            // Just apply damage logically here since we don't have a direct _stats.TakeDamage method visible
            float currentHp = _stats.GetCurrentValue(StatType.Health);
            _stats.SetCurrentValue(StatType.Health, currentHp - p_amount);
        }

        bool isDead = _stats == null || _stats.GetCurrentValue(StatType.Health) <= 0;

        if (p_context?.Attacker is Node2D attackerNode)
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

    private void HandleDeath(DamageContext p_context = null)
    {
        m_passiveController.SetDead();

        if (m_wasKilledByPlayer)
        {
            int meatAmount = 1;

            float luck = p_context?.AttackerLuck ?? 0f;

            float bonusChance = luck * 0.05f;
            int bonusQuantity = (int)bonusChance;
            float fractionalChance = bonusChance - bonusQuantity;

            Random random = new();
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

