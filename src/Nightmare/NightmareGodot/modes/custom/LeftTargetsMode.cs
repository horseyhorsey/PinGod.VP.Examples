using Godot;

public class LeftTargetsMode : PinGodGameMode
{
	private Game game;
	private NightmarePlayer player;

	public override void _EnterTree()
	{
		base._EnterTree();

		//get game to resume music
		game = GetParent().GetParent() as Game;
	}

	public override void _Input(InputEvent @event)
	{
		if (!pinGod.GameInPlay) return; //disable when running scene on its own to test input
		if (pinGod.IsTilted) return;
		base._Input(@event);
	}

	protected override void OnBallStarted()
	{
		player = ((NightmarePinGodGame)pinGod).GetPlayer();
		player.LeftTargetsCompleted = 0;
		player.SaucerBonus = false;
		player.ExtraBallLit = false;
		player.SuperJackpotLit = false;
		//ResetTargets();
	}

    protected override void UpdateLamps()
    {
        base.UpdateLamps();
    }

    void _on_PinballTargetsBank_OnTargetActivated(string swName, bool complete)
    {
		pinGod.AddPoints(NightmareConstants.MED_SCORE);
		pinGod.AddBonus(NightmareConstants.SMALL_SCORE);
		pinGod.PlaySfx("snd_squirk");
		UpdateLamps();
	}

	void _on_PinballTargetsBank_OnTargetsCompleted()
    {
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
			pinGod.SolenoidOn("vpcoil", 7); // lampshow vp 
			
			//run UpdateLamps in groups marked as Mode
			pinGod.UpdateLamps(game?.GetTree());
		}
		else
		{
			UpdateLamps();
		}
	}	
}
