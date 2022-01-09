using Godot;
using Godot.Collections;
using System;
using System.Linq;

public class KarrMode : Node
{
	private string[] _karrLamps = new string[5] { "karr_0", "karr_1", "karr_2", "karr_3", "karr_4" };
	private float _karrModeCurrentTime;
	private float _karrModeTimeTotal;
	private Label _label;
	private KrPinGodGame _pinGod;
	private KnightRiderPlayer _player;

	public override void _EnterTree()
	{
		base._EnterTree();
		_pinGod = GetNode<KrPinGodGame>("/root/PinGodGame");		
		_label = GetNode<Label>("Label"); _label.Visible = false;
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (!_pinGod.GameInPlay || _pinGod.IsTilted) return;

		if (_player?.IsKarrRunning ?? false)
		{
			if (_pinGod.SwitchOn("karr_loop_0", @event)) { ProcessKarrShot(0); }
			if (_pinGod.SwitchOn("karr_loop_1", @event)) { ProcessKarrShot(1); }
			if (_pinGod.SwitchOn("karr_loop_2", @event)) { ProcessKarrShot(2); }
			if (_pinGod.SwitchOn("scoop_ramp", @event)) { ProcessKarrShot(3); _pinGod.SolenoidPulse("scoop_ramp"); }
			if (_pinGod.SwitchOn("karr_loop_3", @event)) { ProcessKarrShot(4); }
		}
		else
		{
			ulong lOrbitLastHit = 0; ulong rOrbitLastHit = 0;
			if (_pinGod.IsMultiballRunning)
			{
				lOrbitLastHit = _pinGod.GetLastSwitchChangedTime("karr_loop_1");
				rOrbitLastHit = _pinGod.GetLastSwitchChangedTime("karr_loop_3");
			}

			//left inner
			if (_pinGod.SwitchOn("karr_loop_0", @event))
			{
				if (!_pinGod.IsMultiballRunning)
				{
					_pinGod.AddPoints(Constant.SCORE_STD * 2);
					_pinGod.SolenoidPulse("flash_mid_l", 190);
					PlaySfx(4);
					_pinGod.PlayTvScene("kitt_corn", "", 1.5f, loop: false);
				}
				else
				{
					_pinGod.AddPoints(Constant.SCORE_STD * 3);
					_pinGod.PlaySfx("motor01");
				}				
			}

			//left orbit
			if (_pinGod.SwitchOn("karr_loop_1", @event))
			{
				if (!_pinGod.IsMultiballRunning)
				{
					if (_player != null)
					{						
						_pinGod.SolenoidPulse("flash_top_l", 190);

						if (_player.BillionShotLit)
						{
							_pinGod.AwardBillion();
							_player.ResetCompletedModes();
						}
                        else
                        {
							_pinGod.AddPoints(Constant.SCORE_STD * 2);
						}
					}
				}
				else
				{
					if(rOrbitLastHit > 2000)
					{
						_pinGod.AwardJackpot();
					}					
				}			
			}

			//center inter, two wheels
			if (_pinGod.SwitchOn("karr_loop_2", @event))
			{
				if (!_pinGod.IsMultiballRunning)
				{
					var points = Constant.SCORE_STD * 3;
					_pinGod.AddPoints(points);
					_pinGod.SolenoidPulse("flash_top_l", 190);
					_pinGod.SolenoidPulse("flash_top_r", 190);
					PlaySfx(4);
					_pinGod.PlayTvScene("kitt_two_wheel", Tr("TWO_WHEELS") + $"\n{points.ToScoreString()}", 1.5f, loop: false);
				}
				else
				{
					_pinGod.AddPoints(Constant.SCORE_STD * 3);
				}
			}

			//scoop ramp
			if (_pinGod.SwitchOn("scoop_ramp", @event))
			{
				if (!_pinGod.IsMultiballRunning)
				{
					var points = Constant.SCORE_STD * 2;
					_pinGod.AddPoints(points);
					_pinGod.SolenoidPulse("flash_top_r", 81);
					_pinGod.SolenoidPulse("flash_mid_r", 162);
					_pinGod.SolenoidPulse("scoop_ramp");
					_pinGod.PlayTvScene("kitt_jump", Tr("JUMP_SCORED") + $"\n{points.ToScoreString()}", 1.5f, loop: false);

					if (_player != null)
					{
						if (_player.BillionShotLit)
						{
							_pinGod.AwardBillion();
							_player.ResetCompletedModes();
						}
					}
				}
				else
				{
					var points = _pinGod.AwardJackpot();
					_pinGod.PlayTvScene("kitt_jump", Tr("JACKPOT") + $"\n{points.ToScoreString()}", 1.5f, loop: false);
					_pinGod.SolenoidPulse("scoop_ramp");
				}
			}

			//right orbit
			if (_pinGod.SwitchOn("karr_loop_3", @event))
			{
				if (!_pinGod.IsMultiballRunning)
				{
					_pinGod.AddPoints(Constant.SCORE_STD * 2);
					_pinGod.SolenoidPulse("flash_top_r", 81);

					if (_player != null)
					{
						if (_player.BillionShotLit)
						{
							_pinGod.AwardBillion();
							_player.ResetCompletedModes();
						}
					}
				}
				else
				{
					if (lOrbitLastHit > 2000)
					{
						_pinGod.AwardJackpot();
					}
				}
			}
		}
	}

	public override void _Ready()
	{
		base._Ready();
		_pinGod.Connect(nameof(KrPinGodGame.StartKarrTimer), this, nameof(OnStartTimer));
	}

	public void OnStartTimer(float time = 30f)
	{
		_pinGod.LogDebug("karr mode timer started");
		_karrModeTimeTotal = time;
		_karrModeCurrentTime = time;
		_label.Text = Math.Round(time, 0).ToString(); _label.Visible = true;
		_pinGod.PlayVoice("KARR04");
		_pinGod.EnableTruckRamp();
		GetNode<Timer>("Timer").Start();
	}

	public void StopTimer()
	{
		GetNode<Timer>("Timer").Stop();
		_label.Visible = false;
	}

	private void _on_Timer_timeout()
	{
		_pinGod.LogDebug("karr mode: timed out. resetting");
		if(_karrModeCurrentTime <= 0)
		{
			_player.ResetKarrActive();
			_label.Visible = false;
			StopTimer();
			_pinGod.PlayTvScene("kitt_corn", Tr("KARR_FAILED"), 2f, loop: false);
			_pinGod.PlayVoice("KARR03");
			_pinGod.EnableTruckRamp();
			UpdateLamps();
		}
		else
		{
			_karrModeCurrentTime-=1f;
			_label.Text = Math.Round(_karrModeCurrentTime, 0).ToString();
		}
	}

	private void AwardKarr()
	{
		_pinGod.AddPoints(25000000);
		_player.ExtraBallLit = true;
		StopTimer();
		_pinGod.PlayLampshow();
		_pinGod.PlayLampshowFlash();
		_pinGod.PlayVoice("KITT04");
		_pinGod.LogDebug($"karr mode awarded. todo: KarrDefeated");
		_pinGod.PlayTvScene("kitt_dash", Tr("KARR_DEFEATED"), 2f, loop: false);
		_player.ResetKarrActive();		
	}

	private bool IsKarrComplete()
	{
		if (!_player.KarrTargets.Any(x => x == 2))
		{			
			_player.KarrCompleteCount++;

			if(_player.ExtraBallsAwarded < 2)
			{
				_player.ExtraBallLit = true;
				_pinGod.LogInfo($"karr mode lighting extra ball lit");
			}
			else
			{
				_pinGod.AddPoints(Constant.SCORE_1MIL);
			}
			
			_player.KarrCompleteReady = true;
			_player.IsKarrRunning = false;
			
			_player.BillionShotLit = _player.AllModesComplete();
			_pinGod.LogInfo($"karr mode complete-billion ready: {_player.BillionShotLit}");
			return true;
		}
		else
		{
			_pinGod.AddPoints(Constant.SCORE_250K);
			_pinGod.PlayLampshowFlash();
			return false;
		}		
	}

	void OnBallDrained()
	{
		StopTimer();
		_player.ResetKarrActive();
	}

	void OnBallStarted()
	{
		_player = _pinGod.Player as KnightRiderPlayer;
		_pinGod.LogDebug($"karrMode: ball started");
		_player.ResetKarrActive();
	}
	private void ProcessKarrShot(int index)
	{
		_pinGod.LogDebug($"karr: karr shot - {index}");

		if (_player.KarrTargets[index] == 2)
		{
			_player.KarrTargets[index] = 1;
			bool completed = IsKarrComplete();
			if (completed)
			{
				AwardKarr();

				//upate all lamps
				var game = GetParent().GetParent() as Game;
				_pinGod.UpdateLamps(game.GetTree());
			}
			else
			{
				_pinGod.PlayTvScene("karr_dash", Tr("KARR LANE COMPLETE"), 2f, loop: false);
				_pinGod.PlayVoice("KARR02");
				_pinGod.PlaySfx("krfx08");
				UpdateLamps();
			}

			_pinGod.LogDebug($"karr: karr mode complete - {completed}");			
		}
		else if (_player.KarrTargets[index] == 1)
		{
			_pinGod.LogDebug($"karr: karr shot {index} already made");
			_pinGod.AddPoints(Constant.SCORE_STD);

			PlaySfx(index);
		}
	}

	private void PlaySfx(int index)
	{
		//play sound depending on switch index hit
		switch (index)
		{
			case 0:
				_pinGod.PlaySfx("motor01");
				break;
			case 1:
			case 2:
			case 4:
				_pinGod.PlaySfx("krfx08");
				break;
			case 3:
				_pinGod.PlaySfx("TurboBoostShort");
				break;
			default:
				break;
		}
	}

	void UpdateLamps()
	{
		if(_player?.IsKarrRunning ?? false)
		{
			for (int i = 0; i < _player.KarrTargets.Length; i++)
			{
				_pinGod.SetLampState(_karrLamps[i], _player.KarrTargets[i]);
			}
		}
		else
		{
			for (int i = 0; i < _player.KarrTargets.Length; i++)
			{
				_pinGod.SetLampState(_karrLamps[i], 0);
			}
		}		
	}
}
