using Godot;

public class ParallaxBackground : Godot.ParallaxBackground
{
	[Export] Vector2 _camera_velocity = new Vector2(0, 200);

	public override void _Process(float delta)
	{
		ScrollOffset = ScrollOffset + _camera_velocity * delta;
	}
}
