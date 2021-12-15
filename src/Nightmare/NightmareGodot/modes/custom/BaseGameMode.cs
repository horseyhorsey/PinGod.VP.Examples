using Godot;

public class BaseGameMode : Node
{
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
			pinGod.StopMusic();
			if(!game.bgmMusicPlayer.Playing)
				game.bgmMusicPlayer.Play();
			game.PauseBgm(false);
		}
		if (pinGod.SwitchOn("outlane_l", @event))
		{
			pinGod.AddPoints(NightmareConstants.MED_SCORE);
		}
		if (pinGod.SwitchOn("inlane_l", @event))
		{			
			if (player.LanePanicLit)
			{
				player.LanePanicLit = false;
				game.StartHurryUp(true);			
			}
			ScoreInlanes();
			pinGod.SolenoidPulse("flasher_left");
		}
		if (pinGod.SwitchOn("inlane_r", @event))
		{			
			if (player.LaneExtraBallLit)
			{
				player.LaneExtraBallLit = false;
				player.ExtraBallLit = true;
				game.OnDisplayMessage("COLLECT EXTRA\nBALL");				
				pinGod.PlaySfx("snd_down");
			}
            else
            {
				pinGod.PlaySfx("snd_inlane"); //todo: remix sound
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

		if (!player.HurryUpRunning && pinGod.SwitchOn("orbit_r", @event))
		{
			pinGod.LogInfo("orbit r, last active: " + orbRTime);
			pinGod.SolenoidPulse("flasher_right_mid");			

			AwardCoffin();

			if (orbLTime < 5000)
			{
				if (player.RunForLifeOn)
				{
					player.RunForLifeOn = false;
					pinGod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE);
					pinGod.AddBonus(NightmareConstants.SMALL_SCORE);
					if (!player.MidnightRunning)
					{						
						//run for your life
						if (game != null)
						{
							game.PlayThenResumeMusic("mus_rightorbitcombo", 3.1f);
							game.OnDisplayMessage("RUN FOR\nYOUR LIFE\n" + NightmareConstants.EXTRA_LARGE_SCORE.ToScoreString());
						}
					}
				}
			}
			else
			{
				player.RunForLifeOn = false;
				game.OnDisplayMessage("RUN FOR\nYOUR LIFE\n");
				pinGod.PlaySfx("snd_start");
			}

			UpdateLamps();
		}

		if (pinGod.SwitchOn("orbit_l", @event))
		{
			pinGod.AddPoints(NightmareConstants.MED_SCORE * 10); //100K
			pinGod.AddBonus(NightmareConstants.SMALL_SCORE);
			pinGod.PlaySfx("snd_lowsquirk");

            if (!player.RunForLifeOn)
            {
				player.RunForLifeOn = true;
				game.OnDisplayMessage("RUN FOR\nYOUR LIFE");
				UpdateLamps();
			}			
		}

        //midnight ramps
        if (!player?.MidnightRunning ?? false)
        {
			ProcessRampLeft(@event);
			ProcessRampRight(@event);
		}		
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
		pinGod.LogInfo("awarding coffin level: " + player.CoffinValue);

		if (player.CoffinValue <= 4)
		{			
			//pinGod.LogInfo(string.Join(",", player.CoffinStack));

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
			pinGod.AddBonus(NightmareConstants.SMALL_SCORE);
			pinGod.LogInfo("basemode: awarded coffin stack: ", player.CoffinValue, " ", points);
			player.CoffinValue++;
		}		
	}

	private void OnBallDrained()
	{
		game.PauseBgm(true);
		game.bgmMusicPlayer.Stop();
		game.resumeBgmTimer.Stop();
		pinGod.StopMusic();					
		pinGod.DisableAllLamps();
		pinGod.PlaySfx("snd_drain");
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
		
		player.CrossStackReset();
		player.CoffinReset();

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

	/// <summary>
	/// Process the LEFT ramp if not in midnight mode
	/// </summary>
	/// <param name="event"></param>
	private void ProcessRampLeft(InputEvent @event)
	{		
		if (pinGod.SwitchOn("ramp_l", @event) && !player.MidnightRunning)
		{            
			//music to play and delay to resume music
			string music = "mus_leftramp"; float delay = 2f;

			ScoreForRamps();

			if (game.RemixSoundsMode)
			{
				music = "mus_remix_leftramp"; delay = 2f;
			}

			//combo for ramp - player scores 1 million in combo time
			var rampRChanged = pinGod.GetLastSwitchChangedTime("ramp_r");
			if (rampRChanged <= _rampComboDelay && rampRChanged > 0)
			{
				pinGod.LogInfo("ramps: R>L ramp combo time " + rampRChanged);
				pinGod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE);
				music = "mus_rampmillion";
				delay = 2.0f;
			}
            else
            {
				pinGod.AddPoints(NightmareConstants.SCORE_50K);
			}

			//advance roman numerals when odd
			var romanVal = player.RomanValue;
			if (romanVal % 2 == 1)
			{
				AdvanceRoman();				
				delay = 2.8f;
				game.OnDisplayMessage("MIDNIGHT GETS\nCLOSER");
				music = "mus_extrahour";
			}

			pinGod.LogInfo("ramps: roman value " + player.RomanValue);

			//LEFT RAMP STARTS MIDNIGHT
			if (player.RomanValue == 10)
			{				
				pinGod.DisableAllLamps();
				player.MidnightRunning = true;
				game.CallDeferred(nameof(Game.StartMidnight));
			}
			else
			{
				game.PlayThenResumeMusic(music, delay);
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
		if (pinGod.SwitchOn("ramp_r", @event) && !player.MidnightRunning)
		{
			var rampLChanged = pinGod.GetLastSwitchChangedTime("ramp_l");
			//music to play and delay to resume music
			string music = !game.RemixSoundsMode ? "mus_rightramp" : music = "mus_remix_rightramp";
			float delay = 1.5f;

			ScoreForRamps();

			//combo for ramp - player scores 1 million in combo time
			if (rampLChanged <= _rampComboDelay && rampLChanged > 0)
			{				
				player.ScoreBonusLit = true;
				pinGod.AddPoints(NightmareConstants.EXTRA_LARGE_SCORE);
				music = "mus_rampmillion"; delay = 1.8f;
				pinGod.LogInfo("ramps: R ramp combo. " + rampLChanged);
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
				pinGod.LogInfo("bgm: r ramp roman to midnight " + player.RomanValue);
				if (music != "mus_leftramp") music = "mus_rightramp";
			}
			else
			{
				if (music != "mus_rampmillion") music = "mus_rightramp";
			}

			//stop music and play ramp music
			game.PlayThenResumeMusic(music, delay);

			UpdateLamps();
		}
	}

	/// <summary>
	/// Adds bonus and increases jackpot value
	/// </summary>
	private void ScoreForRamps()
	{
		player.JackpotValue += NightmareConstants.MED_SCORE;
		pinGod.AddBonus(NightmareConstants.SMALL_SCORE);		
		pinGod.LogInfo($"ramps: ramp jackpot added value. {player.JackpotValue}");
	}

	/// <summary>
	/// Scores and plays sound
	/// </summary>
	private void ScoreInlanes()
	{
		pinGod.AddPoints(NightmareConstants.MED_SCORE);
		pinGod.AddBonus(NightmareConstants.SMALL_SCORE);		
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
		else if (player.CoffinStack[1] && player.CoffinValue == 2)
		{
			pinGod.SetLampState("coffin_1", 1);
			pinGod.SetLampState("coffin_2", 2);
		}
		else
		{
			pinGod.SetLampState("coffin_1", 2);
		}
		//hurry up
		if (player.CoffinStack[0]) pinGod.SetLampState("coffin_0", 2);
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
