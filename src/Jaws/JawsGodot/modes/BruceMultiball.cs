using Godot;
using System;
using System.Collections.Generic;

public class BruceMultiball : Node
{
	#region Properties / Fields
	private Control activatorControl;
	private BlinkingLabel activatorLabel;
	private BlinkingLabel superLabel;
	private Game game;
    private JawsPinGodGame pinGod;
    private Timer jawsChompTimer;
	private Timer clearJackpotLayersTimer;
	private Timer jawsTimer;
	private Timer lockTimer;
	private Timer suspenseTimer;
	public bool CanSkipScene { get; private set; }
	public bool IsMultiballRunning { get; set; }
	public VideoPlayerPinball VideoPlayer { get; private set; }
	public Dictionary<string, VideoStreamTheora> VideoStreams { get; private set; }

	bool[] JackportTargets = new bool[2];
	private bool _jackpotBusy;
	const string MSG_LEFT_TARGET = "SHOOT\r\nLEFT TARGET";
	const string MSG_RIGHT_TARGET = "SHOOT\r\nRIGHT TARGET";
	const string MSG_SUPER_JACKPOT = "SUPER JACKPOT";
	const string MSG_SUPER_RDY = "SUPER\r\nIS READY";
	#endregion

	public override void _EnterTree()
	{
		base._EnterTree();
		game = GetParent().GetParent() as Game;
		pinGod = GetNode("/root/PinGodGame") as JawsPinGodGame;
		jawsTimer = GetNode("JawsTimer") as Timer;
		lockTimer = GetNode("LockTimer") as Timer;
		suspenseTimer = GetNode("SuspenseTimer") as Timer;
		jawsChompTimer = GetNode("JawsChompTimer") as Timer;

		clearJackpotLayersTimer = GetNode("CenterContainer/ClearJackpotLayersTimer") as Timer;		

		activatorControl = GetNode("Activator") as Control;
		activatorControl.Visible = false;
		activatorLabel = activatorControl.GetNode("CenterContainer/BlinkingLabel") as BlinkingLabel;

		superLabel = GetNode("CenterContainer/BlinkingLabel") as BlinkingLabel;
		superLabel.Text = ""; superLabel.Visible = false;

		VideoPlayer = GetNode("VideoPlayerPinball") as VideoPlayerPinball;
		VideoStreams = new Dictionary<string, VideoStreamTheora>();
		VideoStreams.Add("quint_lock_1a", new VideoStreamTheora() { File = "res://assets/videos/quint_lock_1a.ogv" });
		VideoStreams.Add("quint_lock_2a", new VideoStreamTheora() { File = "res://assets/videos/quint_lock_2a.ogv" });
		VideoStreams.Add("quint_lock_3a", new VideoStreamTheora() { File = "res://assets/videos/quint_lock_3a.ogv" });
		VideoStreams.Add("quint_lock_1b", new VideoStreamTheora() { File = "res://assets/videos/quint_lock_1b.ogv" });
		VideoStreams.Add("quint_lock_2b", new VideoStreamTheora() { File = "res://assets/videos/quint_lock_2b.ogv" });
		VideoStreams.Add("quint_lock_3b", new VideoStreamTheora() { File = "res://assets/videos/quint_lock_3b.ogv" });
		VideoStreams.Add("quint_dead", new VideoStreamTheora() { File = "res://assets/videos/quint_dead.ogv" });
		VideoStreams.Add("brody_bruce_start", new VideoStreamTheora() { File = "res://assets/videos/brody_bruce_start.ogv" });
		VideoStreams.Add("brody_miss_01", new VideoStreamTheora() { File = "res://assets/videos/brody_miss_01.ogv" });
		VideoStreams.Add("brody_miss_02", new VideoStreamTheora() { File = "res://assets/videos/brody_miss_02.ogv" });
		VideoStreams.Add("brody_super_shot", new VideoStreamTheora() { File = "res://assets/videos/brody_super_shot.ogv" });

	}

	public override void _Input(InputEvent @event)
	{
		if (!pinGod.GameInPlay || pinGod.IsTilted) return;

		//lock VUK
		if (pinGod.SwitchOn("bruce_vuk", @event))
		{
			BruceVukHit();
		}
		//This is the Exit hole when hitting jaws's jaw 
		if (pinGod.SwitchOn("jaws_kicker", @event))
		{
			if (!IsMultiballRunning)
			{
				GD.Print("jaws kicker hit");
				pinGod.EnableJawsToy(true);
				if (pinGod.IsMultiballRunning)
				{
					pinGod.SolenoidOn("flash_vuk", 1);
					JawsKickinMultiball();
				}
				else
				{
					if (game.currentPlayer.SharkLockEnabled)
					{
						JawsLockCheck();
					}
					else
					{
						Kickoutjaws();
					}
				}
			}
			else
			{
				//todo: score jackpotseed * 10 and add to best
				//game.currentPlayer.BruceMballBest = 

				pinGod.DisableAllLamps();
				game.SetGiState(LightState.Blink);
				JackportTargets = new bool[2];
				pinGod.EnableJawsToy(true);
				GD.Print("super jackpot");
				_jackpotBusy = true;
				superLabel.Text = MSG_SUPER_JACKPOT;
				superLabel.Visible = true;
				pinGod.PlaySfx("brody_super_cheer");
				CallDeferred("PlayScene", "brody_super_shot");
				jawsTimer.Start(4.4f);
			}
		}

		if (pinGod.SwitchOn("lock_activate_0", @event))
		{
			ActivatorCheck(0);
		}
		if (pinGod.SwitchOn("lock_activate_1", @event))
		{
			ActivatorCheck(1);
		}
		if (pinGod.SwitchOn("lock_activate_2", @event))
		{
			ActivatorCheck(2);
		}

		//skip mouth and lock scenes
		if (CanSkipScene && !game.IsMultiballScoringStarted)
		{
			if (pinGod.SwitchOn("flipper_l", @event))
			{
				if (pinGod.SwitchOn("flipper_r"))
				{
					SkipBruceScene();
				}
			}
		}

		if (IsMultiballRunning)
		{
			//Brody_ShotMiss01
			if (pinGod.SwitchOn("jaws_target_left", @event))
			{
				if (!_jackpotBusy)
				{
					if (!JackportTargets[0])
					{
						JackportTargets[0] = true;
						if (JackportTargets[1])
						{
							superLabel.Text = MSG_SUPER_RDY;
							superLabel.Visible = true;
							pinGod.EnableJawsToy(false);
							CallDeferred("PlayScene", "brody_miss_01");
							clearJackpotLayersTimer.Start();
						}
						else
						{
							superLabel.Text = MSG_RIGHT_TARGET;
							superLabel.Visible = true;
							CallDeferred("PlayScene", "brody_miss_01");
							clearJackpotLayersTimer.Start();
						}

						UpdateLamps();
					}					
				}
			}
			if (pinGod.SwitchOn("jaws_target_right", @event))
			{
				if (!_jackpotBusy)
				{
					if (!JackportTargets[1])
					{
						clearJackpotLayersTimer.Stop();
						JackportTargets[1] = true;
						if (JackportTargets[0])
						{
							superLabel.Text = MSG_SUPER_RDY;
							superLabel.Visible = true;
							pinGod.EnableJawsToy(false);							
							CallDeferred("PlayScene", "brody_miss_01");
							clearJackpotLayersTimer.Start();
						}
						else
						{
							superLabel.Text = MSG_LEFT_TARGET;
							superLabel.Visible = true;
							CallDeferred("PlayScene", "brody_miss_02");
							clearJackpotLayersTimer.Start();
						}

						UpdateLamps();
					}
				}
			}
		}
	}

	private void SkipBruceScene()
	{
		CanSkipScene = false;
		game.currentPlayer.BonusSkipper += game.DoublePlayfield ? Game.BONUS_SKIPPER_VALUE * 2 : Game.BONUS_SKIPPER_VALUE;        
		lockTimer.Stop();        
		game.AddPoints(7500, false);
		if (pinGod.SwitchOn("bruce_vuk"))
		{
			CallDeferred(nameof(_on_LockTimer_timeout));
		}
		else
		{
			Kickoutjaws();
		}
	}

	public void UpdateLamps()
	{
		//activator lamps
		for (int i = 0; i < game.currentPlayer.QuintActivated.Length; i++)
		{
			if (game.currentPlayer.QuintActivated[i]) pinGod.SetLampState("lock_activate_" + i, 1);
			else pinGod.SetLampState("lock_activate_" + i, 0);
		}
		//bruce m-ball lamp
		if (game.currentPlayer.BruceMballComplete) pinGod.SetLampState("bruce_multiball", 1);
		else pinGod.SetLampState("bruce_multiball", 0);

		if (IsMultiballRunning)
		{
			//if(JackportTargets[0])
			if (JackportTargets[0]) pinGod.SetLampState("jaws_l", 1);
			else pinGod.SetLampState("jaws_l", 2);

			if (JackportTargets[1]) pinGod.SetLampState("jaws_r", 1);
			else pinGod.SetLampState("jaws_r", 2);
		}

		if (IsMultiballRunning) pinGod.SetLampState("bruce_multiball", 2);
		else if (game.currentPlayer.BarrelMballComplete) pinGod.SetLampState("bruce_multiball", 1);
		else pinGod.SetLampState("bruce_multiball", 0);

		var locks = game.currentPlayer.SharkLocksComplete;
		if (game.currentPlayer.SharkLockEnabled && locks == 0)
		{
			pinGod.SetLampState("bruce_0", 2);
		}
		else if (locks == 1)
		{
			pinGod.SetLampState("bruce_0", 1);

			if (game.currentPlayer.SharkLockEnabled)
			{
				pinGod.SetLampState("bruce_1", 2);
				pinGod.SetLampState("shark_lock_bulb", 1);
			}
			else
			{
				pinGod.SetLampState("bruce_1", 0);
				pinGod.SetLampState("shark_lock_bulb", 0);
			}
		}
		else if (locks == 2)
		{
			pinGod.SetLampState("bruce_0", 1);
			pinGod.SetLampState("bruce_1", 1);
			if (game.currentPlayer.SharkLockEnabled)
			{
				pinGod.SetLampState("bruce_2", 2);
				pinGod.SetLampState("shark_lock_bulb", 1);
			}
			else
			{
				pinGod.SetLampState("bruce_2", 0);
				pinGod.SetLampState("shark_lock_bulb", 0);
			}
		}
		else if (locks == 3)
		{
			pinGod.SetLampState("bruce_0", 1);
			pinGod.SetLampState("bruce_1", 1);
			pinGod.SetLampState("bruce_2", 1);
		}

		//the lamps to the right of playfield
		var quintOn = game.currentPlayer.QuintLockEnabled;
		switch (game.currentPlayer.QuintLocksComplete)
		{
			case 0:
				if (quintOn)
					pinGod.SetLampState("quint_0", 2);
				break;
			case 1:
				pinGod.SetLampState("quint_0", 1);
				if (quintOn)
					pinGod.SetLampState("quint_1", 2);
				break;
			case 2:
				pinGod.SetLampState("quint_0", 1);
				pinGod.SetLampState("quint_1", 1);
				if (quintOn)
					pinGod.SetLampState("quint_2", 2);
				break;
			case 3:
				pinGod.SetLampState("quint_0", 1);
				pinGod.SetLampState("quint_1", 1);
				pinGod.SetLampState("quint_2", 1);
				break;
			default:
				break;
		}
	}

	private void _on_ClearJackpotLayersTimer_timeout()
	{
		superLabel.Visible = false;
	}
	private void _on_JawsChompTimer_timeout()
	{
		if (pinGod.JawsToyState > 0)
			pinGod.EnableJawsToy(false);
		else
			pinGod.EnableJawsToy(true);
	}
	private void _on_JawsTimer_timeout()
	{
		Kickoutjaws();
	}
	private void _on_LockTimer_timeout()
	{
		int lockNum = PlayHurryUpSuspense();

		GD.Print("jaws open:" + game.IsJawsLockReady());
		pinGod.SolenoidOn("flash_jaws", 1);
		superLabel.Text = "\r\n\r\nSHOOT\r\nSHARK"; superLabel.Visible = true;
		suspenseTimer.Start(3.2f);
		KickoutLock();
		switch (lockNum)
		{
			case 1:				
				CallDeferred("PlayScene", "quint_lock_1b");
				break;
			case 2:
				CallDeferred("PlayScene", "quint_lock_2b");
				break;
			case 3:
				CallDeferred("PlayScene", "quint_lock_3b");
				break;
			default:
				break;
		}		
	}

	private int PlayHurryUpSuspense()
	{
		var lockNum = game.currentPlayer.QuintLocksComplete;
		if (lockNum < 3)
		{
			pinGod.PlayMusic("music_suspense_end");
		}

		return lockNum;
	}

	private void _on_RemoveActivatorTimer_timeout()
	{
		activatorControl.Visible = false;
	}
	private void _on_SuspenseTimer_timeout()
	{
		//update game lamps		
		//todo: music		
		superLabel.Visible = false;
		game.UpdateLamps();
		pinGod.PlayMusic("suspense_loop");
		game.StartBruceHurryUp();
	}
	/// <summary>
	/// Activator locks for burce. Will return if multiball is running
	/// </summary>
	/// <param name="index"></param>
	void ActivatorCheck(int index)
	{
		game.AddPoints(7500);
		game.currentPlayer.Bonus += Game.BONUS_VALUE;
		if (IsMultiballRunning)
			return;

		var quintOn = game.currentPlayer.QuintLockEnabled;
		var sharkOn = game.currentPlayer.SharkLockEnabled;

		pinGod.SolenoidPulse("flash_lock");
		pinGod.PlaySfx("shark_hit_fx");

		if (quintOn || sharkOn)
			return;
		else
		{
			game.AddPoints(7500);
			GD.Print("activated");
			if (!game.currentPlayer.QuintActivated[index])
			{				
				game.currentPlayer.QuintActivated[index] = true;
			}

			var completed = game.currentPlayer.IsActivatorComplete();
			//scene: bStars, sharktoon
			if (completed)
			{
				GD.Print("activator completed");
				game.currentPlayer.QuintLockEnabled = true;
				activatorLabel.Text = "BRUCE\r\nLOCKS OPEN";
				if (!pinGod.IsMultiballRunning)
				{
					pinGod.PlayMusic("dundun_1");
				}

				pinGod.SolenoidOn("diverter", 1);
			}
			else
			{
				activatorLabel.Text = "BRUCE\r\nACTIVATOR";
			}

			activatorControl.Visible = true;
			var timer = activatorControl.GetNode("RemoveActivatorTimer") as Timer;
			timer.Stop();
			timer.Start(1.2f);
			UpdateLamps();
		}
	}
	private void BruceVukHit()
	{
		GD.Print("bruce kicker");
		if (pinGod.IsMultiballRunning)
		{
			GD.Print("bruce: in multiball exiting");
			game.currentPlayer.BonusBruce += game.DoublePlayfield ? Game.BONUS_BRUCEY_VALUE * 2 : Game.BONUS_BRUCEY_VALUE;
			KickoutLock();
		}
		else if (game.currentPlayer.BarrelsOn)
		{
			GD.Print("bruce: targets up exiting");
			game.PlayBarrelReminderScene();
			KickoutLock();
		}
		else
		{
			GD.Print("bruce: checking quint");
			QuintLockCheck();
		}
	}
	private void EndMultiball()
	{
		if (IsMultiballRunning)
		{
			IsMultiballRunning = false;
			game.currentPlayer.SharkLockEnabled = false;
			game.currentPlayer.SharkLocksComplete = 0;
			game.currentPlayer.QuintCount = 0;
			pinGod.EnableJawsToy(true);
			UpdateLamps();
			GD.Print("bruce: EndMultiball");
		}
	}
	private void JawsKickinMultiball()
	{

	}
	/// <summary>
	/// When player hits the Jaws mouth
	/// </summary>
	private void JawsLockCheck()
	{
		if (game.currentPlayer.SharkLockEnabled)
		{
			GD.Print("bruce: jaws lock check. shark complete:", game.currentPlayer.SharkLocksComplete);
			pinGod.SolenoidOn("flash_vuk", 1);
			pinGod.SolenoidOn("flash_jaws", 0);
			game.AwardHurryUp();

			if (game.currentPlayer.SharkLocksComplete < 2)
			{
				game.AddPoints(100000);
				pinGod.SolenoidOn("diverter", 1);
				game.currentPlayer.SharkLockEnabled = false;
				game.currentPlayer.QuintLockEnabled = true;
				game.IsJawsLockReady();

				if (game.currentPlayer.SharkLocksComplete == 0)
					game.currentPlayer.SharkLocksComplete++;
				else
					game.currentPlayer.SharkLocksComplete = 2;

				pinGod.PlaySfx("shark_hit_fx");
				pinGod.DisableAllLamps();
				game.SetGiState(LightState.On);
				jawsTimer.Start(2.0f);
			}
			else
			{
				pinGod.AudioManager.StopMusic();

				game.currentPlayer.SharkLocksComplete = 3;
				CanSkipScene = true;
				jawsTimer.Start(11.8f);
				jawsChompTimer.Start(0.8f);
				superLabel.Text = "\r\n\r\nBRUCE\r\nMULTIBALL"; superLabel.Visible = true; //TODO: scene timer which displays what to do in multiball
				CallDeferred("PlayScene", "quint_dead");
			}

			GD.Print("bruce: jaws lock check. shark complete:", game.currentPlayer.SharkLocksComplete);
		}
		else
		{
			Kickoutjaws();
		}
	}
	/// <summary>
	/// Starting multiball?
	/// </summary>
	private void Kickoutjaws()
	{
		pinGod.SolenoidOn("flash_vuk", 0);
		pinGod.SolenoidOn("flash_jaws", 0);

		game.currentPlayer.Bonus += Game.BONUS_VALUE;
		game.currentPlayer.BonusBruce += game.DoublePlayfield ? Game.BONUS_BRUCEY_VALUE * 2 : Game.BONUS_BRUCEY_VALUE;

		if (!IsMultiballRunning)
		{
			game.AwardHurryUp();
			game.StopHurryUp();
			game.currentPlayer.SharkLockEnabled = false;

			//starting multiball
			if (game.currentPlayer.SharkLocksComplete == 3)
			{
				jawsChompTimer.Stop();
				IsMultiballRunning = true;
				game.currentPlayer.BruceMballCompleteCount++;
				game.UpdateProgress();

				pinGod.AudioManager.StopMusic();
				pinGod.PlayMusic("end_music_final");
				game.AddMultiScoringMode();
				pinGod.StartMultiBall(4, 20);
				game.currentPlayer.ResetBruce();
				pinGod.EnableJawsToy(true);
				pinGod.SolenoidPulse("jaws_kicker");
				superLabel.Text = "\r\n\r\nBRUCE\r\nMULTIBALL"; superLabel.Visible = true; //TODO: scene timer which displays what to do in multiball
				CallDeferred("PlayScene", "brody_bruce_start");
			}
			else
			{
				game.PlayMusicForMode();
				pinGod.SolenoidPulse("jaws_kicker");
			}			
			game.UpdateLamps();
		}
		else
		{
			_jackpotBusy = false;
			superLabel.Visible = false;

			game.UpdateLamps();
			pinGod.SolenoidOn("flash_jaws", 0);
			pinGod.SolenoidPulse("jaws_kicker");
		}
	}
	private void KickoutLock()
	{
		pinGod.SolenoidPulse("bruce_vuk");
	}
	private void PlayScene(string name)
	{
		activatorControl.Visible = false;
		GD.Print("playing scene: ", name);
		VideoPlayer.Stream = VideoStreams[name];
		VideoPlayer.Play();
		VideoPlayer.Visible = true;		
	}
	void QuintLockCheck()
	{
		var quinkLocksOn = game.currentPlayer.QuintLockEnabled;
		var quinkLocksComplete = game.currentPlayer.QuintLocksComplete;
		game.currentPlayer.BonusBruce += game.DoublePlayfield ? Game.BONUS_BRUCEY_VALUE * 2 : Game.BONUS_BRUCEY_VALUE;
		if (quinkLocksOn)
		{
			GD.Print("quint check locks on");
			pinGod.DisableAllLamps();
			game.SetGiState(LightState.On);
			game.currentPlayer.BonusBruce += game.DoublePlayfield ? Game.BONUS_BRUCEY_VALUE * 2 : Game.BONUS_BRUCEY_VALUE;
			switch (quinkLocksComplete)
			{
				case 0:
					game.currentPlayer.QuintLocksComplete++;
					lockTimer.Start(4.3f);
					CanSkipScene = true;
					superLabel.Text = "\r\n\r\nBRUCE LOCK 1"; superLabel.Visible = true;
					CallDeferred("PlayScene", "quint_lock_1a");
					break;
				case 1:
					lockTimer.Start(5f);
					CanSkipScene = true;
					game.currentPlayer.QuintLocksComplete++;
					superLabel.Text = "\r\n\r\nBRUCE LOCK 2"; superLabel.Visible = true;
					CallDeferred("PlayScene", "quint_lock_2a");
					break;
				case 2:
					game.currentPlayer.QuintLocksComplete++;
					lockTimer.Start(4.8f);
					CanSkipScene = true;
					superLabel.Text = "\r\n\r\nBRUCE LOCK 3"; superLabel.Visible = true;
					CallDeferred("PlayScene", "quint_lock_3a");
					break;
				default:
					break;
			}

			game.currentPlayer.QuintLockEnabled = false;
			game.currentPlayer.SharkLockEnabled = true;
		}
		else
		{
			GD.Print("quint check locks off");
			KickoutLock();
		}
	}
}
