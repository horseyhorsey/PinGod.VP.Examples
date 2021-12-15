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
		if(pinGod.GameInPlay && !pinGod.IsTilted)
        {
			pinGod.LogInfo("mystery switch active, 10k");
			pinGod.AddPoints(NightmareConstants.MED_SCORE);
			pinGod.AddBonus(NightmareConstants.SMALL_SCORE);

			//pause music
			_musicTime = pinGod.StopMusic();

			if (player.BallLockEnabled && !player.LeftLock)
			{
				player.LeftLock = true;
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

				ballStack.Start();
			}
			else if (player.MysterySpinLit)
			{
				player.MysterySpinLit = false;
				player.SpinTimesPlayed++;

				game.OnDisplayMessage("Mystery spin", 6.5f);
				game.PlayThenResumeMusic("mus_spinbonus", 8f);

				//play spin lampshow in VP `SpecialSolCallback`
				pinGod.SolenoidOn("vpcoil", 2);
				timer.Start(6.5f);
			}
			else
			{
				player.JackpotValue += NightmareConstants.SCORE_100K;
				pinGod.LogInfo("scoop: advancing jackpot..", player.JackpotValue);
				game.OnDisplayMessage($"RAISING JACKPOT\n{player.JackpotValue.ToScoreString()}");
				game.PlayThenResumeMusic("mus_raisingjackpot", 2f);
				ballStack.Start();
			}
		}
        else
        {
			ballStack.Start();
		}				
	}
	/// <summary>
	/// Saucer inactive play sound update lamps
	/// </summary>
	private void _on_BallStackPinball_SwitchInActive()
	{
		kickersound.Play();
		game.UpdateLamps();
	}
	/// <summary>
	/// Exit coil or tunnel done in simulator
	/// </summary>
	private void _on_BallStackPinball_timeout()
	{
		pinGod.LogDebug("mystery scoop timed out");

		//disable lampshows VP script `SpecialSolCallback`
		pinGod.SolenoidOn("vpcoil", 0);

		if (!destroyBall)
		{
			pinGod.SolenoidPulse("saucer_left");
		}
		else
		{
			pinGod.SolenoidPulse("saucer_top_left_tunnel"); //saucer left tunnel, creates ball in plunger lane (in VP)
			pinGod.PlayMusic("mus_ballready");
		}
	}
	/// <summary>
	/// Awards mystery based from the <see cref="_mysteryChoices"/>
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	float AwardMystery(int index)
	{
		var delay = 0f;
		switch (index)
		{
			case 0: //100k		
				pinGod.AddPoints(NightmareConstants.LARGE_SCORE * 4);
				awardText = $"100K";
				delay = 1.0f;
				game.PlayThenResumeMusic("mus_100k", delay);
				break;
			case 1:
			case 5:
				player.BonusHeld = true;
				awardText = $"BONUS HELD";
				delay = 1.5f;
				break;
			case 2:
				pinGod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE);
				awardText = $"1\nMILLION";
				delay = 2.5f;
				game.PlayThenResumeMusic("mus_spinmillion", delay);
				break;
			case 3:
				pinGod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE*3);
				awardText = $"3\nMILLION";
				delay = 2.5f;
				game.PlayThenResumeMusic("mus_spinmillion", delay);
				break;
			case 4:
				pinGod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE * 10);
				awardText = $"10\nMILLION";
				delay = 2.5f;
				game.PlayThenResumeMusic("mus_spinmillion", delay);
				break;
			case 6:
				player.LanePanicLit = true;
				player.LaneExtraBallLit = true;
				delay = 2.5f;
				break;
			case 7:
				player.ExtraBalls++;
				delay = 4.5f;
				game.PlayThenResumeMusic("mus_extraball", delay);
				break;
			case 8:
				var doubleScore = player.Points * 2;
				pinGod.AddPoints((int)doubleScore);
				awardText = $"CATCH UP!\n{doubleScore}";
				delay = 2.5f;
				break;
			case 9: //jackpot				
				pinGod.AddPoints(player.JackpotValue);
				awardText = $"JACKPOT!\n{player.JackpotValue}";
				delay = 2.0f;
				game.PlayThenResumeMusic("mus_jackpot", delay);
				break;
			default:
				delay = 1.0f;
				awardText = "DEFAULT";
				break;
		}

		pinGod.LogInfo("awarding mystery spin: " + awardText, " delay", delay);
		game.OnDisplayMessage($"MYSTERY\n{awardText}", delay);
		return delay;
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
		player.LeftLock = false;
		player.LanePanicLit = false;
		player.LaneExtraBallLit = false;
		UpdateLamps();

		pinGod.LogInfo($"ball start: mystery scoop");
	}
	void UpdateLamps() 
	{
		if (player.MysterySpinLit) pinGod.SetLampState("spin_left", 2);
		else pinGod.SetLampState("spin_left", 0);

		if (player.LeftLock) pinGod.SetLampState("lock_left", 2);
		else pinGod.SetLampState("lock_left", 0);

		if (player.LanePanicLit) pinGod.SetLampState("panic", 1);
		else pinGod.SetLampState("panic", 0);

		if (player.LaneExtraBallLit) pinGod.SetLampState("xb_right", 1);
		else pinGod.SetLampState("xb_right", 0);
	}

	private void Timer_timeout()
    {
		_selectedAward = GetRandomAward();
		awardText = _mysteryChoices[_selectedAward];
		var delay = AwardMystery(_selectedAward);

		pinGod.LogDebug("delay for mystery " + timer);
		ballStack.Start(delay);
	}
}
