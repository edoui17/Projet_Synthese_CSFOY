namespace IslandSurvivor.Logic.MagicSpells;

using Godot;

[GlobalClass]
public abstract partial class MagicSpellNode : Node2D
{
    [Export] public float SelectionWeight { get; set; } = 1.0f;

    [Signal]
    public delegate void SpellFinishedEventHandler();

    /// <summary>
    /// Executes the magic spell logic.
    /// </summary>
    /// <param name="p_shooter">The node casting the spell.</param>
    /// <param name="p_targetPos">The initial targeted position.</param>
    public abstract void Execute(Node2D p_shooter, Vector2 p_targetPos);
}
