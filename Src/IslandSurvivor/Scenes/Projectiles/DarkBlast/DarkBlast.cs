using Godot;
using System;
using Core.Interfaces;
using IslandSurvivor.Scenes.Projectiles;

namespace IslandSurvivor.Scenes.Projectiles;

public partial class DarkBlast : BaseAoEAttack
{
    private bool m_isExploding = false;

    public async void PauseForWarning()
    {
        if (m_animationPlayer == null) return;

        m_isPausedForWarning = true;
        m_animationPlayer.Pause();

        await ToSignal(GetTree().CreateTimer(WarningDuration), SceneTreeTimer.SignalName.Timeout);

        if (IsInstanceValid(m_animationPlayer))
        {
            m_animationPlayer.Play();
        }
    }

    public void EnableExplosionCollision()
    {
        if (m_collisionShape != null)
        {
            m_collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
            m_isExploding = true;
        }
    }

    public void DisableExplosionCollision()
    {
        if (m_collisionShape != null)
        {
            m_collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            m_isExploding = false;
        }
    }

    protected override void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("Player") && m_isExploding)
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
