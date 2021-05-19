using Godot;
using System;
using static Godot.GD;

/// <summary>
/// Pinball machines run a ball search when no activity from switches when game is started. <para/>
/// These pulse specific coils, like a ball saucer to kick a ball
/// </summary>
public class BallSearch : Control
{
	/// <summary>
	/// The coil numbers to pulse when timer runs out.
	/// </summary>
	[Export] private byte[] _search_coils;

	[Export] private int _wait_time_secs;

	/// <summary>
	/// Taken from the default timer when ready and <see cref="_wait_time_secs"/> isn't set
	/// </summary>
	private float _ballSearchWaitTime;
	private Timer _timer;
	private Label _label;

	bool IsBallSearchRunning = false;

	public override void _Ready()
	{
		//timer to activate ball searches
		_timer = GetNode("Timer") as Timer;

		_ballSearchWaitTime = _wait_time_secs;

		_label = GetNode("Label") as Label;

		var pinGodGame = GetNode("/root/PinGodGame") as PinGodGame;
		pinGodGame?.Connect("BallSearchReset", this, "OnBallSearchReset");
		pinGodGame?.Connect("BallSearchStop", this, "OnBallSearchStop");
	}

	void OnBallSearchReset()
	{
		Print("ball_search: search_reset");
		_label.Visible = false;
		_timer.Start(_ballSearchWaitTime);
	}

	void OnBallSearchStop()
	{
		_label.Visible = false;
		_timer.Stop();
		Print("ball_search: search_stopped");
	}

	/// <summary>
	/// No switches have been used, show the ball search and run it.
	/// </summary>
	private void _on_Timer_timeout()
	{
		Print("ball_search: search_reset: timeout");
		if(_search_coils?.Length > 0)
		{
			_label.Visible = true;
			IsBallSearchRunning = true;

			for (int i = 0; i < _search_coils.Length; i++)
			{
				OscService.PulseCoilState(_search_coils[i]);
			}
		}
		else
		{
			Print("ball_search: no coils set to check");
		}
	}
}

