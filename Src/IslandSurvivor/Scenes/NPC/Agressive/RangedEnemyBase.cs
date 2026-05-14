namespace IslandSurvivor.Scenes.NPC.Agressive;

using Godot;
using Core.Interfaces.Entities;
using IslandSurvivor.Logic.Entities;

public abstract partial class RangedEnemyBase : EnemyBase
{
    [Export] public PackedScene ProjectileScene { get; set; }

    protected override void InitializeController()
    {
        StoppingDistance = 250.0f;
        m_agressorController = new RangedController(StoppingDistance);
    }

    protected override async void HandleAttackState()
    {
        if (m_targetPlayer != null && m_agressorController is IRangedController controller)
        {
            float distanceToPlayer = GlobalPosition.DistanceTo(m_targetPlayer.GlobalPosition);

            if (distanceToPlayer <= StoppingDistance)
            {
                if (controller.CanAttack() && CheckLineOfSight())
                {
                    controller.StartAttack();

                    await ToSignal(GetTree().CreateTimer(0.4f), SceneTreeTimer.SignalName.Timeout);
                    if (m_agressorController.CurrentState == NpcStates.DEAD) return;

                    ShootProjectile();
                }
            }
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

            float damageAmount = Stats?.BaseDamage ?? 10.0f;

            projectile.Initialize(startPositionNumerics, directionNumerics, damageAmount, this);

            GetTree().CurrentScene.AddChild(projectileNode);

            projectile.Fire();
        }
    }
}
