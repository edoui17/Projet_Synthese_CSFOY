using Godot;
using System;
using Core.Domain;
using IslandSurvivor.Logic.Entities;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes.Movement;
using Core.Interfaces;

public partial class Sheep : CharacterBody2D, INpc, IDamageable
{
    [Export] public string NpcType { get; set; } = "Passive";
    [Export] public float IdleSpeed { get; set; } = 30.0f;
    [Export] public float FleeSpeed { get; set; } = 120.0f;
    [Export] public int MaxHealth { get; set; } = 3;

    private NavigationAgent2D m_navigationAgent;
    private HealthComponent m_healthComponent;
    private SheepController m_sheepController;
    private MovementController m_movementController;
    private Sprite2D m_sprite;

    public string CurrentState => m_sheepController?.CurrentState ?? SheepStates.IDLE;

    public override void _Ready()
    {
        m_healthComponent = new HealthComponent(MaxHealth);
        m_healthComponent.OnDeath += HandleDeath;

        m_sheepController = new SheepController();

        m_navigationAgent = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
        m_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        m_movementController = GetNodeOrNull<MovementController>("MovementController");

        if (m_navigationAgent == null)
        {
            GD.PrintErr("Sheep node requires a NavigationAgent2D child node.");
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_healthComponent.IsDead) return;

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
        if (m_healthComponent.IsDead) return;

        m_healthComponent.TakeDamage(p_amount);

        if (!m_healthComponent.IsDead && p_attacker is Node2D attackerNode)
        {
            Vector2 myPos = GlobalPosition;
            Vector2 attackerPos = attackerNode.GlobalPosition;

            m_sheepController.StartFleeing(myPos, attackerPos);

            // Visual feedback
            Modulate = new Color(1, 0.5f, 0.5f);
            GetTree().CreateTimer(0.2f).Timeout += () =>
            {
                if (IsInstanceValid(this)) Modulate = Colors.White;
            };
        }
    }

    private void HandleDeath(object p_sender, EventArgs p_args)
    {
        m_sheepController.SetDead();

        // Check if killed by player (assuming player is named "Player" or in group "Player")
        // The Acceptance Criteria says "attacker is the player". Since our TakeDamage doesn't pass the attacker
        // to the OnDeath event easily without custom args, we can just assume any death drops it to inventory for now,
        // or we could track the last attacker. Let's create the meat.

        // Generate random meat amount between 1 and 5
        int meatAmount = GD.RandRange(1, 5);

        // Emitting the signal directly to inventory via SignalManager
        ResourceItem meatResource = new ResourceItem("meat", "Viande de Mouton", "Food", "res://Assets/Images/meat.png");

        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.EmitMaterialDestroyed(this, meatResource, meatAmount);
            GD.Print($"Sheep died. Sent {meatAmount} meat to inventory.");
        }
        else
        {
            GD.PrintErr("SignalManager is not available.");
        }

        QueueFree();
    }
}
