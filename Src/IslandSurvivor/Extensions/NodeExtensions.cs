using Godot;

namespace IslandSurvivor.Extensions;

public static class NodeExtensions
{
    public static void PlayHitFlash(this CanvasItem p_canvasItem)
    {
        p_canvasItem.Modulate = new Color(1, 0.5f, 0.5f);
        p_canvasItem.GetTree().CreateTimer(0.2f).Timeout += () =>
        {
            if (GodotObject.IsInstanceValid(p_canvasItem))
            {
                p_canvasItem.Modulate = Colors.White;
            }
        };
    }
}
