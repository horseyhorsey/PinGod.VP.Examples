using Godot;
using static Godot.GD;

/// <summary>
/// Ball saver display. Uses <see cref="Trough.BallSaved"/> to show and timeout
/// </summary>
public class BallSave : Control
{
	private Timer timer;

	/// <summary>
	/// Hide initially and connect to ball saved signals from Trough <see cref="Trough.BallSaved"/>
	/// </summary>
	public override void _Ready()
	{
		Visible = false;
		GetNode("/root/Trough").Connect("BallSaved", this, "OnBallSaved");
		timer = GetNode("Timer") as Timer;
	}

	/// <summary>
	/// Show the ball saved scene layers
	/// </summary>
	void OnBallSaved()
	{
		Print("ballsave: ball_saved");
		this.Visible = true;

		if (!timer.IsStopped())
			timer.Stop();

		timer.Start();
	}

	void _on_Timer_timeout() => this.Visible = false;
}
