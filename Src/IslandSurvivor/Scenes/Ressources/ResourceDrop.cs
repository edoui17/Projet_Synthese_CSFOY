using Godot;
using Core.Domain;
using IslandSurvivor.Globals;
using System;

namespace IslandSurvivor.Scenes.Ressources;

public partial class ResourceDrop : Node2D
{
    private ResourceItem m_item;
    private int m_quantity;
    private Vector2 m_targetPosition;
    private Sprite2D m_sprite;

    public void Initialize(ResourceItem p_item, int p_quantity, Vector2 p_startPosition, Vector2 p_targetPosition)
    {
        m_item = p_item;
        m_quantity = p_quantity;
        GlobalPosition = p_startPosition;
        m_targetPosition = p_targetPosition;
    }

    public override void _Ready()
    {
        // 1. Create a sprite to visualize the drop
        m_sprite = new Sprite2D();

        // Try to load the texture, fallback to an icon if not found
        Texture2D texture = GD.Load<Texture2D>(m_item.IconPath);
        if (texture != null)
        {
            m_sprite.Texture = texture;
        }

        AddChild(m_sprite);

        // 2. Add an animation to burst out slightly before moving to the target
        AnimateDrop();
    }

    private void AnimateDrop()
    {
        Tween tween = CreateTween();

        // Random offset for burst effect
        Random random = new();
        float burstX = GlobalPosition.X + (float)(random.NextDouble() * 40 - 20);
        float burstY = GlobalPosition.Y - (float)(random.NextDouble() * 30 + 10);
        Vector2 burstPosition = new Vector2(burstX, burstY);

        // Burst out
        tween.TweenProperty(this, "global_position", burstPosition, 0.3f)
             .SetTrans(Tween.TransitionType.Quad)
             .SetEase(Tween.EaseType.Out);

        // Optional small pause
        tween.TweenInterval(0.1f);

        // Move to target position (e.g. inventory UI or player)
        // Since UI position might be hard to map to world coordinates directly without specific setup,
        // we'll tween to the target position provided (e.g., Player's position).
        tween.TweenProperty(this, "global_position", m_targetPosition, 0.6f)
             .SetTrans(Tween.TransitionType.Back)
             .SetEase(Tween.EaseType.In);

        // Scale down as it gets closer
        tween.Parallel().TweenProperty(this, "scale", Vector2.Zero, 0.6f)
             .SetDelay(0.4f); // Start shrinking halfway through the move

        // When animation finishes, notify inventory and destroy itself
        tween.Finished += OnAnimationFinished;
    }

    private void OnAnimationFinished()
    {
        // Emit signal to update inventory
        SignalManager.Instance.EmitMaterialDestroyed(this, m_item, m_quantity);
        QueueFree();
    }
}
