using Godot;
using System;

public partial class RessourceSlot : Control
{
    [Export] public TextureRect IconTextureRect = null!;
    [Export] public Label AmountLabel = null!;

    private int _lastAmount = -1;
    private Tween _currentTween;

    public void SetIcon(Texture2D texture)
    {
        IconTextureRect.Texture = texture;
    }

    public void SetAmount(int amount)
    {
        AmountLabel.Text = amount.ToString();

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
