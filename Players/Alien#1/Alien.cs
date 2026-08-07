using Godot;

public partial class Alien : CharacterBody2D
{
    private Node2D _visuals = null!;
    private AnimatedSprite2D _sprite = null!;
    private Control _thoughtBubble = null!;
    private Label _thoughtLabel = null!;
    private Timer _thoughtTimer = null!;
    
    public override void _Ready()
    {
        _visuals = GetNode<Node2D>("Visuals");
        _sprite = GetNode<AnimatedSprite2D>("Visuals/AnimatedSprite2D");
        _thoughtBubble = GetNode<Control>("ThoughtBubble");
        _thoughtLabel = GetNode<Label>("ThoughtBubble/ThoughtLabel");
        _thoughtTimer = GetNode<Timer>("ThoughtTimer");
        
        // Floating mode is suitable for top-down and point-and-click movement
        // because it does not apply platform-style floor and wall behavior.
        MotionMode = MotionModeEnum.Floating;

        // Sets the player's initial horizontal facing direction.
        _sprite.FlipH = true;

        _thoughtTimer.Timeout += HideThought;
        _thoughtBubble.Hide();

        PlayAnimation("idle");
    }

    public void ShowThought(string text, double duration = 3.0)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        _thoughtLabel.Text = text;
        _thoughtBubble.Show();
        _thoughtTimer.Stop();
        _thoughtTimer.WaitTime = duration;
        _thoughtTimer.Start();

        Callable.From(KeepThoughtBubbleInsideViewport).CallDeferred();
    }

    private void KeepThoughtBubbleInsideViewport()
    {
        if (!_thoughtBubble.Visible)
        {
            return;
        }

        const float screenMargin = 16.0f;
        Rect2 viewportRect = GetViewportRect();
        Vector2 bubbleTopLeft = GlobalPosition + _thoughtBubble.Position;
        Vector2 minimum = new(screenMargin, screenMargin);
        Vector2 maximum = new(
            Mathf.Max(screenMargin, viewportRect.Size.X - screenMargin - _thoughtBubble.Size.X),
            Mathf.Max(screenMargin, viewportRect.Size.Y - screenMargin - _thoughtBubble.Size.Y)
        );
        Vector2 clampedTopLeft = new(
            Mathf.Clamp(bubbleTopLeft.X, minimum.X, maximum.X),
            Mathf.Clamp(bubbleTopLeft.Y, minimum.Y, maximum.Y)
        );

        _thoughtBubble.Position += clampedTopLeft - bubbleTopLeft;
    }

    private void HideThought() => _thoughtBubble.Hide();

    
    private void PlayAnimation(StringName animationName)
    {
        if (_sprite.SpriteFrames == null)
        {
            return;
        }

        if (!_sprite.SpriteFrames.HasAnimation(animationName))
        {
            return;
        }

        if (_sprite.Animation != animationName ||
            !_sprite.IsPlaying())
        {
            _sprite.Play(animationName);
        }
    }
}
