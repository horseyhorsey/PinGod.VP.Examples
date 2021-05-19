using Godot;
using System;
using static Godot.GD;

/// <summary>
/// Example of Multiball
/// </summary>
public class Multiball : Control
{
	private PinGodGame pinGodGame;

	/// <summary>
	/// 4 ball multiplayer
	/// </summary>
	[Export] byte _num_of_balls = 4;

	[Export] byte _ball_save_time_seconds = 3;

	public override void _EnterTree()
	{
		pinGodGame = GetNode("/root/PinGodGame") as PinGodGame;
	}

	public override void _Ready()
	{
		Print("mball: _ready: secs/balls", _ball_save_time_seconds, _num_of_balls);
		pinGodGame.StartMultiBall(_num_of_balls, _ball_save_time_seconds);
	}

	internal void EndMultiball() => this.QueueFree();
}
