using Godot;

public class LeftTargetsMode : PinballTargetsControl
{
	private Game game;
	private NightmarePlayer player;

	public override void _EnterTree()
	{
		base._EnterTree();

		//get game to resume music
		game = GetParent().GetParent() as Game;

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

		player.LeftTargetsCompleted++;
		switch (player.LeftTargetsCompleted)
		{
			case 1:
				player.ScoreBonusLit = true;
				game.OnDisplayMessage("SCORE BONUS\nIS LIT", 2f);
				break;
			case 2:
				player.ExtraBallLit = true;
				game.OnDisplayMessage("EXTRA BALL\nIS LIT", 2f);
				break;
			case 3:
				player.SuperJackpotLit = true;
				player.LeftTargetsCompleted = 0;
				game.OnDisplayMessage("SUPER JACKPOT\nIS LIT", 2f);
				break;
			default:				
				break;
		}

		pinGod.LogDebug("left targets completed ", player.LeftTargetsCompleted);

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
}
