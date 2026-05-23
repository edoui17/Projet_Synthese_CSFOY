using Godot;
using System;
using Core.Interfaces.Stats;
using IslandSurvivor.Scenes.Projectiles;

namespace IslandSurvivor.Scenes.Projectiles.DarkBlast;

public partial class DarkBlast : BaseAoEAttack
{
    [Export] public int WarningFrame { get; set; } = 4;
    [Export] public int ExplosionStartFrame { get; set; } = 15;
    [Export] public int ExplosionEndFrame { get; set; } = 23;

    protected override async void OnFrameChanged()
    {
        if (m_animatedSprite == null || m_collisionShape == null) return;

        int frame = m_animatedSprite.Frame;

        if (frame == WarningFrame && !m_isPausedForWarning)
        {
            m_isPausedForWarning = true;
            m_animatedSprite.Pause();

            await ToSignal(GetTree().CreateTimer(WarningDuration), SceneTreeTimer.SignalName.Timeout);

            if (IsInstanceValid(m_animatedSprite))
            {
                m_animatedSprite.Play();
            }
        }
        else if (frame == ExplosionStartFrame)
        {
            m_collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
        }
    }

    protected override void OnBodyEntered(Node2D body)
    {
        if (m_animatedSprite == null) return;

        if (body.IsInGroup("Player") && m_animatedSprite.Frame >= ExplosionStartFrame && m_animatedSprite.Frame <= ExplosionEndFrame)
        {
            if (body is IDamageable damageable)
            {
                damageable.TakeDamage(Damage, this);
            }
            else if (body.HasMethod("TakeDamage"))
            {
                body.Call("TakeDamage", Damage);
            }
        }
    }
}
