using Godot;
using System;
using Core.Interfaces;
using IslandSurvivor.Scenes.Projectiles;

namespace IslandSurvivor.Scenes.Projectiles;

public partial class EvilEye : BaseBeamAttack
{
    private bool m_isSweeping = false;

    public override void _Ready()
    {
        base._Ready();

        if (m_rayCast != null)
        {
            m_rayCast.TargetPosition = new Vector2(0, 2000);
        }
    }

    public void OnLaserHitGround()
    {
        if (m_rayCast == null) return;
        m_rayCast.Enabled = true;
        m_hasHitPlayer = false;
    }

    public void SetSweeping(bool p_isSweeping)
    {
        m_isSweeping = p_isSweeping;
    }

    public void OnClosingEye()
    {
        if (m_rayCast == null) return;
        m_rayCast.Enabled = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (m_animationPlayer == null || !m_animationPlayer.IsPlaying() || m_rayCast == null) return;

        if (m_isSweeping)
        {
            m_rayCast.Rotation -= SweepSpeed * (float)delta;
        }

        if (m_rayCast.Enabled && m_rayCast.IsColliding() && !m_hasHitPlayer)
        {
            Node collider = (Node)m_rayCast.GetCollider();
            if (collider != null && collider.IsInGroup("Player"))
            {
                m_hasHitPlayer = true;

                if (collider is IDamageable damageable)
                {
                    damageable.TakeDamage(Damage, this);
                }
                else if (collider.HasMethod("TakeDamage"))
                {
                    collider.Call("TakeDamage", Damage);
                }
            }
        }
    }
}
