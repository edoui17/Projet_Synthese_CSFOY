using Godot;
using Core.Managers.Stats;
using IslandSurvivor.Nodes;

namespace IslandSurvivor.Nodes.Movement;

public partial class MovementController : Node
{
    [Export] public StatManager? Stats { get; set; }

    private CharacterBody2D? m_parentBody;

    public override void _Ready()
    {
        base._Ready();

        m_parentBody = GetParent() as CharacterBody2D;

        if (m_parentBody == null)
        {
            GD.PushWarning("MovementController: Parent is not a CharacterBody2D.");
        }
    }

    public void Move(Vector2 p_direction, float? p_customBaseSpeed = null)
    {
        if (m_parentBody == null) return;

        float baseSpeedToUse = p_customBaseSpeed ?? Stats?.BaseSpeed ?? 300f;

        // 1 stat point = +5% speed
        float speedStat = Stats?.GetCurrentValue(StatType.Speed) ?? 0f;
        float finalSpeed = baseSpeedToUse * (1f + (speedStat * 0.05f));

        m_parentBody.Velocity = p_direction * finalSpeed;
        m_parentBody.MoveAndSlide();
    }
}
