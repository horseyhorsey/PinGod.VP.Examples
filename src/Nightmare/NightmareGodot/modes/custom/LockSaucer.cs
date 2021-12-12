using Godot;

/// <summary>
/// Middle lock saucer. Awards can be multiple and then displayed in order delayed
/// </summary>
public class LockSaucer : Control
{
	private BallStackPinball ballStack;
	private PinGodGame pinGod;
    private Game game;
    private NightmarePlayer player;

	public override void _EnterTree()
	{
		ballStack = GetNode("BallStackPinball") as BallStackPinball;
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		game = GetParent().GetParent() as Game;
	}
	private void _on_BallStackPinball_SwitchActive()
	{
		pinGod.LogInfo("lock saucer active");
		pinGod.SolenoidPulse("flasher_top_left");

		if (player.ScoreBonusLit)
		{
			var bonus = player.Bonus;
			pinGod.AddPoints((int)bonus);
			//todo: show scoring bonus
			pinGod.LogInfo("scoring bonus");
		}
		if (player.SuperJackpotLit)
		{
			player.SuperJackpotLit = false;
			var superTotal = player.JackpotValue * 2;
			pinGod.LogInfo("awarding super jackpot, ", superTotal);
			pinGod.AddPoints(superTotal);

			//todo: add superjackpot layers
			player.JackpotValue = 5000000;

			if (player.BallLockEnabled)
			{
				if (player.LeftLock || player.RightLock)
					DisableBallLocks();
			}
		}
		if (player.JackpotLit) 
		{
			//todo:
		}

		//advance ball locks
		if (player.BallsLocked < 3 && !player.BallLockEnabled)
		{
			player.BallsLocked++;
			pinGod.LogInfo("adding ball to lock,", player.BallsLocked);
			switch (player.BallsLocked)
			{
				case 1:
				case 2:
					break;
				case 3:
					player.BallLockEnabled = true;
					player.JackpotLit = true;
					break;
				default:
					break;
			}
		}

		if (player.ExtraBallLit)
		{
			pinGod.LogInfo("awarding extra ball");
			player.ExtraBallLit = false;
		}

		UpdateLamps();
		//start a timer to exit the saucer
		ballStack.Start();
	}
	private void _on_BallStackPinball_SwitchInActive()
	{
		// Replace with function body.
	}
	private void _on_BallStackPinball_timeout()
	{
		ballStack.SolenoidPulse();
		pinGod.PlaySfx("snd_kicker");
		if (player.SuperJackpotLit)
		{
			//todo: disable super jackpot after a timer 4 seconds
		}

		UpdateLamps();
	}
	/// <summary>
	/// Disables locks and lights super jackpot
	/// </summary>
	private void DisableBallLocks()
	{
		player.BallLockEnabled = false;
		player.RightLock = false;
		player.LeftLock = false;
		player.BallsLocked = 0;
		player.SuperJackpotLit = true;
	}
	void DisplayScoreBonus(long bonus)
	{

	}
	private void OnBallStarted()
	{
		player = ((NightmarePinGodGame)pinGod).GetPlayer();
		player.BallLockEnabled = false;
		player.RightLock = false;
		player.LeftLock = false;
		player.SuperJackpotLit = false;
		player.BallsLocked = 0;        
	}
	/// <summary>
	/// Updates lamps pointing to this saucer
	/// </summary>
	private void UpdateLamps()
	{
		if (player.ScoreBonusLit) pinGod.SetLampState("top_score_b", 2);
		else pinGod.SetLampState("top_score_b", 0);

		if (player.SuperJackpotLit) pinGod.SetLampState("top_super", 2);
		else pinGod.SetLampState("top_super", 0);

		if (player.JackpotLit) pinGod.SetLampState("top_jackpot", 2);
		else pinGod.SetLampState("top_jackpot", 0);

		if (player.ExtraBallLit) pinGod.SetLampState("top_xtraball", 2);
		else pinGod.SetLampState("top_xtraball", 0);

		if (player.BallsLocked == 1)
			pinGod.SetLampState("top_init", 0);
		else if (player.BallsLocked == 2)
			pinGod.SetLampState("top_init", 1);
		else if (player.BallsLocked == 3)
			pinGod.SetLampState("top_init", 0);
	}
}
