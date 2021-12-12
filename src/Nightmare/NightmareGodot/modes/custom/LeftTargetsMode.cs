using Godot;

public class LeftTargetsMode : PinballTargetsControl
{
	private Game game;
	private NightmarePlayer player;
	private Timer _clearLayerTimer;

	public override void _EnterTree()
	{
		base._EnterTree();
		//get game to resume music
		game = GetParent().GetParent() as Game;

		//create timer to clear layer, hide
		_clearLayerTimer = new Timer() { Autostart = false, OneShot = true };
		_clearLayerTimer.Connect("timeout", this, "Clear");
		AddChild(_clearLayerTimer);

		//hide this mode, still process the switches
		Clear();
	}

	public void Clear() => this.Visible = false;

	public override void _Input(InputEvent @event)
	{
		if (!pinGod.GameInPlay) return; //disable when running scene on its own to test input
		if (pinGod.IsTilted) return;
		base._Input(@event);
	}

	public override bool CheckTargetsCompleted(int index)
	{
		pinGod.AddPoints(NightmareConstants.MED_SCORE);
		pinGod.AddBonus(NightmareConstants.SMALL_SCORE);
		pinGod.PlaySfx("snd_squirk");

		var completed = base.CheckTargetsCompleted(index);
		UpdateLamps();
		return completed;
	}

	public void OnBallStarted()
	{
		player = ((NightmarePinGodGame)pinGod).GetPlayer();
		player.LeftTargetsCompleted = 0;
		player.SaucerBonus = false;
		player.ExtraBallLit = false;
		player.SuperJackpotLit = false;
		ResetTargets();
	}

	public override void TargetsCompleted(bool reset = true)
	{
		base.TargetsCompleted(reset);
		_clearLayerTimer.Stop();

		player.LeftTargetsCompleted++;
		switch (player.LeftTargetsCompleted)
		{
			case 1:
				player.ScoreBonusLit = true;
				DisplayText("SCORE BONUS\nLIT", 2f);
				break;
			case 2:
				player.ExtraBallLit = true;
				DisplayText("EXTRA BALL\nLIT", 2f);
				break;
			case 3:
				player.SuperJackpotLit = true;
				player.LeftTargetsCompleted = 0;
				DisplayText("SUPER JACKPOT\nLIT", 2f);
				break;
			default:				
				break;
		}

		if (game != null)
		{
			game.PlayThenResumeMusic("mus_alltriangles", 0.95f);
			//run UpdateLamps in groups marked as Mode
			pinGod.UpdateLamps(game?.GetTree());
		}
		else
		{
			UpdateLamps();
		}
	}

	public override void UpdateLamps()
	{
		base.UpdateLamps();
	}

	private void DisplayText(string text, float delay = 0)
	{
		this.Visible = true;
		_clearLayerTimer.Start(delay);
	}
}
