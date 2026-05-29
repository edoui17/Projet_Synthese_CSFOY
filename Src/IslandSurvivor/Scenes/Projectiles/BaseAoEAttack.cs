using Godot;
using System;

namespace IslandSurvivor.Scenes.Projectiles;

public partial class BaseAoEAttack : Area2D
{
    [Export] public float WarningDuration { get; set; } = 1.2f;
    [Export] public int Damage { get; set; } = 20;

    protected AnimatedSprite2D m_animatedSprite = null!;
    protected CollisionShape2D m_collisionShape = null!;
    protected bool m_isPausedForWarning;

    public override void _Ready()
    {
        base._Ready();

        m_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        m_collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

        if (m_collisionShape != null)
        {
            // Initially disable collision shape
            m_collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        }

        if (m_animatedSprite != null)
        {
            // Connect signals
            m_animatedSprite.FrameChanged += OnFrameChanged;
            m_animatedSprite.AnimationFinished += OnAnimationFinished;

            // Play the default animation
            m_animatedSprite.Play();
        }

        BodyEntered += OnBodyEntered;
    }

    protected virtual void OnFrameChanged() { }
    protected virtual void OnBodyEntered(Node2D body) { }

    protected virtual void OnAnimationFinished()
    {
        QueueFree();
    }
}
