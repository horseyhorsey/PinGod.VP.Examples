using Godot;

/// <summary>
/// Example of Multi-ball
/// </summary>
public class Multiball : Control
{
	private PinGodGame pinGod;

	/// <summary>
	/// 4 ball multi-ball
	/// </summary>
	[Export] byte _num_of_balls = 4;

	[Export] byte _ball_save_time_seconds = 3;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
	}

	public override void _Ready()
	{
		pinGod.LogDebug("mball: _ready: secs/balls", _ball_save_time_seconds, _num_of_balls);
		pinGod.StartMultiBall(_num_of_balls, _ball_save_time_seconds);
	}

	internal void EndMultiball() => this.QueueFree();
}