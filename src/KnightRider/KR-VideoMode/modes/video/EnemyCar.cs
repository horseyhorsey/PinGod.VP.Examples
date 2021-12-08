using Godot;

public class EnemyCar : Area2D
{
	[Export] int _speed = 5;
	[Signal] public delegate void PlayerCrashed();

	public override void _Process(float delta)
	{
		this.Position = new Vector2(Position.x, Position.y+_speed);
		if (Position.y > 1080f)
		{
			QueueFree();
		}
	}

	private void _on_EnemyCar_body_entered(object body)
	{		
		EmitSignal(nameof(PlayerCrashed));
	}
}
