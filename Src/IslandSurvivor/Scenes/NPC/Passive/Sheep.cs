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
    private SheepController m_sheepController;
    private MovementController m_movementController;
    private Sprite2D m_sprite;
    private bool m_wasKilledByPlayer = false;

    public string CurrentState => m_sheepController?.CurrentState ?? SheepStates.IDLE;

    public override void _Ready()
    {
        m_sheepController = new SheepController();

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
            m_navigationAgent.VelocityComputed += OnVelocityComputed;
        }

        if (Stats != null)
        {
            Stats.SetCurrentValue(StatType.Health, 3);
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_sheepController.CurrentState == SheepStates.DEAD) return;

        m_sheepController.Update((float)p_delta);

        Vector2 direction = m_sheepController.CurrentDirection;

        float targetSpeed = IdleSpeed;
        if (m_sheepController.CurrentState == SheepStates.FLEE)
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
    }

    public void TakeDamage(int p_amount, object p_attacker)
    {
        if (m_sheepController.CurrentState == SheepStates.DEAD) return;

        if (Stats != null)
        {
            Vector2 myPos = GlobalPosition;
            Vector2 attackerPos = attackerNode.GlobalPosition;

        bool isDead = Stats == null || Stats.GetCurrentValue(StatType.Health) <= 0;

        if (p_attacker is Node2D attackerNode)
        {
            if (isDead && attackerNode.IsInGroup("Player"))
            {
                m_wasKilledByPlayer = true;
            }

            if (!isDead)
            {
                System.Numerics.Vector2 myPos = new System.Numerics.Vector2(GlobalPosition.X, GlobalPosition.Y);
                System.Numerics.Vector2 attackerPos = new System.Numerics.Vector2(attackerNode.GlobalPosition.X, attackerNode.GlobalPosition.Y);

                m_sheepController.StartFleeing(myPos, attackerPos);

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
        m_sheepController.SetDead();

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
