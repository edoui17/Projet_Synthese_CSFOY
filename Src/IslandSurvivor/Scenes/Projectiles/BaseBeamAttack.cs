using Godot;
using System;

namespace IslandSurvivor.Scenes.Projectiles;

public partial class BaseBeamAttack : Node2D
{
    [Export] public float SweepSpeed { get; set; } = 1.5f;
    [Export] public int Damage { get; set; } = 30;

    protected Sprite2D m_sprite = null!;
    protected AnimationPlayer m_animationPlayer = null!;
    protected RayCast2D m_rayCast = null!;
    protected bool m_hasHitPlayer;

    public override void _Ready()
    {
        base._Ready();

        m_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        m_animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        m_rayCast = GetNodeOrNull<RayCast2D>("RayCast2D");

        // The user explicitly requested to dynamically create a RayCast2D if one does not exist.
        if (m_rayCast == null)
        {
            m_rayCast = new RayCast2D();
            m_rayCast.Name = "RayCast2D";
            // Ensure the dynamic RayCast has the necessary target position and is added to the scene
            m_rayCast.TargetPosition = new Vector2(0, 2000);
            AddChild(m_rayCast);
        }

        if (m_rayCast != null)
        {
            m_rayCast.Enabled = false;
        }

        if (m_animationPlayer != null && m_animationPlayer.HasAnimation("Attack"))
        {
            // Play the default animation
            m_animationPlayer.Play("Attack");
        }
    }

    public virtual void Initialize(Node2D targetPlayer)
    {
        if (targetPlayer != null)
        {
            LookAt(targetPlayer.GlobalPosition);
            // Default offset to align sprite. Can be overridden in derived classes if needed.
            Rotation -= Mathf.Pi / 2;
        }
    }

    public virtual void OnAnimationFinished()
    {
        QueueFree();
    }
}
