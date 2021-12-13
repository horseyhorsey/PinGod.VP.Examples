using Godot;

public class BaseGameMode : Node
{

	//todo: bonus count down. if starts at 58000, the it will take the 1000s off, then do the 10Ks

	[Export] string[] _roman_lamps;

	uint _rampComboDelay = 5000;

	#region Fields
	private Game game;
	private PinGodGame pinGod;
	private NightmarePlayer player;
	private float _musicTime;    
	#endregion

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		game = GetParent().GetParent() as Game;

		//this array only works when initialized here???
		_roman_lamps = new string[] { "num_r_viii", "num_l_viii", "num_r_ix", "num_l_ix", "num_r_x", "num_l_x", "num_r_xi", "num_l_xi", "num_r_xii", "num_l_xii" };
		//pinGod.LogDebug("roman lamps ", _roman_lamps.Length);
	}
	public override void _Input(InputEvent @event)
	{
		if (!pinGod.GameInPlay || !pinGod.IsBallStarted) return;
		if (pinGod.IsTilted) return; //game is tilted, don't process other switches when tilted

		if (pinGod.SwitchOn("start", @event))
		{
			pinGod.LogDebug("attract: starting game. started?", pinGod.StartGame());
		}
		if (pinGod.SwitchOn("start_gate", @event))
		{
			pinGod.AddPoints(NightmareConstants.SMALL_SCORE);
			pinGod.AudioManager.PlayBgm();
		}
		if (pinGod.SwitchOn("outlane_l", @event))
		{
			pinGod.AddPoints(NightmareConstants.MED_SCORE);
		}
		if (pinGod.SwitchOn("inlane_l", @event))
		{
			ScoreInlanes();
			if (player.LanePanicLit)
			{
				player.LanePanicLit = false;
				//todo: start hurry up "mus_hurryup"         
			}
			pinGod.SolenoidPulse("flasher_left");
		}
		if (pinGod.SwitchOn("inlane_r", @event))
		{			
			if (player.LaneExtraBallLit)
			{
				player.ExtraBallLit = true;
				game.OnDisplayMessage("COLLECT EXTRA\nBALL");
				player.LaneExtraBallLit = false;
				pinGod.PlaySfx("snd_down");
			}

			ScoreInlanes();
			pinGod.SolenoidPulse("flasher_right");
		}
		if (pinGod.SwitchOn("outlane_r", @event))
		{
			pinGod.AddPoints(NightmareConstants.MED_SCORE);
		}
		if (pinGod.SwitchOn("sling_l", @event))
		{
			pinGod.AddPoints(150);
			pinGod.SolenoidPulse("flasher_left");
			pinGod.PlaySfx("snd_slingshot");
		}
		if (pinGod.SwitchOn("sling_r", @event))
		{
			pinGod.AddPoints(150);
			pinGod.SolenoidPulse("flasher_right");
			pinGod.PlaySfx("snd_slingshot");
		}
		if (pinGod.SwitchOn("bumper_1", @event))
		{
			OnBumperHit();
		}
		if (pinGod.SwitchOn("bumper_2", @event))
		{
			OnBumperHit();
		}
		if (pinGod.SwitchOn("bumper_3", @event))
		{
			OnBumperHit();
		}

		var orbLTime = pinGod.GetLastSwitchChangedTime("orbit_l");
		var orbRTime = pinGod.GetLastSwitchChangedTime("orbit_r");

		if (pinGod.SwitchOn("orbit_r", @event))
		{
			pinGod.SolenoidPulse("flasher_right_mid");
			AwardCoffin();
			if (orbLTime < 5000)
			{
				if (player.RunForLifeOn)
				{
					player.RunForLifeOn = false;
					pinGod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE);
					if (!player.MidnightRunning)
					{						
						//run for your life
						if (game != null)
						{
							game.PlayThenResumeMusic("mus_rightorbitcombo", 2.9f);
							game.OnDisplayMessage("RUN FOR\nYOUR LIFE\n" + NightmareConstants.EXTRA_LARGE_SCORE.ToScoreString());
						}
					}
				}
				else
				{
					pinGod.PlaySfx("snd_start");
				}
			}
			else
			{
				player.RunForLifeOn = false;
			}

			UpdateLamps();
		}

		if (pinGod.SwitchOn("orbit_l", @event))
		{
			pinGod.AddPoints(NightmareConstants.MED_SCORE * 10); //100K
			pinGod.PlaySfx("snd_lowsquirk");

			if(orbRTime < 2000 && orbRTime > 0)
			{
				player.RunForLifeOn = true;
				game.OnDisplayMessage("RUN FOR\nYOUR LIFE");
				UpdateLamps();
			}
		}
		//midnight ramps
		ProcessRampLeft(@event);
		ProcessRampRight(@event);
	}
	public void AdvanceRoman()
	{
		if (player.RomanValue < 10)
		{
			player.RomanValue++;
			pinGod.AddBonus(NightmareConstants.SMALL_SCORE);
			UpdateRomanLamps();
		}
	}

	/// <summary>
	/// Awards the blinking coffin 
	/// </summary>
	private void AwardCoffin()
	{
		player.CoffinValue++;
		if (player.CoffinValue <= 4)
		{
			var points = 0;
			if (player.CoffinStack[4]) points = NightmareConstants.EXTRA_LARGE_SCORE;
			else if (player.CoffinStack[3])
			{
				points = NightmareConstants.EXTRA_LARGE_SCORE - NightmareConstants.EXTRA_LARGE_SCORE / 4;
				player.CoffinStack[4] = true;
			}
			else if (player.CoffinStack[2])
			{
				points = NightmareConstants.EXTRA_LARGE_SCORE - NightmareConstants.EXTRA_LARGE_SCORE / 2;
				player.CoffinStack[3] = true;
			}
			else if (player.CoffinStack[1])
			{
				points = NightmareConstants.EXTRA_LARGE_SCORE / 4;
				player.CoffinStack[2] = true;
			}

			game.OnDisplayMessage("COFFIN AWARD\n" + points.ToScoreString());
			pinGod.AddPoints(points);
			pinGod.LogInfo("basemode: awarded coffin stack: ", player.CoffinValue, " ", points);
		}		
	}

	private void OnBallDrained()
	{
		pinGod.StopMusic();
		pinGod.DisableAllLamps();
	}

	/// <summary>
	/// Resets cross and coffin. Starts music for plunger lane
	/// </summary>
	private void OnBallStarted()
	{
		pinGod.LogInfo("base_mode: ball started");
		//pinGod.AudioManager.PlayBgm();

		player = ((NightmarePinGodGame)pinGod).GetPlayer();        		
		pinGod.AddBonus(500);

		player.CrossStack = new byte[5] { 2, 0, 0, 0, 0 };
		player.CoffinStack = new bool[5];

		pinGod.PlayMusic("mus_ingame00");

		UpdateLamps();
	}

	/// <summary>
	/// Add score and bonus when a bumper is hit
	/// </summary>
	private void OnBumperHit()
	{
		pinGod.AddPoints(NightmareConstants.SMALL_SCORE);
		pinGod.AddBonus(100);
		pinGod.PlaySfx("snd_bumper");
		pinGod.SolenoidPulse("flasher_top_left");
	}

	private void ProcessRampLeft(InputEvent @event)
	{
		if (player.MidnightRunning) return;

		var rampRChanged = pinGod.GetLastSwitchChangedTime("ramp_r");
		if (pinGod.SwitchOn("ramp_l", @event))
		{            
			//music to play and delay to resume music
			string music = "mus_leftramp"; float delay = 1.5f;
			//ramp score
			ScoreForRamps();

			if (game.RemixSoundsMode)
			{
				music = "mus_remix_leftramp"; delay = 2.5f;
			}

			if (rampRChanged <= _rampComboDelay)
			{
				Logger.LogInfo("bgm: r/l ramp combo. " + rampRChanged);
				pinGod.AddPoints(NightmareConstants.SCORE_100K);
				music = "mus_rampmillion";
				delay = 1.8f;
			}
            else
            {
				pinGod.AddPoints(NightmareConstants.SCORE_50K);
			}
			//advance roman numerals?
			var romanVal = player.RomanValue;
			if (romanVal % 2 == 1)
			{
				AdvanceRoman();
				Logger.LogInfo("bgm: l/ramp roman to midnight " + player.RomanValue);
				delay = 1.2f;
				if (!player.MidnightRunning)
				{
					//todo: display extra hour ExtraHour
					music = "mus_extrahour";
				}
			}
			//start midnight?
			if (romanVal == 10 && !player.MidnightRunning)
			{
				pinGod.DisableAllLamps();
				player.MidnightRunning = true;
				game.CallDeferred(nameof(Game.StartMidnight));
			}

			if (!player.MidnightRunning)
			{
				if (game != null)
				{
					game.PlayThenResumeMusic(music, delay);
				}
				else GD.PushWarning("no game found left ramp");
			}

			UpdateLamps();
		}
	}

	/// <summary>
	/// Process the right ramp to start midnight...this shot is the first in the ramp sequence
	/// </summary>
	/// <param name="event"></param>
	private void ProcessRampRight(InputEvent @event)
	{		
		//var game = (pinGod as CustomPinGodGame);
		if (player.MidnightRunning) return;

		var rampLChanged = pinGod.GetLastSwitchChangedTime("ramp_l");
		if (pinGod.SwitchOn("ramp_r", @event))
		{
			//music to play and delay to resume music
			string music = "mus_rightramp"; float delay = 1.5f;
			ScoreForRamps();

			if (game.RemixSoundsMode)
			{				
				music = "mus_remix_rightramp"; 
			}

			if (rampLChanged <= _rampComboDelay)
			{				
				player.ScoreBonusLit = true;
				pinGod.AddPoints(NightmareConstants.SCORE_100K);
				music = "mus_rampmillion"; delay = 1.8f;
				Logger.LogInfo("bgm: r/l ramp combo. " + rampLChanged);
			}
            else
            {
				pinGod.AddPoints(NightmareConstants.SCORE_50K);
			}

			//advancing roman numerals?
			var romanVal = player.RomanValue;
			if (romanVal % 2 == 0)
			{										
				AdvanceRoman();
				Logger.LogInfo("bgm: r ramp roman to midnight " + player.RomanValue);
				if (music != "mus_leftramp") music = "mus_rightramp";
			}
			else
			{
				if (music != "mus_rampmillion") music = "mus_rightramp";
			}

			//stop music and play ramp music
			if (!player.MidnightRunning)
			{
				if (game != null)
				{
					game.PlayThenResumeMusic(music, delay);
				}
				else GD.PushWarning("no game found left ramp");
			}

			UpdateLamps();
		}
	}

	/// <summary>
	/// Adds score and increases jackpot
	/// </summary>
	private void ScoreForRamps()
	{
		player.JackpotValue += 8000;
		pinGod.AddBonus(NightmareConstants.SMALL_SCORE);		
		Logger.LogInfo($"bgm: ramp jackpot added value. {player.JackpotValue}");
	}

	/// <summary>
	/// Scores and plays sound
	/// </summary>
	private void ScoreInlanes()
	{
		pinGod.AddPoints(NightmareConstants.MED_SCORE);
		pinGod.PlaySfx("sd_inlane"); //todo: remix sound
	}

	public void UpdateLamps()
	{
		if (player.RunForLifeOn) pinGod.SetLampState("arrow_right", 2);
		else pinGod.SetLampState("arrow_right", 0);

		//coffin stack lamps
		if (player.CoffinStack[4])
		{
			pinGod.SetLampState("coffin_1", 1);
			pinGod.SetLampState("coffin_2", 1);
			pinGod.SetLampState("coffin_3", 1);
			pinGod.SetLampState("coffin_4", 1);
		}
		else if (player.CoffinStack[3])
		{
			pinGod.SetLampState("coffin_1", 1);
			pinGod.SetLampState("coffin_2", 1);
			pinGod.SetLampState("coffin_3", 1);
			pinGod.SetLampState("coffin_4", 2);
		}
		else if (player.CoffinStack[2])
		{
			pinGod.SetLampState("coffin_1", 1);
			pinGod.SetLampState("coffin_2", 1);
			pinGod.SetLampState("coffin_3", 2);
		}
		else if (player.CoffinStack[1])
		{
			pinGod.SetLampState("coffin_1", 1);
			pinGod.SetLampState("coffin_2", 2);
		}
		else if (!player.CoffinStack[1])
		{
			pinGod.SetLampState("coffin_1", 2);
		}
		//hurry up
		if (player.CoffinStack[1]) pinGod.SetLampState("coffin_0", 2);
		else pinGod.SetLampState("coffin_0", 0);

		UpdateRomanLamps();
	}

	private void UpdateRomanLamps()
	{		
		if (_roman_lamps?.Length > 0)
		{
			var val = player.RomanValue;
			for (int i = 0; i < _roman_lamps.Length; i++)
			{
				if(i == 0)
					pinGod.SetLampState(_roman_lamps[i], 2);

				if (val < i + 1)
				{
					pinGod.SetLampState(_roman_lamps[i], 2);
					break;
				}

				pinGod.SetLampState(_roman_lamps[i], 1);
			}
		}
	}
}
