using Godot;
using System.Threading.Tasks;

public partial class PoliceAlien : CharacterBody2D
{
	public static readonly StringName WriteTicketAnimation = "write_ticket";
	public static readonly StringName PlaceTicketAnimation = "place_ticket";
	public static readonly StringName WalkAnimation = "walk";

	private AnimatedSprite2D _sprite = null!;
	private CollisionShape2D _collisionShape = null!;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		_sprite.Stop();
		SetPresent(false);
	}

	public void SetPresent(bool isPresent)
	{
		Visible = isPresent;
		_collisionShape.Disabled = !isPresent;
	}

	public async Task PlayOnceAsync(StringName animation)
	{
		_sprite.Stop();
		_sprite.Animation = animation;
		_sprite.Frame = 0;
		_sprite.Play();

		await ToSignal(_sprite, AnimatedSprite2D.SignalName.AnimationFinished);
	}

	public async Task WalkToAsync(Vector2 destination, float speed = 210.0f)
	{
		float distance = GlobalPosition.DistanceTo(destination);

		if (distance <= 1.0f)
		{
			GlobalPosition = destination;
			return;
		}

		_sprite.FlipH = destination.X > GlobalPosition.X;
		_sprite.Play(WalkAnimation);

		Tween tween = CreateTween();
		tween.TweenProperty(this, "global_position", destination, distance / speed);

		await ToSignal(tween, Tween.SignalName.Finished);
		_sprite.Stop();
	}
}
