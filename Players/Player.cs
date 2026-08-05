using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Player : CharacterBody2D
{
	[ExportGroup("Movement")]
	[Export]
	public float MoveSpeed { get; set; } = 220.0f;

	[Export]
	public float StopDistance { get; set; } = 4.0f;
	
	[ExportGroup("Perspective")]
	[Export]
	public float FarY { get; set; } = 360.0f;

	[Export]
	public float NearY { get; set; } = 630.0f;

	[Export]
	public float FarScale { get; set; } = 0.65f;

	[Export]
	public float NearScale { get; set; } = 1.05f;
	
	[ExportGroup("Thought Bubble")]
	[Export]
	public float ThoughtBubbleScreenMargin { get; set; } = 16.0f;
	
	private Node2D _visuals = null!;
	private Vector2 _baseVisualScale;
	private Vector2 _baseThoughtBubblePosition;
	private float _currentPerspectiveScale = 1.0f;
	private int _thoughtSequenceId;
	
	[Signal]
	public delegate void DestinationReachedEventHandler();

	private AnimatedSprite2D _sprite = null!;
	private NavigationAgent2D _navigationAgent = null!;

	private bool _hasTarget;

	private PanelContainer _thoughtBubble = null!;
	private Label _thoughtLabel = null!;
	private Timer _thoughtTimer = null!;

	public override void _Ready()
	{
		_visuals = GetNode<Node2D>("Visuals");
		_sprite = GetNode<AnimatedSprite2D>("Visuals/AnimatedSprite2D");
		_navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		_thoughtBubble = GetNode<PanelContainer>("ThoughtBubble");
		_thoughtLabel = GetNode<Label>("ThoughtBubble/ThoughtLabel");
		_thoughtTimer = GetNode<Timer>("ThoughtTimer");

		MotionMode = MotionModeEnum.Floating;

		_sprite.FlipH = true;
		
		_navigationAgent.PathDesiredDistance = 5.0f;
		_navigationAgent.TargetDesiredDistance = StopDistance;
		_navigationAgent.DebugEnabled = false;

		_baseVisualScale = _visuals.Scale;
		_baseThoughtBubblePosition = _thoughtBubble.Position;

		UpdatePerspectiveScale();

		_thoughtTimer.Timeout += HideThought;
		_thoughtBubble.Hide();

		PlayAnimation("idle");
	}

	public void MoveTo(Vector2 globalTarget)
	{
		_navigationAgent.TargetPosition = globalTarget;
		_hasTarget = true;
	}

	public void Stop()
	{
		_hasTarget = false;
		Velocity = Vector2.Zero;

		PlayAnimation("idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		UpdatePerspectiveScale();
		if (!_hasTarget)
		{
			Velocity = Vector2.Zero;
			return;
		}

		if (_navigationAgent.IsNavigationFinished())
		{
			FinishMovement();
			return;
		}

		Vector2 nextPathPosition = _navigationAgent.GetNextPathPosition();
		Vector2 toNextPosition = nextPathPosition - GlobalPosition;

		float distance = toNextPosition.Length();

		if (distance <= 0.01f)
		{
			Velocity = Vector2.Zero;
			return;
		}

		Vector2 direction = toNextPosition.Normalized();

		float perspectiveMoveSpeed = MoveSpeed * _currentPerspectiveScale;

		float currentSpeed = Mathf.Min(perspectiveMoveSpeed, distance / (float)delta);

		Velocity = direction * currentSpeed;

		UpdateFacingDirection(direction);
		PlayAnimation("walk");

		MoveAndSlide();
	}

	public async Task ShowThoughtSequenceAsync(IReadOnlyList<ThoughtLine> lines, double initialDelay = 0.0)
	{
		if (lines.Count == 0)
		{
			return;
		}

		int sequenceId = ++_thoughtSequenceId;

		if (initialDelay > 0.0)
		{
			bool canContinue = await WaitForSequenceAsync(initialDelay, sequenceId);

			if (!canContinue)
			{
				return;
			}
		}

		for (int index = 0; index < lines.Count; index++)
		{
			if (sequenceId != _thoughtSequenceId)
			{
				return;
			}

			ThoughtLine line = lines[index];

			ShowThoughtInternal(line.Text, line.Duration);

			bool canContinue = await WaitForSequenceAsync(line.Duration, sequenceId);

			if (!canContinue)
			{
				return;
			}

			bool hasAnotherLine = index < lines.Count - 1;

			if (hasAnotherLine && line.PauseAfter > 0.0)
			{
				canContinue = await WaitForSequenceAsync(line.PauseAfter, sequenceId);
				
				if (!canContinue)
				{
					return;
				}
			}
		}
	}

	private void ClampThoughtBubbleToViewport()
	{
	    if (!_thoughtBubble.Visible)
	    {
	        return;
	    }
	
	    Rect2 viewportRect = GetViewport().GetVisibleRect();
	
	    Transform2D bubbleToScreen = _thoughtBubble.GetGlobalTransformWithCanvas();
	
	    Vector2 topLeft =
	        bubbleToScreen * Vector2.Zero;
	
	    Vector2 topRight =
	        bubbleToScreen *
	        new Vector2(_thoughtBubble.Size.X, 0.0f);
	
	    Vector2 bottomLeft =
	        bubbleToScreen *
	        new Vector2(0.0f, _thoughtBubble.Size.Y);
	
	    Vector2 bottomRight =
	        bubbleToScreen * _thoughtBubble.Size;
	
	    float bubbleLeft = Mathf.Min(
	        Mathf.Min(topLeft.X, topRight.X),
	        Mathf.Min(bottomLeft.X, bottomRight.X)
	    );
	
	    float bubbleRight = Mathf.Max(
	        Mathf.Max(topLeft.X, topRight.X),
	        Mathf.Max(bottomLeft.X, bottomRight.X)
	    );
	
	    float bubbleTop = Mathf.Min(
	        Mathf.Min(topLeft.Y, topRight.Y),
	        Mathf.Min(bottomLeft.Y, bottomRight.Y)
	    );
	
	    float bubbleBottom = Mathf.Max(
	        Mathf.Max(topLeft.Y, topRight.Y),
	        Mathf.Max(bottomLeft.Y, bottomRight.Y)
	    );
	
	    float allowedLeft = viewportRect.Position.X + ThoughtBubbleScreenMargin;
	
	    float allowedRight = viewportRect.Position.X + viewportRect.Size.X - ThoughtBubbleScreenMargin;
	
	    float allowedTop = viewportRect.Position.Y + ThoughtBubbleScreenMargin;
	
	    float allowedBottom = viewportRect.Position.Y + viewportRect.Size.Y - ThoughtBubbleScreenMargin;
	
	    Vector2 screenCorrection = Vector2.Zero;
	
	    if (bubbleLeft < allowedLeft)
	    {
	        screenCorrection.X =
	            allowedLeft - bubbleLeft;
	    }
	    else if (bubbleRight > allowedRight)
	    {
	        screenCorrection.X =
	            allowedRight - bubbleRight;
	    }
	
	    if (bubbleTop < allowedTop)
	    {
	        screenCorrection.Y =
	            allowedTop - bubbleTop;
	    }
	    else if (bubbleBottom > allowedBottom)
	    {
	        screenCorrection.Y =
	            allowedBottom - bubbleBottom;
	    }
	
	    if (screenCorrection.IsZeroApprox())
	    {
	        return;
	    }
	    
	    Transform2D screenToPlayer = GetGlobalTransformWithCanvas().AffineInverse();
	
	    Vector2 localOrigin = screenToPlayer * Vector2.Zero;
	
	    Vector2 localCorrection = screenToPlayer * screenCorrection - localOrigin;
	
	    _thoughtBubble.Position += localCorrection;
	}
	
	private async Task<bool> WaitForSequenceAsync(double seconds, int sequenceId)
	{
		await ToSignal(
			GetTree().CreateTimer(seconds),
			SceneTreeTimer.SignalName.Timeout
		);

		return
			GodotObject.IsInstanceValid(this) &&
			IsInsideTree() &&
			sequenceId == _thoughtSequenceId;
	}

	private void UpdateFacingDirection(Vector2 direction)
	{
		if (Mathf.Abs(direction.X) < 0.01f)
		{
			return;
		}

		_sprite.FlipH = direction.X > 0.0f;
	}

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
	
	private void FinishMovement()
	{
		if (!_hasTarget)
		{
			return;
		}

		_hasTarget = false;
		Velocity = Vector2.Zero;

		PlayAnimation("idle");

		EmitSignal(SignalName.DestinationReached);
	}
	
	public void ShowThought(string text, double duration = 2.5)
	{
		_thoughtSequenceId++;

		ShowThoughtInternal(text, duration);
	}

	private void ShowThoughtInternal(string text, double duration)
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
		
		
		Callable
			.From(ClampThoughtBubbleToViewport)
			.CallDeferred();
		
		Callable.From(() =>
		{
			Rect2 viewport =
				GetViewport().GetVisibleRect();

			GD.Print(
				$"Bubble visible: {_thoughtBubble.Visible}, " +
				$"in tree: {_thoughtBubble.IsVisibleInTree()}, " +
				$"global position: {_thoughtBubble.GlobalPosition}, " +
				$"local position: {_thoughtBubble.Position}, " +
				$"size: {_thoughtBubble.Size}, " +
				$"viewport: {viewport}, " +
				$"perspective: {_currentPerspectiveScale}"
			);
		}).CallDeferred();
		
	}

	private void HideThought() => _thoughtBubble.Hide();
	
	private void UpdatePerspectiveScale()
	{
		if (Mathf.IsEqualApprox(FarY, NearY))
		{
			return;
		}
		float weight = Mathf.InverseLerp(
			FarY,
			NearY,
			GlobalPosition.Y
		);

		weight = Mathf.Clamp(weight, 0.0f, 1.0f);

		float perspectiveScale = Mathf.Lerp(
			FarScale,
			NearScale,
			weight
		);

		_currentPerspectiveScale = perspectiveScale;
		_visuals.Scale = _baseVisualScale * _currentPerspectiveScale;
		
		_thoughtBubble.Position = new Vector2(
			_baseThoughtBubblePosition.X,
			_baseThoughtBubblePosition.Y * perspectiveScale
		);
		
		if (_thoughtBubble.Visible)
		{
			ClampThoughtBubbleToViewport();
		}
	}
}
