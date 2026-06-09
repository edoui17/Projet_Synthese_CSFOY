namespace IslandSurvivor.Scenes.Projectiles;

using Godot;
using IslandSurvivor.Logic;

public partial class OrbProjectile : BaseProjectile
{
    [ExportGroup("Seeking Settings")]
    [Export] public float SeekingDelay { get; set; } = 1.0f;
    [Export] public float DetectionRadius { get; set; } = 200.0f;
    [Export] public float SeekingSpeedMultiplier { get; set; } = 1.5f;

    private float m_aliveTime = 0.0f;
    private bool m_isSeeking = false;
    private Node2D m_target = null!;

    public override void _PhysicsProcess(double p_delta)
    {
        if (!m_isSeeking)
        {
            m_aliveTime += (float)p_delta;
            if (m_aliveTime >= SeekingDelay)
            {
                TryFindTarget();
            }
        }

        if (m_isSeeking && GodotObject.IsInstanceValid(m_target))
        {
            Vector2 directionToTarget = (m_target.GlobalPosition - GlobalPosition).Normalized();

            // Adjust rotation smoothly or instantly depending on desired behavior. Here, instant.
            Rotation = directionToTarget.Angle();
            m_velocity = directionToTarget * (Speed * SeekingSpeedMultiplier);
        }

        base._PhysicsProcess(p_delta);
    }

    private void TryFindTarget()
    {
        m_isSeeking = true;

        // Use Godot's SceneTree to find the player within range.
        var nodes = GetTree().GetNodesInGroup("Player");
        if (nodes.Count > 0 && nodes[0] is Node2D playerNode)
        {
            if (GlobalPosition.DistanceTo(playerNode.GlobalPosition) <= DetectionRadius)
            {
                m_target = playerNode;
            }
        }
    }
}
