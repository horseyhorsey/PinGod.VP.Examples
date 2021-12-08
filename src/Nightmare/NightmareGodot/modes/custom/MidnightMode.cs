using Godot;
using System;

public class MidnightMode : Control
{
	[Export] float _mode_time_out = 30f;

	private PinGodGame pingod;
	private string[] switches;
	private long _midnightScore = 0;
    private Game game;
    private float _timeLeft;
    private NightmarePlayer player;

    public Label Label { get; private set; }

	public override void _EnterTree()
	{
		pingod = GetNode("/root/PinGodGame") as PinGodGame;
		switches = new string[] { "orbit_r", "ramp_l", "ramp_r" };		
		Label = GetNode("Label") as Label;
		pingod.LogInfo("midnight started");
		game = GetParent().GetParent() as Game;
		_timeLeft = _mode_time_out;
	}

	public override void _Ready()
	{
		player = game.GetPlayer();
		player.MidnightRunning = true;
		player.MidnightTimesPlayed++;
	}

	public override void _Input(InputEvent @event)
	{
		if (pingod.IsSwitch(switches, @event))
		{
			AwardMidnight();
		}
	}

	public override void _ExitTree()
	{
		player.MidnightRunning = false;
		player.RomanValue = 0;
	}

	private void _on_Timer_timeout()
	{
		_timeLeft -= 1.0f;
		if(_timeLeft < 0)
		{
			this.QueueFree();
		}
		else
		{
			Label.Text = $"MIDNIGHT {(int)_timeLeft}\n{_midnightScore.ToScoreString()}";
		}
	}

	public void OnBallDrained()
	{
		pingod.LogInfo("midnight: ball drained");
		this.QueueFree();
	}

	private void AwardMidnight()
	{
		var score = NightmareConstants.EXTRA_LARGE_SCORE * 5;
		_midnightScore += score;
		pingod.AddPoints(score);

        if (!player.HurryUpRunning)
        {
			//todo: play sequence Midnight5Million
			//clear 2 seconds
		}
	}
}
