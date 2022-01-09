using Godot;
using System.Linq;

/// <summary>
/// Truck locks. TODO: quick shot from ramp to lock bonus points
/// </summary>
public class SuperPursuitMode : Node
{
	private string[] _lockLamps = new string[] { "truck_ball_1", "truck_ball_2", "truck_ball_3" };
	private KnightRiderPlayer _player;    
	private Timer _truckLockSwitchTimer;
	private int currentLock = 0;
	private KrPinGodGame pinGod;
	[Export] int _mballTimeout = 11;

	public override void _EnterTree()
	{
		base._EnterTree();
		pinGod = GetNode<KrPinGodGame>("/root/PinGodGame");
		_truckLockSwitchTimer = GetNode<Timer>("Timer");		
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (!pinGod.GameInPlay || pinGod.IsTilted) return;

		if (_player != null)
		{			
			if (pinGod.SwitchOn("target_truck", @event))
			{
				//ProcessTruckTarget();
				pinGod.LogInfo("truck target hit");

				if(!pinGod.IsMultiballRunning)
					pinGod.PlayLampshowFlash();
				else
				{
					var points = pinGod.AwardJackpot(true);
					pinGod.PlayVoice("nice_kit"); //todo: show jackpot??
					//pinGod.PlayTvScene("kitt_jump", "JACKPOT\n" + points.ToScoreString(), 1.5f, loop: false); //todo: use somewhere else
				}
			}
			else if (pinGod.SwitchOff("target_truck", @event)) { }

			if (pinGod.SwitchOn("truck_3", @event))
			{
				if (!pinGod.IsMultiballRunning)
				{
					currentLock = 3;
					_truckLockSwitchTimer.Stop();
					_truckLockSwitchTimer.Start();
				}
			}
			else if (pinGod.SwitchOff("truck_3", @event)) { }

			if (pinGod.SwitchOn("truck_2", @event))
			{
				if (!pinGod.IsMultiballRunning)
				{
					currentLock = 2;
					_truckLockSwitchTimer.Stop();
					_truckLockSwitchTimer.Start();
				}
			}
			else if (pinGod.SwitchOff("truck_2", @event)) { }

			if (pinGod.SwitchOn("truck_1", @event))
			{
				if (!pinGod.IsMultiballRunning)
				{
					currentLock = 1;
					_truckLockSwitchTimer.Stop();
					_truckLockSwitchTimer.Start();
				}
			}
			else if (pinGod.SwitchOff("truck_1", @event)) { }

			if (pinGod.SwitchOn("truck_0", @event))
			{
				if (!pinGod.IsMultiballRunning)
				{
					currentLock = 0;
					_truckLockSwitchTimer.Stop();
					_truckLockSwitchTimer.Start();
				}
			}
			else if (pinGod.SwitchOff("truck_0", @event)) { }

			if (pinGod.SwitchOn("saucer_truck", @event))
			{
				ProcessTruckSaucer();
				pinGod.PlayLampshowFlash();
			}
			else if (pinGod.SwitchOff("saucer_truck", @event)) { }
		}
	}	

	private void _on_Timer_timeout()
	{
		ProcessTruckTarget(currentLock);
	}

	/// <summary>
	/// TODO: add to mode grop
	/// </summary>
	void OnBallStarted()
	{
		_player = pinGod.Player as KnightRiderPlayer;
		var enabled = pinGod.EnableTruckRamp();
		pinGod.LogInfo($"pursuit: Truck ramp open? {enabled}");
		_player.JackpotAdded = 0;
	}

	void ProcessTruckSaucer()
	{
		pinGod.LogInfo($"pursuit: truck target hit");
		pinGod.AddPoints(Constant.SCORE_STD);
		if (_player != null)
		{
			if (_player.AnyModeIsRunning() || pinGod.IsMultiballRunning)
			{
				pinGod.LogInfo($"pursuit: mode already running exiting saucer");
				pinGod.SolenoidPulse("saucer_truck");
				pinGod.PlayVoice("stop_this");
				return;
			}

			if (_player.IsVideoModeLit)
			{
				pinGod.LogInfo($"pursuit: starting video mode: todo");
				var game = GetParent().GetParent() as Game;
				if(game != null)
				{
					game.AddVideoMode();
					_player.IsVideoModeLit = false;
				}
				return;
			}

			if (_player.IsSuperPursuitMode)
			{
				if (_player.TruckLocks[0] == 0)
				{
					_player.TruckLocks[0] = 2;
					pinGod.PlayVoice("Micheal03");
					pinGod.AddPoints(Constant.SCORE_250K);
					_player.IsTruckRampReady = true;
					pinGod.LogInfo($"pursuit: truck lock 1, opening ramp");
					pinGod.PlayTvScene("truck_entry", Tr("TRUCK_LOCK").Replace("{VAL}","1"), 2f, loop: false);
				}
				else if (_player.TruckLocks[1] == 2)
				{
					pinGod.PlayVoice("DEVONstrongestcar");
					pinGod.AddPoints(Constant.SCORE_500K);
					_player.IsTruckRampReady = true;
					pinGod.LogInfo($"pursuit: truck lock 2, opening ramp");
					pinGod.PlayTvScene("truck_entry", Tr("TRUCK_LOCK").Replace("{VAL}", "2"), 2f, loop: false);
				}
				else if (_player.TruckLocks[2] == 2)
				{
					pinGod.PlayVoice("Micheal09");
					pinGod.AddPoints(Constant.SCORE_1MIL);
					_player.IsTruckRampReady = true;
					pinGod.LogInfo($"pursuit: truck lock 3, opening ramp");
					pinGod.PlayTvScene("truck_entry", Tr("TRUCK_LOCK").Replace("{VAL}", "3"), 2f, loop: false);
				}
				else
				{
					pinGod.LogInfo($"pursuit: no locks ready, not pulling down the ramp");
				}

				pinGod.PlayLampshow();
				bool rampClosed = pinGod.EnableTruckRamp();
				pinGod.LogInfo($"truck ramp closed? {rampClosed}");
			}
			else
			{
				pinGod.PlayVoice("stop_this");
			}

			pinGod.SolenoidPulse("saucer_truck");
		}
	}

	/// <summary>
	/// The target inside of the truck when ramp is down
	/// </summary>
	private void ProcessTruckTarget(int index = 0)
	{
		pinGod.LogInfo("pursuit: truck lock target hit index: " + index);		

		if (pinGod.IsMultiballRunning || _player.IsKarrRunning || !_player.IsTruckRampReady)
		{
			pinGod.SolenoidPulse("truck_ramp_diverter");
			pinGod.AddPoints(Constant.SCORE_STD);
			pinGod.LogInfo("pursuit: mode running, exit");
			return;
		}

		//get the saucer last changed for quick lock points
		var saucerLast = pinGod.GetLastSwitchChangedTime("saucer_truck");
		if (saucerLast < 6000)
		{
			pinGod.LogInfo("quick lock shot.");
			pinGod.AddPoints(Constant.SCORE_1MIL * 2);
			pinGod.PlayVoice("nice_kit");
		}

		//disable ramp
		_player.IsTruckRampReady = false;
		pinGod.EnableTruckRamp();
		pinGod.LogInfo("pursuit: disabling ramp");

		var currentLock = 3;
		//set the truck lot state to one if ready (2)
		for (int i = 0; i < _player.TruckLocks.Length; i++)
		{
			if (_player.TruckLocks[i] == 2)
			{
				_player.TruckLocks[i] = 1;
				currentLock = i + 1;
				if (currentLock < _player.TruckLocks.Length)
					_player.TruckLocks[currentLock] = 2;
				break;
			}
		}

		if (!_player.TruckLocks.Any(x => x == 2))
		{
			pinGod.LogInfo("pursuit: starting multiball");
			var game = GetParent().GetParent() as Game;
			if (game != null)
			{
				StartMultiball(game);
			}
		}
		else
		{
			var lockMsg = Tr("BALL_LOCKED").Replace("{VAL}", currentLock.ToString());
			pinGod.LogInfo(lockMsg);
			pinGod.PlayTvScene("truck_entry", lockMsg, 2f, loop: false);

			//play voice
			if (currentLock == 1) { pinGod.PlayVoice("Bonnie02"); }
			else if (currentLock == 2) { pinGod.PlayVoice("Micheal05"); }

			if (pinGod.SwitchOn("truck_3") && pinGod.SwitchOn("truck_2") && pinGod.SwitchOn("truck_1") && pinGod.SwitchOn("truck_0"))
			{
				pinGod.SolenoidPulse("truck_diverter", 171);
				pinGod.LogInfo("pursuit: letting ball free from lock");
			}
			else
			{
				//add ball lock to trough
				pinGod._trough.BallsLocked++;
				pinGod._trough.PulseTrough();
			}
		}

		UpdateLamps();
		pinGod.PlayLampshowFlash();
	}

	private void StartMultiball(Game game)
	{
		pinGod.PlayVoice("kr_pursuit_mode");
		game.StartSuperPursuitMultiball();
		UpdateLamps();
	}

	void UpdateLamps()
	{
		if (_player != null)
		{
			for (int i = 0; i < _player.TruckLocks.Length; i++)
			{
				//blink the first lamp
				if (i == 0 && _player.IsSuperPursuitMode && _player.TruckLocks[0] == 0)
				{
					pinGod.SetLampState(_lockLamps[i], 2);
				}
				else
				{
					pinGod.SetLampState(_lockLamps[i], _player.TruckLocks[i]);
				}
			}

			if (pinGod.IsMultiballRunning) //todo lamps not working?
			{
				for (int i = 0; i < 4; i++) { pinGod.SetLampState("jackpot_" + i, 2); }
			}
			else
			{
				for (int i = 0; i < 4; i++) { pinGod.SetLampState("jackpot_" + i, 0); }
			}
		}
	}
}
