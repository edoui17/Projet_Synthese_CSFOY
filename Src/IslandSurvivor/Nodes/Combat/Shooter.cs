namespace IslandSurvivor.Nodes;

using Godot;
using IslandSurvivor.Logic;

[GlobalClass]
public partial class Shooter : Node2D
{
    [Export] public PackedScene ProjectileScene { get; set; } = null!;
    [Export] public float Cooldown { get; set; } = 2.0f;
    [Export] public Marker2D SpawnPosition { get; set; } = null!;

    private float m_cooldownTimer = 0f;
    private Node? m_owner;

    public bool CanShoot => m_cooldownTimer <= 0f;

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint()) return;

        m_owner = GetOwner<Node>();
        if (m_owner == null)
        {
            m_owner = GetParent();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint()) return;

        if (m_cooldownTimer > 0f)
        {
            m_cooldownTimer -= (float)delta;
        }
    }

    public void Shoot()
    {
        if (!CanShoot || ProjectileScene == null) return;

        Node projectileNode = ProjectileScene.Instantiate();
        if (projectileNode is IProjectile projectile)
        {
            Vector2 direction = Vector2.Right;
            Vector2 startPosition = GlobalPosition;
            if (SpawnPosition != null)
            {
                startPosition = SpawnPosition.GlobalPosition;
            }

            if (m_owner is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggNpc)
            {
                var target = aggNpc.GetTarget();
                if (GodotObject.IsInstanceValid(target))
                {
                    direction = (target.GlobalPosition - startPosition).Normalized();
                }
                else
                {
                    direction = aggNpc.LockedDirection != Vector2.Zero ? aggNpc.LockedDirection : Vector2.Right;
                }

                float damageAmount = aggNpc.Stats?.BaseAttackValue ?? 10.0f;

                Vector2 directionNumerics = new Vector2(direction.X, direction.Y);
                Vector2 startPositionNumerics = new Vector2(startPosition.X, startPosition.Y);

                projectile.Initialize(startPositionNumerics, directionNumerics, damageAmount, m_owner);
                GetTree().CurrentScene.AddChild(projectileNode);
                projectile.Fire();
            }
            else
            {
                // Fallback if not AggressiveNpcBase
                Vector2 directionNumerics = new Vector2(direction.X, direction.Y);
                Vector2 startPositionNumerics = new Vector2(startPosition.X, startPosition.Y);
                projectile.Initialize(startPositionNumerics, directionNumerics, 10.0f, m_owner ?? this);
                GetTree().CurrentScene.AddChild(projectileNode);
                projectile.Fire();
            }
        }

        m_cooldownTimer = Cooldown;
    }
}
