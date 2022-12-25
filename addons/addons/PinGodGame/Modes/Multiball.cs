using Godot;

/// <summary>
/// Multi-ball starts when this scene is added to the tree
/// </summary>
public class Multiball : Control
{
	private PinGodGame pinGod;

	/// <summary>
	/// 4 ball multi-ball
	/// </summary>
	[Export] byte _num_of_balls = 4;

	[Export] byte _ball_save_time_seconds = 3;

	/// <summary>
	/// Gets a reference for <see cref="pinGod"/>
	/// </summary>
	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
	}

	/// <summary>
	/// Starts multi-ball <see cref="PinGodGame.StartMultiBall(byte, byte, float)"/>
	/// </summary>
	public override void _Ready()
	{
		pinGod.LogDebug("mball: _ready: secs/balls", _ball_save_time_seconds, _num_of_balls);
		pinGod.StartMultiBall(_num_of_balls, _ball_save_time_seconds);
	}

    /// <summary>
    /// Signal emitted from trough <see cref="Trough._Input"/>
    /// </summary>
    internal void EndMultiball() => this.QueueFree();
}