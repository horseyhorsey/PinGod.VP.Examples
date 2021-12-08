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
	private PinGodGame pingod;

	public override void _EnterTree()
	{
		ballStack = GetNode("BallStackPinball") as BallStackPinball;
		pingod = GetNode("/root/PinGodGame") as PinGodGame;
		game = GetParent().GetParent() as Game;
	}
	private void _on_BallStackPinball_SwitchActive()
	{
		pingod.AddPoints(NightmareConstants.MED_SCORE);
		pingod.AddBonus(NightmareConstants.SMALL_SCORE);

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
				//todo: scene : shoot_the_ball
				music = "mus_ballready";
				//kick ball on timer, in shooter lane
				destroy_ball = true;

				if (p.LeftLock)
					p.SuperJackpotLit = true;
			}
			else if (p.DoubleBonus)
			{
				//todo: music?
				p.DoubleBonus = false;
				p.Bonus = p.Bonus * 2;
				ballStack.Start(2);
			}
			else
			{
				music = "mus_raisingjackpot";
				p.JackpotValue += NightmareConstants.LARGE_SCORE;
				AdvanceAndAwardCrossStack();
				ballStack.Start(2);
			}

			if (!string.IsNullOrWhiteSpace(music))
			{
				if(game != null)
				{
					game.PlayThenResumeMusic(music, 2f);
				}
			}
			
			pingod.UpdateLamps(game.GetTree());
		}		
	}
	private void _on_BallStackPinball_SwitchInActive()
	{
		// Replace with function body.
	}
	private void _on_BallStackPinball_timeout()
	{
		//destroying ball should create a ball in shooter lane in the simulator
		if (destroy_ball)
		{
			pingod.SolenoidPulse("saucer_top_right_tunnel");
		}
		else
		{
			pingod.SolenoidPulse(ballStack._coil);
		}
	}
	private void AdvanceAndAwardCrossStack()
	{
		var msg = "awarding cross stack, ";
		var crossStack = _player.CrossStack;
		if (crossStack[0] == 2)
		{
			if (_player.RomanValue < 10)
			{
				_player.RomanValue+=2;
				pingod.AddBonus(NightmareConstants.SMALL_SCORE / 2 * 2);
			}
			crossStack[0] = 1;
			msg += "advanced roman";
			//todo: seq: extra_hour
		}
		else if (crossStack[1] == 2)
		{
			var bonus = _player.Bonus;
			pingod.AddBonus((int)bonus);
			crossStack[1] = 1;
			msg += "awarded bonus to player: " + bonus;
			//todo: score bonus countdown
		}
		else if (crossStack[2] == 2)
		{
			_player.BonusHeld = true;
			crossStack[2] = 1;
			msg += "bonus held";
		}
		else if (crossStack[3] == 2)
		{
			_player.ExtraBallLit = true;
			crossStack[3] = 1;
			msg += "extra ball lit";
		}
		else if (crossStack[4] == 2)
		{
			pingod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE);
			crossStack[4] = 1;
			msg += "1 million";
		}

		pingod.LogInfo(msg);
	}
	private void OnBallStarted()
	{
		_player = game.GetPlayer();
		_player.BallsLocked = 0;
		_player.BallLockEnabled = false;
	}
}
