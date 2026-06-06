using Godot;
using System;
using Core.Interfaces;
using IslandSurvivor.Scenes.Projectiles;

namespace IslandSurvivor.Scenes.Projectiles;

public partial class EvilEye : BaseBeamAttack
{
    [Export] public int LaserHitGroundFrame { get; set; } = 10;
    [Export] public int SweepStartFrame { get; set; } = 13;
    [Export] public int SweepEndFrame { get; set; } = 18;
    [Export] public int ClosingEyeFrame { get; set; } = 21;

    public override void _Ready()
    {
        base._Ready();

        if (m_rayCast != null)
        {
            m_rayCast.TargetPosition = new Vector2(0, 2000);
        }
    }

    protected override void OnFrameChanged()
    {
        if (m_animatedSprite == null || m_rayCast == null) return;

        int frame = m_animatedSprite.Frame;

        if (frame == LaserHitGroundFrame)
        {
            m_rayCast.Enabled = true;
            m_hasHitPlayer = false;
        }
        else if (frame == ClosingEyeFrame)
        {
            m_rayCast.Enabled = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (m_animatedSprite == null || !m_animatedSprite.IsPlaying() || m_rayCast == null) return;

        int frame = m_animatedSprite.Frame;

        if (frame >= SweepStartFrame && frame <= SweepEndFrame)
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
