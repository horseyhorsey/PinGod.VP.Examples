using Godot;
using System.Linq;

public class KittRampMode : Node
{
	private KrPinGodGame _pinGod;
	private KnightRiderPlayer _player;
	[Export] Godot.Collections.Array<AudioStreamSample> _voices = new Godot.Collections.Array<AudioStreamSample>();
	private AudioStreamPlayer _audio;

	public override void _EnterTree()
	{
		base._EnterTree();
		_pinGod = GetNode<KrPinGodGame>("/root/PinGodGame");
		_audio = GetNode<AudioStreamPlayer>(nameof(AudioStreamPlayer));		
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (!_pinGod.GameInPlay || _pinGod.IsTilted) return;

		if(_pinGod.SwitchOn("ramp_kitt", @event))
		{
			if(_player != null)
			{
				if (_pinGod.IsMultiballRunning)
				{
					_pinGod.AwardJackpot();
				}
				else if (_player.BillionShotLit)
				{
					_pinGod.AwardBillion();					
				}
				else
				{
					if (_player.IsKittRunning)
					{
						_pinGod.LogInfo("kitt ramp: award");
						if (_player.KittTargets.Any(x => x == 2))
						{
							for (int i = 0; i < _player.KittTargets.Length; i++)
							{
								if (_player.KittTargets[i] == 2)
								{
									_player.KittTargets[i] = 1;
									AwardScoreForRamp(i);
									GetParent().GetNode("UpperPF").GetNode<AudioStreamPlayer2D>("KittRampMusic").Stop();
									break;
								}
							}
						}
						else
						{
							//play kitt05 sound
							_audio.Stream = _voices[1]; _audio.Play();

							if (_player.KittCompleteCount > 0)
							{								
								var points = Constant.SCORE_1MIL * _player.KittCompleteCount+1;
								_pinGod.AddPoints(points);
								_pinGod.PlayTvScene("kitt_intro_headon", $"RAMP VALUE\n{_player.KittCompleteCount+1} MILLION", loop: false);
							}
							else
							{
								_pinGod.AddPoints(Constant.SCORE_250K);
								_pinGod.PlayTvScene("kitt_intro_headon", "RAMP VALUE\n250K", loop: false);
							}

							GetParent().GetNode("UpperPF").GetNode<AudioStreamPlayer2D>("KittRampMusic").Stop();
						}
					}
				}
			}            
		}
		else if (_pinGod.SwitchOff("ramp_kitt", @event)) { }
	}

	private void AwardScoreForRamp(int i)
	{		
		_audio.Stream = _voices[0];_audio.Play();
		switch (i)
		{
			case 0:
				_pinGod.AddPoints(Constant.SCORE_1MIL * 10);
				ResetKittActive();
				_pinGod.LogInfo("ramp: 10 million");
				_pinGod.PlayTvScene("kitt_turboboost", "TURBO 10 MILL", 6.5f, loop: false);
				break;
			case 1:
				_pinGod.AddPoints(Constant.SCORE_1MIL * 15);
				ResetKittActive();
				_pinGod.LogInfo("ramp: 15 million");
				_pinGod.PlayTvScene("kitt_turboboost", "TURBO 15 MILL", 5.5f, loop: false);
				break;
			case 2:
				_pinGod.AddPoints(Constant.SCORE_1MIL * 20);
				ResetKittActive();
				_pinGod.LogInfo("ramp: 20 million");
				_pinGod.PlaySfx("KITT08");
				_pinGod.PlayTvScene("kitt_turboboost", "TURBO 20 MILL\nVIDEO MODE LIT", 5.5f, loop: false);
				_player.IsVideoModeLit = true;
				_pinGod.UpdateLamps((GetParent().GetParent() as Game).GetTree());
				break;
			case 3:
				_pinGod.AddPoints(Constant.SCORE_1MIL * 100);
				ResetKittActive();
				_pinGod.LogInfo("ramp: 100 million");
				_pinGod.PlayTvScene("kitt_turboboost", "TURBO 100 MILL", 5.5f, loop: false);
				break;
			case 4:
				_pinGod.AddPoints(Constant.SCORE_1MIL * 100);
				_pinGod.LogInfo("ramp: 100 million");
				_pinGod.PlayTvScene("kitt_turboboost", "TURBO 100 MILL - COMPLETE", 5.5f, loop: false);
				_player.KittCompleteReady = true;
				_player.IsKittRunning = false;
				_player.KittCompleteCount++;

				//light the billion shot if all completed
				_player.BillionShotLit = _player.AllModesComplete();
				var game = GetParent().GetParent() as Game;
				_pinGod.UpdateLamps(game.GetTree());
				return;
			default:
				break;
		}

		UpdateLamps();
	}

	private void ResetKittActive()
	{
		_player.ResetKittActive();
		UpdateLamps();
	}

	void OnBallStarted()
	{
		_player = _pinGod.Player as KnightRiderPlayer;
	}

	void UpdateLamps()
	{
		if(_player != null)
		{
			for (int i = 0; i < _player.KittTargets.Length; i++)
			{
				_pinGod.SetLampState("ramp_"+i, _player.KittTargets[i]);
			}
		}
	}
}
