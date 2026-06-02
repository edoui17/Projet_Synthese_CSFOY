using Godot;
using Core.Domain;
using IslandSurvivor.Globals;
using System;

namespace IslandSurvivor.Scenes;

public partial class ResourceDrop : Node2D
{
    private ResourceItem m_item = null!;
    private int m_quantity;
    private Node2D m_targetNode = null!;
    private Vector2 m_fallbackTargetPosition;
    private Sprite2D m_sprite = null!;
    private Vector2 m_startLerpPosition;

    public void Initialize(ResourceItem p_item, int p_quantity, Vector2 p_startPosition, Node2D p_targetNode, Vector2 p_fallbackTargetPosition)
    {
        m_item = p_item;
        m_quantity = p_quantity;
        GlobalPosition = p_startPosition;
        m_targetNode = p_targetNode;
        m_fallbackTargetPosition = p_fallbackTargetPosition;
    }

    public override void _Ready()
    {
        // 1. Create a sprite to visualize the drop
        m_sprite = new Sprite2D();

        // Try to load the texture, fallback to an icon if not found
        if (!string.IsNullOrEmpty(m_item.IconPath) && ResourceLoader.Exists(m_item.IconPath))
        {
            Texture2D texture = GD.Load<Texture2D>(m_item.IconPath);
            if (texture != null)
            {
                m_sprite.Texture = texture;
            }
        }

        AddChild(m_sprite);

        // Scale it down so it's not too large on the ground
        Scale = new Vector2(0.5f, 0.5f);

        // 2. Add an animation to burst out slightly before moving to the target
        AnimateBurst();
    }

    private void AnimateBurst()
    {
        Tween burstTween = CreateTween();

        // Random offset for burst effect
        Random random = new();
        float burstX = GlobalPosition.X + (float)(random.NextDouble() * 60 - 30);
        float burstY = GlobalPosition.Y - (float)(random.NextDouble() * 50 - 10);
        Vector2 burstPosition = new Vector2(burstX, burstY);

        // Burst out
        burstTween.TweenProperty(this, "global_position", burstPosition, 0.4f)
             .SetTrans(Tween.TransitionType.Quad)
             .SetEase(Tween.EaseType.Out);

        // Wait 1 second on the floor, then start moving to target
        burstTween.TweenInterval(1.0f);
        burstTween.Finished += AnimateToTarget;
    }

    private void AnimateToTarget()
    {
        // Save the position where the drop currently is after the burst, so the lerp has a stable origin.
        m_startLerpPosition = GlobalPosition;

        // Continuous tween to track moving target
        Tween moveTween = CreateTween();

        // Increase travel time to 0.8 seconds so it doesn't disappear too quickly
        float travelDuration = 0.8f;
        float shrinkDuration = 0.4f;
        float shrinkDelay = travelDuration - shrinkDuration; // Start shrinking halfway through

        // We will do a generic movement using _Process instead to track the player,
        // but since we want to use Tween for ease-in, we can tween property over time.
        // However, a standard tween locks the target position at start.
        // We can use TweenMethod to pass a value from 0 to 1 and lerp.
        moveTween.TweenMethod(Callable.From<float>(MoveStep), 0.0f, 1.0f, travelDuration)
             .SetTrans(Tween.TransitionType.Back)
             .SetEase(Tween.EaseType.In);

        // Delay the scale reduction so the resource stays larger for longer
        moveTween.Parallel().TweenProperty(this, "scale", Vector2.Zero, shrinkDuration)
             .SetDelay(shrinkDelay)
             .SetTrans(Tween.TransitionType.Expo)
             .SetEase(Tween.EaseType.In);

        moveTween.Finished += OnAnimationFinished;
    }

    private void MoveStep(float p_progress)
    {
        Vector2 currentTargetPos = m_fallbackTargetPosition;
        if (GodotObject.IsInstanceValid(m_targetNode) && m_targetNode.IsInsideTree())
        {
            currentTargetPos = m_targetNode.GlobalPosition;
        }

        GlobalPosition = m_startLerpPosition.Lerp(currentTargetPos, p_progress);
    }

    private void OnAnimationFinished()
    {
        // Emit signal to update inventory
        SignalManager.Instance.EmitMaterialDestroyed(this, m_item, m_quantity);
        QueueFree();
    }
}
