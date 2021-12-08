using Godot;

/// <summary>
/// Middle lock saucer. Awards can be multiple and then displayed in order delayed
/// </summary>
public class LockSaucer : Control
{
	private BallStackPinball ballStack;
	private PinGodGame pingod;
    private Game game;
    private NightmarePlayer player;

	public override void _EnterTree()
	{
		ballStack = GetNode("BallStackPinball") as BallStackPinball;
		pingod = GetNode("/root/PinGodGame") as PinGodGame;
		game = GetParent().GetParent() as Game;
	}
	private void _on_BallStackPinball_SwitchActive()
	{
		pingod.LogInfo("lock saucer active");
		pingod.SolenoidPulse("flasher_top_left");

		if (player.ScoreBonusLit)
		{
			var bonus = player.Bonus;
			pingod.AddPoints((int)bonus);
			//todo: show scoring bonus
			pingod.LogInfo("scoring bonus");
		}
		if (player.SuperJackpotLit)
		{
			player.SuperJackpotLit = false;
			var superTotal = player.JackpotValue * 2;
			pingod.LogInfo("awarding super jackpot, ", superTotal);
			pingod.AddPoints(superTotal);

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
			pingod.LogInfo("adding ball to lock,", player.BallsLocked);
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
			pingod.LogInfo("awarding extra ball");
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
		pingod.SolenoidPulse(ballStack._coil);
		pingod.PlaySfx("snd_kicker");
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
		player = game.GetPlayer();
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
		if (player.ScoreBonusLit) pingod.SetLampState("top_score_b", 2);
		else pingod.SetLampState("top_score_b", 0);

		if (player.SuperJackpotLit) pingod.SetLampState("top_super", 2);
		else pingod.SetLampState("top_super", 0);

		if (player.JackpotLit) pingod.SetLampState("top_jackpot", 2);
		else pingod.SetLampState("top_jackpot", 0);

		if (player.ExtraBallLit) pingod.SetLampState("top_xtraball", 2);
		else pingod.SetLampState("top_xtraball", 0);

		if (player.BallsLocked == 1)
			pingod.SetLampState("top_init", 0);
		else if (player.BallsLocked == 2)
			pingod.SetLampState("top_init", 1);
		else if (player.BallsLocked == 3)
			pingod.SetLampState("top_init", 0);
	}
}
