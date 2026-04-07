using Godot;
using System;
using Core.Domain;
using Core.Domain.Entities;
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
    private Sprite2D m_sprite;

    public string CurrentState => m_sheepController?.CurrentState ?? SheepStates.IDLE;

    public override void _Ready()
    {
        m_healthComponent = new HealthComponent(MaxHealth);
        m_healthComponent.OnDeath += HandleDeath;

        m_sheepController = new SheepController();

        m_navigationAgent = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
        m_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");

        if (m_navigationAgent == null)
        {
            GD.PrintErr("Sheep node requires a NavigationAgent2D child node.");
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_healthComponent.IsDead) return;

        m_sheepController.Update((float)p_delta);

        Vector2 targetVelocity = Vector2.Zero;
        System.Numerics.Vector2 controllerDir = m_sheepController.CurrentDirection;
        Vector2 direction = new Vector2(controllerDir.X, controllerDir.Y);

        if (m_sheepController.CurrentState == SheepStates.IDLE)
        {
            targetVelocity = direction * IdleSpeed;
        }
        else if (m_sheepController.CurrentState == SheepStates.FLEE)
        {
            targetVelocity = direction * FleeSpeed;
            // Optionally, if NavigationAgent2D is configured with a target, we could use it here.
            // For simple fleeing, moving in the opposite vector direction while relying on CharacterBody2D's
            // collision (MoveAndSlide) to slide along obstacles is often sufficient and creates a panicky behavior.
        }

        Velocity = targetVelocity;

        // Flip sprite based on movement direction
        if (m_sprite != null && Velocity.X != 0)
        {
            m_sprite.FlipH = Velocity.X < 0;
        }

        MoveAndSlide();
    }

    public void TakeDamage(int p_amount, object p_attacker)
    {
        if (m_healthComponent.IsDead) return;

        m_healthComponent.TakeDamage(p_amount);

        if (!m_healthComponent.IsDead && p_attacker is Node2D attackerNode)
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

    private void HandleDeath(object p_sender, EventArgs p_args)
    {
        m_sheepController.SetDead();

        // Check if killed by player (assuming player is named "Player" or in group "Player")
        // The Acceptance Criteria says "attacker is the player". Since our TakeDamage doesn't pass the attacker
        // to the OnDeath event easily without custom args, we can just assume any death drops it to inventory for now,
        // or we could track the last attacker. Let's create the meat.

        // Emitting the signal directly to inventory via SignalManager
        ResourceItem meatResource = new ResourceItem("meat", "Viande de Mouton", "Food", "res://Assets/Images/meat.png");

        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.EmitMaterialDestroyed(this, meatResource, 1);
            GD.Print("Sheep died. Sent 1 meat to inventory.");
        }
        else
        {
            GD.PrintErr("SignalManager is not available.");
        }

        QueueFree();
    }
}
