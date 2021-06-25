using Godot;
using static Godot.GD;

/// <summary>
/// Ball saver display. Uses <see cref="Trough.BallSaved"/> to show and timeout
/// </summary>
public class BallSave : Control
{
	private PinGodGame pinGod;
	private Timer timer;
	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		pinGod.Connect(nameof(PinGodGameBase.BallSaved), this, "OnBallSaved");
		timer = GetNode("Timer") as Timer;
	}

	/// <summary>
	/// Hide initially and connect to ball saved signals from Trough <see cref="Trough.BallSaved"/>
	/// </summary>
	public override void _Ready()
	{
		Visible = false;
	}

	void _on_Timer_timeout() => this.Visible = false;

	/// <summary>
	/// Show the ball saved scene layers
	/// </summary>
	void OnBallSaved()
	{
		if (!pinGod.IsMultiballRunning)
		{
			pinGod.LogDebug("ballsave: ball_saved");
			this.Visible = true;

			if (!timer.IsStopped())
				timer.Stop();

			timer.Start();
		}
		else
		{
			pinGod.LogDebug("skipping save display in multiball");
		}
	}
}
