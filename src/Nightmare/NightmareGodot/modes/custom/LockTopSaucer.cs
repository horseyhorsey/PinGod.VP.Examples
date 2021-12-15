using Godot;

/// <summary>
/// Saucer Top right of table above lanes
/// </summary>
public class LockTopSaucer : Control
{
	bool destroy_ball = false;
	private NightmarePlayer _player;
	private BallStackPinball ballStack;    
	private Game game;
	private PinGodGame pinGod;

	public override void _EnterTree()
	{
		ballStack = GetNode("BallStackPinball") as BallStackPinball;
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		game = GetParent().GetParent() as Game;
	}
	private void _on_BallStackPinball_SwitchActive()
	{
		//pinGod.AddPoints(NightmareConstants.MED_SCORE);
		//pinGod.AddBonus(NightmareConstants.SMALL_SCORE);

		var music = "";
		var p = _player;
		if (p.RightLock)
		{
			ballStack.Start(0.2f);
		}
		else
		{
			if (p.BallLockEnabled)
			{
				p.RightLock = true;
				game.OnDisplayMessage("SHOOT THE\nBALL");
				music = "mus_ballready";
				//kick ball on timer, in shooter lane
				destroy_ball = true;

				ballStack.Start(2);

				if (p.LeftLock)
					p.SuperJackpotLit = true;
			}
			else if (p.DoubleBonus)
			{
				p.DoubleBonus = false;
				p.Bonus = p.Bonus * 2;
				ballStack.Start(2);
				game.OnDisplayMessage($"DOUBLING BONUS\n{_player.Bonus.ToScoreString()}");
			}
			else
			{
				isRaisingJackpot = true;
				ballStack.Start(AdvanceAndAwardCrossStack());
			}

			if (!string.IsNullOrWhiteSpace(music))
			{
				if(game != null)
				{
					game.PlayThenResumeMusic(music, 2f);
				}
			}
			
			pinGod.UpdateLamps(game.GetTree());
		}		
	}

	bool isRaisingJackpot = false;
	private void _on_BallStackPinball_SwitchInActive()
	{

	}
	private void _on_BallStackPinball_timeout()
	{
		//destroying ball should create a ball in shooter lane in the simulator
		if (destroy_ball)
		{
			pinGod.SolenoidPulse("saucer_top_right_tunnel");
		}
		else
		{
			if (isRaisingJackpot)
			{
				isRaisingJackpot = false;
				_player.JackpotValue += NightmareConstants.SCORE_100K;
				game.OnDisplayMessage($"RAISING JACKPOT\n{_player.JackpotValue.ToScoreString()}", 3.1f);
				game.PlayThenResumeMusic("mus_raisingjackpotorgan", 3.1f);
				ballStack.Start(1.5f);
				return;
			}

			pinGod.SolenoidPulse(ballStack._coil);
		}
	}
	private float AdvanceAndAwardCrossStack()
	{
		float delay = 0;
		var msg = "awarding cross stack, ";
		var crossStack = _player.CrossStack;
		if (crossStack[0] == 2)
		{
			if (_player.RomanValue < 10)
			{
				_player.RomanValue+=2;
				pinGod.AddBonus(NightmareConstants.SMALL_SCORE / 2 * 2);
				game.OnDisplayMessage("MIDNIGHT GETS\nCLOSER", 2f);
				delay = 2f;
			}
			crossStack[0] = 1;
			msg += "advanced roman";
			game.PlayThenResumeMusic("mus_extrahour", 2f);
		}
		else if (crossStack[1] == 2)
		{
			var bonus = _player.Bonus;
			pinGod.AddBonus((int)bonus);
			crossStack[1] = 1;
			msg += "awarded bonus to player: " + bonus;
			pinGod.AddPoints((int)bonus);
			pinGod.LogInfo(msg);
			game.StartBonusDisplay();
			delay = 1f;
		}
		else if (crossStack[2] == 2)
		{
			_player.BonusHeld = true;
			crossStack[2] = 1;
			msg += "bonus held";
			delay = 1f;
		}
		else if (crossStack[3] == 2)
		{
			_player.ExtraBallLit = true;
			crossStack[3] = 1;
			msg += "extra ball lit";
			delay = 2f;
		}
		else if (crossStack[4] == 2)
		{
			pinGod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE);
			crossStack[4] = 1;
			msg += "1 million";
			delay = 2f;
		}

		pinGod.AddBonus(NightmareConstants.SMALL_SCORE);
		pinGod.LogInfo(msg);
		return delay;
	}
	private void OnBallStarted()
	{
		_player = ((NightmarePinGodGame)pinGod).GetPlayer();
		_player.BallsLocked = 0;
		_player.BallLockEnabled = false;
	}
}
