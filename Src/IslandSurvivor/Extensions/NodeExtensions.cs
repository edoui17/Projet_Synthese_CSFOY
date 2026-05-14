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

    /// <summary>
    /// Creates a shake effect for a Node2D using Tweeners.
    /// Useful for impact feedback on resources, players, and enemies.
    /// </summary>
    public static void PlayShake(this Node2D p_node, float p_duration = 0.2f, float p_intensity = 5.0f)
    {
        if (!GodotObject.IsInstanceValid(p_node) || !p_node.IsInsideTree()) return;

        Vector2 originalPosition = p_node.Position;
        Tween tween = p_node.CreateTween();

        // Number of shakes
        int shakes = 5;
        float shakeDuration = p_duration / shakes;

        for (int i = 0; i < shakes; i++)
        {
            Vector2 randomOffset = new Vector2(
                (float)GD.RandRange(-p_intensity, p_intensity),
                (float)GD.RandRange(-p_intensity, p_intensity)
            );

            tween.TweenProperty(p_node, "position", originalPosition + randomOffset, shakeDuration)
                 .SetTrans(Tween.TransitionType.Sine)
                 .SetEase(Tween.EaseType.InOut);
        }

        // Return to original position
        tween.TweenProperty(p_node, "position", originalPosition, shakeDuration)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.InOut);
    }
}
