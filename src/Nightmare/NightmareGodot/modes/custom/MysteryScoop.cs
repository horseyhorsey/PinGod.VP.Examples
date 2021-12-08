using Godot;

public class MysteryScoop : Control
{
	#region Fields
	private float _musicTime;
	string[] _mysteryChoices = new string[] {"100k", "Hold Bonus", "1 Million", "3 Million", "10 Million",
								"Return Lanes Lit", "Extra Ball", "Catch Up", "Jackpot" };

	private BallStackPinball ballStack;
    private Game game;

    /// <summary>
    /// Used for tunnel
    /// </summary>
    bool destroyBall = false;

	private AudioStreamPlayer kickersound;
	private PinGodGame pingod;
	private NightmarePlayer player;
	#endregion

	public override void _EnterTree()
	{
		pingod = GetNode("/root/PinGodGame") as PinGodGame;
		kickersound = GetNode("KickerSound") as AudioStreamPlayer;
		ballStack = GetNode("BallStackPinball") as BallStackPinball;
		game = GetParent().GetParent() as Game;
	}	
	/// <summary>
	/// Locks balls and mystery awards. Increases jackpots
	/// </summary>
	private void _on_BallStackPinball_SwitchActive()
	{
		pingod.LogInfo("mystery switch active, 5k");
		pingod.AddPoints(NightmareConstants.MED_SCORE/2);
		pingod.AddBonus(NightmareConstants.SMALL_SCORE);

		//pause music
		_musicTime = pingod.StopMusic();

		if (player.BallLockEnabled)
		{
			if (!player.LeftLock)
			{
				player.LeftLock = true;
				//todo: shoot the ball
				destroyBall = true;
				if (player.LeftLock && player.RightLock)
				{
					player.SuperJackpotLit = true;
					pingod.LogInfo("scoop: left lock on, super jackpot lit");
				}
				else
				{
					pingod.LogInfo("scoop: left lock on");
				}
			}			

			ballStack.Start();
		}
		else if (player.MysterySpinLit)
		{
			//todo: add mode?
			player.MysterySpinLit = false;
			player.SpinTimesPlayed++;
			//title : Mystery spin

			//todo: spin for 6.5, 4.5 in remix, show awarded for 1.5

			//todo: timer for showing choices
			var randomindex = GetRandomAward();
			var awardTitle = _mysteryChoices[randomindex];			
			var delay = AwardMystery(randomindex);
			pingod.LogInfo("mystery awarded, kick delay: ", awardTitle, " - ", delay);
			ballStack.Start(delay);
		}
		else
		{			
			player.JackpotValue += NightmareConstants.LARGE_SCORE;
			pingod.LogInfo("scoop: advancing jackpot..", player.JackpotValue);			
			ballStack.Start();
		}		
	}
	/// <summary>
	/// Saucer inactive play sound update lamps
	/// </summary>
	private void _on_BallStackPinball_SwitchInActive()
	{
		kickersound.Play();
		UpdateLamps();
	}
	/// <summary>
	/// Exit coil or tunnel done in simulator
	/// </summary>
	private void _on_BallStackPinball_timeout()
	{
		if(!destroyBall) pingod.SolenoidPulse("saucer_left");
		else pingod.SolenoidPulse("saucer_top_left_tunnel");
	}
	/// <summary>
	/// Awards mystery based from the <see cref="_mysteryChoices"/>
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	float AwardMystery(int index)
	{
		player.LanesLit = true;		
		switch (index)
		{
			case 0: //100k		
				pingod.AddPoints(NightmareConstants.LARGE_SCORE * 4);
				pingod.PlayMusic("mus_100k");
				return 1.0f;
			case 1:
				player.BonusHeld = true;
				return 1.5f;
			case 2:
				pingod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE);
				pingod.PlayMusic("mus_spinmillion");
				return 2.5f;
			case 3:
				pingod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE*3);
				pingod.PlayMusic("mus_spinmillion");
				return 2.5f;
			case 4:
				pingod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE * 10);
				pingod.PlayMusic("mus_spinmillion");
				return 2.5f;
			case 5:
				player.LanePanicLit = true;
				player.LaneExtraBallLit = true;
				return 2.5f;
			case 6:
				player.ExtraBalls++;
				pingod.PlayMusic("mus_extraball");
				return 4.5f;
			case 7:
				var doubleScore = player.Points * 2;
				pingod.AddPoints((int)doubleScore);
				//todo: double score display
				return 2.5f;
			case 8:
				pingod.PlayMusic("mus_jackpot");
				pingod.AddPoints(player.JackpotValue);
				return 2.0f;
			default:
				return 1.0f;
		}
	}
	/// <summary>
	/// Gets a random index from the choices
	/// </summary>
	/// <returns></returns>
	int GetRandomAward()
	{
		var r = new System.Random();
		return r.Next(0, _mysteryChoices.Length-1);
	}
	/// <summary>
	/// Reset player variables
	/// </summary>
	void OnBallStarted() 
	{
		player = game.GetPlayer();
		player.MysterySpinLit = false;
		player.LeftLock = false;
		player.LanePanicLit = false;
		player.LaneExtraBallLit = false;
		UpdateLamps();

		pingod.LogInfo($"ball start: mystery scoop");
	}
	void UpdateLamps() 
	{
		if (player.MysterySpinLit) pingod.SetLampState("spin_left", 1);
		else pingod.SetLampState("spin_left", 0);

		if (player.LeftLock) pingod.SetLampState("lock_left", 1);
		else pingod.SetLampState("lock_left", 0);

		if (player.LanePanicLit) pingod.SetLampState("panic", 1);
		else pingod.SetLampState("panic", 0);

		if (player.LaneExtraBallLit) pingod.SetLampState("xb_right", 1);
		else pingod.SetLampState("xb_right", 0);
	}
}
