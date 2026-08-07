using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Player : CharacterBody2D
{
	[ExportGroup("Movement")]
	// Controls how fast the player moves in pixels per second.
	[Export]
	public float MoveSpeed { get; set; } = 220.0f;

	// Defines how close the player must get to the target before stopping.
	[Export]
	public float StopDistance { get; set; } = 4.0f;
	
	[ExportGroup("Perspective")]
	// The Y position that represents the farthest walkable point in the scene.
	[Export]
	public float FarY { get; set; } = 360.0f;

	// The Y position that represents the closest walkable point in the scene.
	[Export]
	public float NearY { get; set; } = 630.0f;

	// Player scale used when the player is near the far edge of the scene.
	[Export]
	public float FarScale { get; set; } = 0.65f;

	// Player scale used when the player is near the front edge of the scene.
	[Export]
	public float NearScale { get; set; } = 1.05f;
	
	[ExportGroup("Thought Bubble")]
	// Minimum distance kept between the thought bubble and the screen edges.
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

	private Panel _thoughtBubble = null!;
	private Label _thoughtLabel = null!;
	private Timer _thoughtTimer = null!;

	public override void _Ready()
	{
		_visuals = GetNode<Node2D>("Visuals");
		_sprite = GetNode<AnimatedSprite2D>("Visuals/AnimatedSprite2D");
		_navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		_thoughtBubble = GetNode<Panel>("ThoughtBubble");
		_thoughtLabel = GetNode<Label>("ThoughtBubble/ThoughtLabel");
		_thoughtTimer = GetNode<Timer>("ThoughtTimer");
		


		_baseVisualScale = _visuals.Scale;
		_baseThoughtBubblePosition = _thoughtBubble.Position;

		// Floating mode is suitable for top-down and point-and-click movement
		// because it does not apply platform-style floor and wall behavior.
		MotionMode = MotionModeEnum.Floating;

		// Sets the player's initial horizontal facing direction.
		_sprite.FlipH = true;

		// Controls how closely the navigation agent follows intermediate path points.
		_navigationAgent.PathDesiredDistance = 5.0f;

		// Controls how close the agent must get to its final destination.
		_navigationAgent.TargetDesiredDistance = StopDistance;

		// Debug flag to have navigation points on runtime
		_navigationAgent.DebugEnabled = false;
		
		// Store the scene-defined values before applying runtime adjustments.
		_baseVisualScale = _visuals.Scale;
		_baseThoughtBubblePosition = _thoughtBubble.Position;
		
		UpdatePerspectiveScale();

		_thoughtTimer.Timeout += HideThought;
		_thoughtBubble.Hide();

		PlayAnimation("idle");
	}

	// Gives the navigation agent a new world-space destination and starts the player's movement process.
	public void MoveTo(Vector2 globalTarget)
	{
		_navigationAgent.TargetPosition = globalTarget;
		_hasTarget = true;
	}

	// Cancels the current movement and returns the player to the idle animation.
	public void Stop()
	{
		_hasTarget = false;
		Velocity = Vector2.Zero;

		PlayAnimation("idle");
	}

	// Updates navigation movement, animation, facing direction, and perspective scaling on every frame.
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
		
		// Get the next point along the path calculated by NavigationAgent2D.
		Vector2 nextPathPosition = _navigationAgent.GetNextPathPosition();

		// Calculate the direction and distance from the player to the next path point.
		Vector2 toNextPosition = nextPathPosition - GlobalPosition;
		float distance = toNextPosition.Length();

		if (distance <= 0.01f)
		{
			Velocity = Vector2.Zero;
			return;
		}

		// Convert the movement vector into a direction with a length of one.
		Vector2 direction = toNextPosition.Normalized();

		// Scale movement speed with perspective so distant movement appears slower and foreground movement appears faster.
		float perspectiveMoveSpeed = MoveSpeed * _currentPerspectiveScale;

		// Prevent the player from traveling farther than the remaining distance during the current frame.
		float currentSpeed = Mathf.Min(perspectiveMoveSpeed, distance / (float)delta);

		Velocity = direction * currentSpeed;

		// Updates where player face based on the direction of movement.
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

	// Keeps the thought bubble inside the visible screen area by correcting its position when it crosses a viewport edge.
	private void KeepThoughtBubbleInsideViewport()
	{
		if (!_thoughtBubble.Visible)
		{
			return;
		}

		Rect2 viewportRect = GetViewportRect();

		Vector2 bubbleTopLeft =
			GlobalPosition +
			_thoughtBubble.Position;

		Vector2 minimum = new(
			ThoughtBubbleScreenMargin,
			ThoughtBubbleScreenMargin
		);

		Vector2 maximum = new(
			viewportRect.Size.X -
			ThoughtBubbleScreenMargin -
			_thoughtBubble.Size.X,

			viewportRect.Size.Y -
			ThoughtBubbleScreenMargin -
			_thoughtBubble.Size.Y
		);

		maximum.X = Mathf.Max(
			minimum.X,
			maximum.X
		);

		maximum.Y = Mathf.Max(
			minimum.Y,
			maximum.Y
		);

		Vector2 clampedTopLeft = new(
			Mathf.Clamp(
				bubbleTopLeft.X,
				minimum.X,
				maximum.X
			),
			Mathf.Clamp(
				bubbleTopLeft.Y,
				minimum.Y,
				maximum.Y
			)
		);

		_thoughtBubble.Position +=
			clampedTopLeft -
			bubbleTopLeft;
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

		// Notify the scene that the player has arrived.
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
		
		
		Callable.From(KeepThoughtBubbleInsideViewport)
			.CallDeferred();

	}

	private void HideThought() => _thoughtBubble.Hide();
	
	private void UpdatePerspectiveScale()
	{
		if (Mathf.IsEqualApprox(FarY, NearY))
		{
			return;
		}
		float weight = Mathf.InverseLerp(FarY, NearY, GlobalPosition.Y);

		weight = Mathf.Clamp(weight, 0.0f, 1.0f);

		float perspectiveScale = Mathf.Lerp(FarScale, NearScale, weight);

		_currentPerspectiveScale = perspectiveScale;
		_visuals.Scale = _baseVisualScale * _currentPerspectiveScale;
		
		_thoughtBubble.Position = new Vector2(_baseThoughtBubblePosition.X, _baseThoughtBubblePosition.Y * perspectiveScale);
		
		if (_thoughtBubble.Visible)
		{
			KeepThoughtBubbleInsideViewport();
		}
	}
}
