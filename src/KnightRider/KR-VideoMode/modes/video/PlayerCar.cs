using Godot;
using System;

public class PlayerCar : KinematicBody2D
{
	[Export] float _speed = 3.5f;

	public override void _Ready()
	{
		var shape = (RectangleShape2D)GetNode<CollisionShape2D>("CollisionShape2D").Shape;
	}

	public override void _PhysicsProcess(float delta)
	{
		var velocity = new Vector2();
		bool changed = false;

		if (Input.IsActionPressed("sw11"))
		{
			velocity.x += _speed;
			changed = true;
		}
		if (Input.IsActionPressed("sw9"))
		{
			velocity.x -= _speed;
			changed = true;
		}

		if (!changed) return;

		var collided = MoveAndCollide(velocity);
	}
}
