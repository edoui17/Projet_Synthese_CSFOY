namespace IslandSurvivor.Scenes.NPC.Aggressive;

using Godot;

public partial class MeleeAggressiveNpcBase : AggressiveNpcBase
{
    protected Area2D m_hitboxAreaRight;
    protected Area2D m_hitboxAreaLeft;

    public override void _Ready()
    {
        base._Ready();

        m_hitboxAreaRight = GetNodeOrNull<Area2D>("HitboxAreaRight");
        m_hitboxAreaLeft = GetNodeOrNull<Area2D>("HitboxAreaLeft");

        if (m_attackController != null)
        {
            m_attackController.Stats = Stats;
            m_attackController.Faction = IslandSurvivor.Enums.EntityFaction.Enemy;
            m_attackController.AttackSprite = m_animatedSprite;
            m_attackController.ActionFrame = 2; // Impact frame for Melee

            if (m_hitboxAreaRight != null) m_attackController.RegisterArea("Right", m_hitboxAreaRight);
            if (m_hitboxAreaLeft != null) m_attackController.RegisterArea("Left", m_hitboxAreaLeft);

            m_attackController.AttackStarted += OnAttackStarted;
        }
        else
        {
            GD.PushWarning($"{Name}: AttackController not found.");
        }
    }

    protected override void HandleAttackState()
    {
        if (m_attackController != null && m_attackController.CanAttack && m_targetPlayer != null)
        {
            float distanceToPlayer = GlobalPosition.DistanceTo(m_targetPlayer.GlobalPosition);

            if (distanceToPlayer <= 50f)
            {
                string direction = (m_animatedSprite != null && m_animatedSprite.FlipH) ? "Left" : "Right";

                // Si la gestion du son n'est pas incluse dans TryAttack, vous pouvez la mettre ici :
                AudioStream attackStream = GD.Load<AudioStream>("res://Assets/Sounds/Combat/enemy_attack.wav");
                if (attackStream != null)
                {
                    IslandSurvivor.Globals.AudioManager.Instance?.PlaySound2D(attackStream, GlobalPosition);
                }

                m_attackController.TryAttack(direction);
            }
        }
    }

    protected virtual void OnAttackStarted()
    {
        if (m_animatedSprite != null)
        {
            m_animatedSprite.Play("Attack");
            m_animatedSprite.Frame = 0;
        }
    }
}