using Godot;

public class MysteryScoop : Control
{
	#region Fields
	private float _musicTime;
	string[] _mysteryChoices = new string[] {"100k", "Hold Bonus", "1 Million", "3 Million", "10 Million", "Hold Bonus",
								"Return Lanes Lit", "Extra Ball", "Catch Up", "Jackpot" };

	private BallStackPinball ballStack;
    private Game game;
    private Timer timer;
	float totalSecondsInMystery = 7.5f;

    /// <summary>
    /// Used for tunnel
    /// </summary>
    bool destroyBall = false;

	private AudioStreamPlayer kickersound;
	private PinGodGame pinGod;
	private NightmarePlayer player;
    private int _selectedAward;
    private string awardText;
    #endregion

    public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		kickersound = GetNode("KickerSound") as AudioStreamPlayer;
		ballStack = GetNode("BallStackPinball") as BallStackPinball;
		game = GetParent().GetParent() as Game;
		timer = GetNode<Timer>("Timer");

		totalSecondsInMystery = 7.5f;
	}	
	/// <summary>
	/// Locks balls and mystery awards. Increases jackpots
	/// </summary>
	private void _on_BallStackPinball_SwitchActive()
	{
		pinGod.LogInfo("mystery switch active, 10k");
		pinGod.AddPoints(NightmareConstants.MED_SCORE);
		pinGod.AddBonus(NightmareConstants.SMALL_SCORE);

		//pause music
		_musicTime = pinGod.StopMusic();

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
					pinGod.LogInfo("scoop: left lock on, super jackpot lit");
				}
				else
				{
					pinGod.LogInfo("scoop: left lock on");
				}
			}			

			ballStack.Start();
		}
		else if (player.MysterySpinLit)
		{
			//todo: add mode?
			player.MysterySpinLit = false;
			player.SpinTimesPlayed++;

			game.OnDisplayMessage("Mystery spin");
			pinGod.PlayMusic("mus_spinbonus");
			timer.Start();
		}
		else
		{			
			player.JackpotValue += NightmareConstants.LARGE_SCORE;
			pinGod.LogInfo("scoop: advancing jackpot..", player.JackpotValue);			
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
		if(!destroyBall) pinGod.SolenoidPulse("saucer_left");
		else pinGod.SolenoidPulse("saucer_top_left_tunnel");
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
				pinGod.AddPoints(NightmareConstants.LARGE_SCORE * 4);
				pinGod.PlayMusic("mus_100k");
				awardText = $"100K";
				return 1.0f;
			case 1:
			case 5:
				player.BonusHeld = true;
				awardText = $"BONUS HELD";
				return 1.5f;
			case 2:
				pinGod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE);
				pinGod.PlayMusic("mus_spinmillion");
				awardText = $"1\nMILLION";
				return 2.5f;
			case 3:
				pinGod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE*3);
				pinGod.PlayMusic("mus_spinmillion");
				awardText = $"3\nMILLION";
				return 2.5f;
			case 4:
				pinGod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE * 10);
				pinGod.PlayMusic("mus_spinmillion");
				awardText = $"10\nMILLION";
				return 2.5f;
			case 6:
				player.LanePanicLit = true;
				player.LaneExtraBallLit = true;
				return 2.5f;
			case 7:
				player.ExtraBalls++;
				pinGod.PlayMusic("mus_extraball");
				return 4.5f;
			case 8:
				var doubleScore = player.Points * 2;
				pinGod.AddPoints((int)doubleScore);
				awardText = $"CATCH UP!\n{doubleScore}";
				return 2.5f;
			case 9: //jackpot
				pinGod.PlayMusic("mus_jackpot");
				pinGod.AddPoints(player.JackpotValue);
				awardText = $"JACKPOT!\n{player.JackpotValue}";
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
		player = ((NightmarePinGodGame)pinGod).GetPlayer();
		player.MysterySpinLit = false;
		player.LeftLock = false;
		player.LanePanicLit = false;
		player.LaneExtraBallLit = false;
		UpdateLamps();

		pinGod.LogInfo($"ball start: mystery scoop");
	}
	void UpdateLamps() 
	{
		if (player.MysterySpinLit) pinGod.SetLampState("spin_left", 1);
		else pinGod.SetLampState("spin_left", 0);

		if (player.LeftLock) pinGod.SetLampState("lock_left", 1);
		else pinGod.SetLampState("lock_left", 0);

		if (player.LanePanicLit) pinGod.SetLampState("panic", 1);
		else pinGod.SetLampState("panic", 0);

		if (player.LaneExtraBallLit) pinGod.SetLampState("xb_right", 1);
		else pinGod.SetLampState("xb_right", 0);
	}

	private void Timer_timeout()
    {
		totalSecondsInMystery -= 1.5f;

		if(totalSecondsInMystery <= 0)
        {
			var delay = AwardMystery(_selectedAward);
			game.OnDisplayMessage(awardText);			
			ballStack.Start(delay);
		}
        else
        {
			_selectedAward = GetRandomAward();
			awardText = _mysteryChoices[_selectedAward];
		}
    }
}
