using Godot;
using System;
using System.Linq;

public class UpperPfMode : Node
{
	private AudioStreamPlayer _audio;
	private Game _game;
	private KnightRiderPlayer _player;
	private AudioStreamSample[] _streams;
	private Timer _vukTimer;
	private KrPinGodGame pinGod;

	public override void _EnterTree()
	{
		base._EnterTree();
		pinGod = GetNode<KrPinGodGame>("/root/PinGodGame");
		_audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		_game = GetParent().GetParent() as Game;
		_vukTimer = GetNode<Timer>("Timer");
		_streams = new AudioStreamSample[9]
		{
			GD.Load<AudioStreamSample>("res://assets/audio/sfx/KRfx04.wav"),
			GD.Load<AudioStreamSample>("res://assets/audio/sfx/tuuut.wav"),
			GD.Load<AudioStreamSample>("res://assets/audio/sfx/Beep.wav"),
			GD.Load<AudioStreamSample>("res://assets/audio/sfx/Beep02.wav"),
			GD.Load<AudioStreamSample>("res://assets/audio/voice/kitt/KITT06.wav"),
			GD.Load<AudioStreamSample>("res://assets/audio/voice/kitt/KITT01.wav"),
			GD.Load<AudioStreamSample>("res://assets/audio/voice/kitt/KITT09.wav"),
			GD.Load<AudioStreamSample>("res://assets/audio/voice/kitt/KITTclutch.wav"),
			GD.Load<AudioStreamSample>("res://assets/audio/voice/mike/Micheal04.wav"),
		};
	}

	public override void _Input(InputEvent @event)
	{
		if (!pinGod.GameInPlay) return;
		if (pinGod.IsTilted) return;

		if (pinGod.SwitchOn("vuk_entry", @event))
		{
			OnVukEntered();
		}
		//NOT USED?
		//if (pinGod.SwitchOn("lower_pf_entry", @event))
		//{
		//	pinGod.EnableFlippers(1);
		//}
		if (pinGod.SwitchOff("target_a", @event)) { ProcessKittTarget(); } else if (pinGod.SwitchOff("target_a", @event)) { }
		if (pinGod.SwitchOn("target_b", @event)) { ProcessKarrTarget(); } else if (pinGod.SwitchOff("target_b", @event)) { }
		if (pinGod.SwitchOn("target_c", @event)) { ProcessTruckTarget(); } else if (pinGod.SwitchOff("target_c", @event)) { }
		if (pinGod.SwitchOn("sling_top_l", @event))
		{
			pinGod.AddPoints(Constant.SCORE_MIN);
			_audio.Stream = _streams[0]; _audio.Play();
		}
		if (pinGod.SwitchOn("sling_top_r", @event))
		{
			pinGod.AddPoints(Constant.SCORE_MIN);
			_audio.Stream = _streams[1]; _audio.Play();
		}
	}

	private void _on_Timer_timeout()
	{
		pinGod.SolenoidPulse("vuk_entry");
	}

	private void activate_next_karr()
	{
		pinGod.EnableTopFlippers(0);
		pinGod.PlayLampshow();
		StartKarrMode();
	}

	private void activate_next_kitt()
	{
		_player.IsKittRunning = true;
		GetNode<AudioStreamPlayer2D>("KittRampMusic").Play();
		pinGod.LogDebug("kit target: IsKittRunning=true, ramp for jackpot...");
		pinGod.EnableTopFlippers(0);
		for (int i = 0; i < _player.KittTargets.Length; i++)
		{
			if (_player.KittTargets[i] == 0)
			{
				_player.KittTargets[i] = 2;
				PlayKitReadyScene(i);
				break;
			}
		}

		pinGod.PlayLampshow();
		//update all lamps in game tree
		pinGod.UpdateLamps(GetParent().GetParent().GetTree());
	}

	private void activate_truck_locks()
	{
		pinGod.EnableTopFlippers(0);
		if (_player != null)
		{
			_audio.Stream = _streams[8]; //micheal04 sound
			_audio.Play();
			pinGod.AddPoints(Constant.SCORE_STD);
			pinGod.PlayMusic("KRsuspense");
			StartPursuitLocks();
			pinGod.PlayTvScene("truck_entry", "TRUCK INITIATED", 2f, loop: false);
		}

		//update all lamps in game tree
		pinGod.UpdateLamps(GetParent().GetParent().GetTree());
	}

	/// <summary>
	/// Adds score
	/// </summary>
	void HitTarget()
	{
		if (pinGod.IsMultiballRunning)
		{
			pinGod.AddPoints(15000, false);
			_player.JackpotAdded += Constant.SCORE_STD;
		}
		else pinGod.AddPoints(5000);
	}

	void OnBallDrained()
	{
		GetNode<AudioStreamPlayer2D>("KittRampMusic").Stop();
		pinGod.EnableTopFlippers(0);
	}

	void OnBallStarted()
	{
		_player = pinGod.Player as KnightRiderPlayer;
		//todo: loop car engine. startloop
		StartPursuitLocks();
		StartKittRamp();
		pinGod.PlayLampshow();
	}

	private void OnVukEntered()
	{
		_vukTimer.Stop();
		pinGod.PlayLampshow();
		pinGod.AddPoints(Constant.SCORE_MIN / 2);
		if (_player != null)
		{
			//disable flippers or enable depending if the modes are running or not
			UpdateLamps();
			if (_player.IsKarrRunning || _player.IsKittRunning) pinGod.EnableTopFlippers(0);
			else 
			{ 
				pinGod.EnableTopFlippers(1);
				_audio.Stream = _streams[1]; _audio.Play();
			}
		}

		_vukTimer.Start();
	}
	private void PlayKitReadyScene(int index)
	{
		pinGod.PlayMusic("KRfx12");
		pinGod.LogDebug($"kit target: kitt ready {index}");

		var points = 0;
		switch (index)
		{
			case 0:
				points = Constant.SCORE_STD;
				pinGod.AddPoints(points);
				_audio.Stream = _streams[5]; _audio.Play();
				break;
			case 1:
				points = Constant.SCORE_500K;
				pinGod.AddPoints(points);
				_audio.Stream = _streams[6]; _audio.Play();
				break;
			case 2:
				points = Constant.SCORE_500K * 10;
				pinGod.AddPoints(points);
				_audio.Stream = _streams[7]; _audio.Play();
				break;
			case 3:
				points = Constant.SCORE_1MIL * 10;
				pinGod.AddPoints(points);
				_audio.Stream = _streams[5]; _audio.Play();
				break;
			default:
				break;
		}

		pinGod.PlayLampshow();
		pinGod.PlayLampshowFlash();
		pinGod.PlayTvScene("kitt_dash", $"{points.ToScoreString()} AWARDED\nRAMP SCORES JACKPOT", 2f, false);
	}

	private void ProcessKarrTarget()
	{
		HitTarget();
		_audio.Stream = _streams[3]; _audio.Play();
		if (!pinGod.IsMultiballRunning)
		{
			if (_player != null && !_player.AnyModeIsRunning())
			{
				pinGod.LogDebug("karr target: hit");
				if (_player.KarrActiveTargets[0] == 2) _player.KarrActiveTargets[0] = 1;
				else if (_player.KarrActiveTargets[1] == 2) _player.KarrActiveTargets[1] = 1;
				else if (_player.KarrActiveTargets[2] == 2)
				{
					_player.KarrActiveTargets[2] = 1;
					activate_next_karr();
				}
			}

			UpdateLamps();
		}		
	}

	private void ProcessKittTarget()
	{
		HitTarget();
		_audio.Stream = _streams[2]; _audio.Play();
		if (!pinGod.IsMultiballRunning)
		{
			//only move next if Kitt isn't running
			if (_player != null && !_player.AnyModeIsRunning())
			{
				pinGod.LogDebug("kit target: hit");
				if (_player.KittActiveTargets[0] == 2) _player.KittActiveTargets[0] = 1;
				else if (_player.KittActiveTargets[1] == 2) _player.KittActiveTargets[1] = 1;
				else if (_player.KittActiveTargets[2] == 2)
				{
					_player.KittActiveTargets[2] = 1;
					activate_next_kitt();
				}

				UpdateLamps();
			}
		}
	}

	private void ProcessTruckTarget()
	{
		HitTarget();
		_audio.Stream = _streams[2]; _audio.Play();
		if (!pinGod.IsMultiballRunning)
		{
			if (_player != null && !_player.AnyModeIsRunning())
			{
				pinGod.LogDebug("truck target: hit");
				if (_player.TruckActiveTargets[0] == 2) _player.TruckActiveTargets[0] = 1;
				else if (_player.TruckActiveTargets[1] == 2) _player.TruckActiveTargets[1] = 1;
				else if (_player.TruckActiveTargets[2] == 2)
				{
					_player.TruckActiveTargets[2] = 1;
					activate_truck_locks();
				}
				else
				{
					//pinGod.LogDebug("truck target: locks already open");
				}
			}
		}
	}

	private void StartKarrMode()
	{
		_player.IsKarrRunning = true;
		pinGod.PlayMusic("KRsuspense");

		var secs = 32f;
		pinGod.PlayTvScene("karr_start", $"DEFEAT KARR\n{(int)secs} SECONDS REMAIN", 4f, loop: false);
		pinGod.EmitSignal(nameof(KrPinGodGame.StartKarrTimer), secs);
		pinGod.UpdateLamps(GetParent().GetParent().GetTree());
		pinGod.LogDebug($"upper: karr started");
	}

	private void StartKittRamp()
	{
		if (_player.IsKittRunning)
		{
			GetNode<AudioStreamPlayer2D>("KittRampMusic").Play();
		}        
	}

	private void StartPursuitLocks()
	{
		if (_player != null && !_player.AnyModeIsRunning())
		{
			if(_player.IsTruckRampReady || _player.TruckActiveTargets.Any(x => x != 2))
			{
				pinGod.LogInfo("upper: pursuit started");				
				_player.IsSuperPursuitMode = true;				
				pinGod.UpdateLamps(_game.GetTree());						
			}
		}
	}
	void UpdateLamps()
	{
		if(_player != null)
		{
			for (int i = 0; i < _player.KittActiveTargets.Length; i++)
			{
				pinGod.SetLampState("a_" + i, _player.KittActiveTargets[i]);
				pinGod.SetLampState("b_" + i, _player.KarrActiveTargets[i]);
				pinGod.SetLampState("c_" + i, _player.TruckActiveTargets[i]);
			}
		}
	}
}
