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

    /// <summary>
    /// Checks if there is a clear line of sight between this node and a target node.
    /// It uses a raycast to check for collisions against the specified collision mask.
    /// </summary>
    public static bool HasLineOfSightTo(this Node2D p_from, Node2D p_to, uint p_collisionMask = 1)
    {
        if (!GodotObject.IsInstanceValid(p_from) || !p_from.IsInsideTree() ||
            !GodotObject.IsInstanceValid(p_to) || !p_to.IsInsideTree())
            return false;

        var spaceState = p_from.GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(p_from.GlobalPosition, p_to.GlobalPosition, p_collisionMask);

        // Exclude the nodes themselves from the raycast check
        var excludeArray = new Godot.Collections.Array<Rid>();
        if (p_from is CollisionObject2D fromCollision)
        {
            excludeArray.Add(fromCollision.GetRid());
        }
        if (p_to is CollisionObject2D toCollision)
        {
            excludeArray.Add(toCollision.GetRid());
        }
        query.Exclude = excludeArray;

        var result = spaceState.IntersectRay(query);

        // If the dictionary is empty, it means the ray hit nothing on the specified mask
        // which means the line of sight is clear.
        return result.Count == 0;
    }
}
