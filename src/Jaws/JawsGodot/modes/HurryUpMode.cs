using Godot;

public class HurryUpMode : Control
{
	[Export] long _hurry_up_award = 10000000;
	[Export] long _hurry_up_score_decrement = 500000;

	private JawsPinGodGame pinGod;
	private Timer timer;
	private BlinkingLabel awardLabel;
	private Game game;
	private long _current_award;
	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as JawsPinGodGame;
		timer = GetNode("Timer") as Timer;
		awardLabel = GetNode("CenterContainer/VBoxContainer/BlinkingLabel") as BlinkingLabel;
		game = GetParent().GetParent() as Game;
	}
	public override void _Ready()
	{
		//StartHurryUp(); //TESTING
	}

	public bool IsRunning() => !timer.IsStopped();

	public long AwardHurryUp()
	{
		timer.Stop();
		awardLabel.Text = "AWARDED\r\n" + _current_award.ToScoreString();
		return _current_award;
	}

	public void StartHurryUp()
	{
		GD.Print("hurry up: starting");
		pinGod.EnableJawsToy(false);
		Visible = true;
		_current_award = _hurry_up_award;
		timer.Start();
		pinGod.PlaySfx("shoot");
		UpdateText();
	}

	private void UpdateText() => awardLabel.Text = _current_award.ToScoreString();

	public long StopHurryUp()
	{
		Visible = false;
		timer.Stop();
		GD.Print("hurry up: stopped");
		game.PlayMusicForMode();
		return _current_award;
	}
	private void _on_Timer_timeout()
	{
		if (_current_award <= 0)
		{			
			GD.Print("hurry up: timed out");
			StopHurryUp();
		}
		else
		{
			_current_award -= _hurry_up_score_decrement;
			UpdateText();
		}		
	}
}
