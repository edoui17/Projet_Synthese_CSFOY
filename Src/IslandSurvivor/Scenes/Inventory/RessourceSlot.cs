using Godot;
using System;

public partial class RessourceSlot : Control
{
    [Export] public TextureRect IconTextureRect = null!;
    [Export] public Label AmountLabel = null!;
    [Export] public string ResourceName = "";

    private int m_lastAmount = -1;
    private Tween m_currentTween = null!;
    private Tween m_hoverTween = null!;

    public override void _Ready()
    {
        FocusMode = FocusModeEnum.All;
        MouseDefaultCursorShape = CursorShape.PointingHand;

        MouseEntered += OnHoverEntered;
        FocusEntered += OnHoverEntered;

        MouseExited += OnHoverExited;
        FocusExited += OnHoverExited;
    }

    private void OnHoverEntered()
    {
        PivotOffset = Size / 2f;
        GrabFocus();

        m_hoverTween?.Kill();
        m_hoverTween = CreateTween();
        m_hoverTween.TweenProperty(this, "scale", new Vector2(1.05f, 1.05f), 0.1f);
    }

    private void OnHoverExited()
    {
        m_hoverTween?.Kill();
        m_hoverTween = CreateTween();
        m_hoverTween.TweenProperty(this, "scale", Vector2.One, 0.1f);
    }

    public void SetIcon(Texture2D texture)
    {
        IconTextureRect.Texture = texture;
    }

    public void SetAmount(int amount)
    {
        AmountLabel.Text = amount.ToString();
        TooltipText = string.IsNullOrEmpty(ResourceName) ? $"Quantity: {amount}" : $"{ResourceName}\nQuantity: {amount}";

        if (m_lastAmount != -1 && amount != m_lastAmount && IsInsideTree())
        {
            m_currentTween?.Kill();
            m_currentTween = CreateTween();

            AmountLabel.PivotOffset = AmountLabel.Size / 2f;

            Color highlightColor = amount > m_lastAmount ? Colors.LimeGreen : Colors.IndianRed;

            m_currentTween.TweenProperty(AmountLabel, "scale", new Vector2(1.2f, 1.2f), 0.1f);
            m_currentTween.Parallel().TweenProperty(AmountLabel, "modulate", highlightColor, 0.1f);
            m_currentTween.TweenProperty(AmountLabel, "scale", Vector2.One, 0.2f);
            m_currentTween.Parallel().TweenProperty(AmountLabel, "modulate", Colors.White, 0.2f);
        }

        m_lastAmount = amount;
    }
}
