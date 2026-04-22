using Godot;
using System;
using Core.Domain;
using IslandSurvivor.Logic.Entities;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes.Movement;
using Core.Interfaces;
using IslandSurvivor.Nodes;
using Core.Managers.Stats;

public partial class Sheep : CharacterBody2D, INpc, IDamageable
{
    [Export] public StatManager Stats { get; set; }

    [Export] public string NpcType { get; set; } = "Passive";
    [Export] public float IdleSpeed { get; set; } = 30.0f;
    [Export] public float FleeSpeed { get; set; } = 120.0f;

    private NavigationAgent2D m_navigationAgent;
    private PassiveController m_passiveController;
    private MovementController m_movementController;
    private Sprite2D m_sprite;
    private bool m_wasKilledByPlayer = false;

    public string CurrentState => m_passiveController?.CurrentState ?? NpcStates.IDLE;

    public override void _Ready()
    {
        m_passiveController = new PassiveController();

        m_navigationAgent = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
        m_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        m_movementController = GetNodeOrNull<MovementController>("MovementController");

        if (m_navigationAgent == null)
        {
            GD.PrintErr("Sheep node requires a NavigationAgent2D child node.");
        }
        else
        {
            m_navigationAgent.PathDesiredDistance = 4.0f;
            m_navigationAgent.TargetDesiredDistance = 4.0f;
            // No need for VelocityComputed if we just use direction + MovementController,
            // but left here in case Godot navigation avoidance is enabled later.
        }

        if (Stats != null)
        {
            Stats.SetCurrentValue(StatType.Health, 3);
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
        else if (m_passiveController.CurrentState == NpcStates.IDLE)
        {
            // Set navigation target based on current direction to wander using navmesh
            if (m_navigationAgent != null)
            {
                // Give a short distance to walk in that direction
                Vector2 targetPos = GlobalPosition + (m_passiveController.CurrentDirection * 50f);
                m_navigationAgent.TargetPosition = targetPos;

                if (!m_navigationAgent.IsNavigationFinished())
                {
                    Vector2 nextPathPosition = m_navigationAgent.GetNextPathPosition();
                    direction = GlobalPosition.DirectionTo(nextPathPosition);
                }
                else
                {
                    direction = Vector2.Zero;
                    // Force a new direction pick sooner if we hit a wall
                    m_passiveController.ResetDirectionChangeTimer();
                }
            }
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
    }

    public void TakeDamage(int p_amount, object p_attacker)
    {
        if (m_passiveController.CurrentState == NpcStates.DEAD) return;

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
                Modulate = new Color(1, 0.5f, 0.5f);
                GetTree().CreateTimer(0.2f).Timeout += () =>
                {
                    if (IsInstanceValid(this)) Modulate = Colors.White;
                };
            }
        }

        if (isDead)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        m_passiveController.SetDead();

        if (m_wasKilledByPlayer)
        {
            int meatAmount = 1;
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

