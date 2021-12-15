using Godot;

public class MidnightMode : Control
{
	[Export] float _mode_time_out = 30f;

	private PinGodGame pinGod;
	private string[] switches;
	private long _midnightScore = 0;
    private Game game;
    private float _timeLeft;
    private NightmarePlayer player;

    public Label Label { get; private set; }

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		switches = new string[] { "orbit_r", "ramp_l", "ramp_r" };		
		Label = GetNode("Label") as Label;
		pinGod.LogInfo("midnight started");
		game = GetParent().GetParent() as Game;
		_timeLeft = _mode_time_out;
	}

	public override void _Ready()
	{
		player = ((NightmarePinGodGame)pinGod).GetPlayer();

		game.resumeBgmTimer.Stop();
		game.PauseBgm(true);

		player.MidnightRunning = true;
		player.MidnightTimesPlayed++;

		pinGod.PlayMusic("mus_midnight");
		game.StartHurryUp(false);
	}

	public override void _Input(InputEvent @event)
	{
		if (pinGod.IsSwitch(switches, @event))
		{
			AwardMidnight();
		}
	}

	public override void _ExitTree()
	{
		pinGod.StopMusic();
		player.MidnightRunning = false;
		player.RomanValue = 0;
		game.PauseBgm(false);

		pinGod.DisableAllLamps();
		game.UpdateLamps();
		pinGod.SolenoidOn("vpcoil", 0);
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
		pinGod.LogInfo("midnight: ball drained");
		this.QueueFree();
	}

	private void AwardMidnight()
	{
		var score = NightmareConstants.EXTRA_LARGE_SCORE * 5;
		_midnightScore += score;
		pinGod.AddPoints(score);
		pinGod.AddBonus(NightmareConstants.SCORE_100K);
		pinGod.SolenoidOn("vpcoil", 1); //lampshow vp
		game.OnDisplayMessage("MIDNIGHT\n5 MILLION\nSHOT", 1.5f);
		if (!player.HurryUpRunning)
        {
			//todo: play sequence Midnight5Million
			//clear 2 seconds
		}
	}
}
