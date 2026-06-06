using Godot;
using System;

namespace IslandSurvivor.Scenes.Projectiles;

public partial class BaseAoEAttack : Area2D
{
    [Export] public float WarningDuration { get; set; } = 1.2f;
    [Export] public int Damage { get; set; } = 20;

    protected Sprite2D m_sprite = null!;
    protected AnimationPlayer m_animationPlayer = null!;
    protected CollisionShape2D m_collisionShape = null!;
    protected bool m_isPausedForWarning;

    public override void _Ready()
    {
        base._Ready();

        m_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        m_animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        m_collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

        if (m_collisionShape != null)
        {
            // Initially disable collision shape
            m_collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        }

        if (m_animationPlayer != null && m_animationPlayer.HasAnimation("Attack"))
        {
            // Play the default animation
            m_animationPlayer.Play("Attack");
        }

        BodyEntered += OnBodyEntered;
    }

    protected virtual void OnBodyEntered(Node2D body) { }

    public virtual void OnAnimationFinished()
    {
        QueueFree();
    }
}
