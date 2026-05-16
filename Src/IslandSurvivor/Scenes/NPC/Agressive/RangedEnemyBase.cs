namespace IslandSurvivor.Scenes.NPC.Agressive;

using Godot;
using Core.Interfaces.Entities;
using IslandSurvivor.Logic.Entities;

public abstract partial class RangedEnemyBase : EnemyBase
{
    [Export] public PackedScene ProjectileScene { get; set; }
    protected IslandSurvivor.Nodes.Combat.AttackController? m_attackController;
    private bool m_hasShotThisAttack = false;

    public override void _Ready()
    {
        base._Ready();

        m_attackController = GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>("AttackController");
        if (m_attackController != null)
        {
            m_attackController.Stats = Stats;
            m_attackController.Faction = IslandSurvivor.Enums.EntityFaction.Enemy;
            m_attackController.AttackStarted += OnAttackStarted;
        }
        else
        {
            GD.PushWarning($"{Name}: AttackController not found.");
        }

        if (m_animatedSprite != null)
        {
            m_animatedSprite.FrameChanged += OnFrameChanged;
            m_animatedSprite.AnimationFinished += OnAnimationFinished;
        }
    }

    protected override void InitializeController()
    {
        StoppingDistance = 250.0f;
        m_agressorController = new RangedController(StoppingDistance);
    }

    protected override void HandleAttackState()
    {
        if (m_targetPlayer != null && m_attackController != null && m_attackController.CanAttack)
        {
            float distanceToPlayer = GlobalPosition.DistanceTo(m_targetPlayer.GlobalPosition);

            if (distanceToPlayer <= StoppingDistance)
            {
                if (CheckLineOfSight())
                {
                    string direction = (m_animatedSprite != null && m_animatedSprite.FlipH) ? "Left" : "Right";
                    m_attackController.TryAttack(direction);
                }
            }
        }
    }

    protected virtual void OnAttackStarted()
    {
        m_hasShotThisAttack = false;
        if (m_animatedSprite != null)
        {
            m_animatedSprite.Play("Attack");
            m_animatedSprite.Frame = 0;
        }
    }

    private void OnFrameChanged()
    {
        if (m_animatedSprite == null || m_attackController == null) return;

        if (m_animatedSprite.Animation == "Attack" && m_attackController.IsAttacking)
        {
            // Arrow release frame is usually around 5 or 6 for Archer (8 frames total)
            if (m_animatedSprite.Frame >= 5 && !m_hasShotThisAttack)
            {
                ShootProjectile();
                m_hasShotThisAttack = true;
            }
        }
    }

    private void OnAnimationFinished()
    {
        if (m_animatedSprite == null || m_attackController == null) return;

        if (m_animatedSprite.Animation == "Attack")
        {
            m_attackController.CancelAttack();
        }
    }

    protected virtual void ShootProjectile()
    {
        if (ProjectileScene == null || m_targetPlayer == null) return;

        Node projectileNode = ProjectileScene.Instantiate();
        if (projectileNode is IProjectile projectile)
        {
            // Calculate direction to player
            Godot.Vector2 directionGodot = (m_targetPlayer.GlobalPosition - GlobalPosition).Normalized();
            System.Numerics.Vector2 directionNumerics = new System.Numerics.Vector2(directionGodot.X, directionGodot.Y);
            System.Numerics.Vector2 startPositionNumerics = new System.Numerics.Vector2(GlobalPosition.X, GlobalPosition.Y);

            float damageAmount = Stats?.BaseAttackValue ?? 10.0f;

            projectile.Initialize(startPositionNumerics, directionNumerics, damageAmount, this);

            GetTree().CurrentScene.AddChild(projectileNode);

            projectile.Fire();
        }
    }
}
