using Godot;
using System;

public partial class RessourceSlot : Control
{
    [Export] public TextureRect IconTextureRect = null!;
    [Export] public Label AmountLabel = null!;
    [Export] public string ResourceName = "";

    private int _lastAmount = -1;
    private Tween _currentTween = null!;
    private Tween _hoverTween = null!;

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

        _hoverTween?.Kill();
        _hoverTween = CreateTween();
        _hoverTween.TweenProperty(this, "scale", new Vector2(1.05f, 1.05f), 0.1f);
    }

    private void OnHoverExited()
    {
        _hoverTween?.Kill();
        _hoverTween = CreateTween();
        _hoverTween.TweenProperty(this, "scale", Vector2.One, 0.1f);
    }

    public void SetIcon(Texture2D texture)
    {
        IconTextureRect.Texture = texture;
    }

    public void SetAmount(int amount)
    {
        AmountLabel.Text = amount.ToString();
        TooltipText = string.IsNullOrEmpty(ResourceName) ? $"Quantity: {amount}" : $"{ResourceName}\nQuantity: {amount}";

        if (_lastAmount != -1 && amount != _lastAmount && IsInsideTree())
        {
            _currentTween?.Kill();
            _currentTween = CreateTween();

            AmountLabel.PivotOffset = AmountLabel.Size / 2f;

            Color highlightColor = amount > _lastAmount ? Colors.LimeGreen : Colors.IndianRed;

            _currentTween.TweenProperty(AmountLabel, "scale", new Vector2(1.2f, 1.2f), 0.1f);
            _currentTween.Parallel().TweenProperty(AmountLabel, "modulate", highlightColor, 0.1f);
            _currentTween.TweenProperty(AmountLabel, "scale", Vector2.One, 0.2f);
            _currentTween.Parallel().TweenProperty(AmountLabel, "modulate", Colors.White, 0.2f);
        }

        _lastAmount = amount;
    }
}
