using Godot;

public class BaseMode : Control
{
	[Export] string BALL_SAVE_SCENE = "res://addons/PinGodGame/Modes/ballsave/BallSave.tscn";
	private KrPinGodGame pinGod;
	private Game game;
	private PackedScene _ballSaveScene;
	private KnightRiderPlayer _player;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as KrPinGodGame;
		game = GetParent().GetParent() as Game;

		_ballSaveScene = GD.Load<PackedScene>(BALL_SAVE_SCENE);
	}

	/// <summary>
	/// Basic input switch handling
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (!pinGod.GameInPlay) return;
		if (pinGod.IsTilted) return; //game is tilted, don't process other switches when tilted

		if (pinGod.SwitchOn("start", @event))
		{
			pinGod.LogInfo("base: start button adding player...", pinGod.StartGame());
		}
		if (pinGod.SwitchOn("outlane_l", @event))
		{
			pinGod.AddPoints(Constant.SCORE_MIN);

			if (_player != null)
			{
				if (_player.SpecialLanes[0])
				{					
					_player.SpecialLanes[0] = false;
					pinGod.AddPoints(Constant.SCORE_1MIL*10);
					pinGod.PlayTvScene("kitt_birds", "SPECIAL 10 MILLION", loop: false);
					pinGod.PlaySfx("mike_laughs");
                }
                else
                {
					pinGod.PlaySfx("micheal");
				}
			}
			//kickback
			if (pinGod.KickbackEnabled)
			{
				pinGod.SolenoidPulse("kickback");
				pinGod.KickbackEnabled = false;
			}
		}
		if (pinGod.SwitchOn("inlane_l", @event))
		{
			pinGod.AddPoints(Constant.SCORE_MIN);
		}
		if (pinGod.SwitchOn("inlane_r", @event))
		{
			pinGod.AddPoints(Constant.SCORE_MIN);
		}
		if (pinGod.SwitchOn("outlane_r", @event))
		{
			pinGod.AddPoints(Constant.SCORE_MIN);
			if (_player != null)
			{
				//score special R
				if (_player.SpecialLanes[1])
				{
					pinGod.AddPoints(Constant.SCORE_1MIL);
					
					_player.SpecialLanes[1] = false;
                }
				else
				{
					pinGod.PlaySfx("micheal");
				}
			}			
		}
		if (pinGod.SwitchOn("sling_l", @event))
		{
			pinGod.AddPoints(Constant.SCORE_MIN / 2);
			pinGod.PlaySfx("gun02");
			pinGod.SolenoidPulse("flash_l");
		}
		if (pinGod.SwitchOn("sling_r", @event))
		{
			pinGod.AddPoints(Constant.SCORE_MIN / 2);
			pinGod.PlaySfx("Gun03");
			pinGod.SolenoidPulse("flash_r");
		}
		if (pinGod.SwitchOn("bumper_0", @event))
		{
			pinGod.AddPoints(Constant.SCORE_STD / 2);
		}
		if (pinGod.SwitchOn("bumper_1", @event))
		{
			pinGod.AddPoints(Constant.SCORE_STD / 2);
		}
		if (pinGod.SwitchOn("bumper_2", @event))
		{
			pinGod.AddPoints(Constant.SCORE_STD);
		}
	}

	public void OnBallDrained() 
	{
		pinGod.KickbackEnabled = true;
	}
	public void OnBallSaved() 
	{
		pinGod.LogDebug("base: ball_saved");
		if (!pinGod.IsMultiballRunning)
		{
			pinGod.LogDebug("ballsave: ball_saved");
			//add ball save scene to tree and remove after 2 secs;
			CallDeferred(nameof(DisplayBallSaveScene), 2f);
		}
		else
		{
			pinGod.LogDebug("skipping save display in multiball");
		}
	}
	public void OnBallStarted() 
	{
		_player = pinGod.Player as KnightRiderPlayer;
		pinGod.UpdateLamps(GetTree());
	}
	public void UpdateLamps() 
	{
		if(_player != null)
		{
			pinGod.SetLampState("complete_karr", _player.KarrCompleteReady ? (byte)1 : (byte)0);
			pinGod.SetLampState("complete_kitt", _player.KittCompleteReady ? (byte)1 : (byte)0);			
			pinGod.SetLampState("complete_truck", _player.TruckCompleteReady ? (byte)1 : (byte)0);
			pinGod.SetLampState("complete_all", _player.BillionShotLit ? (byte)2 : (byte)0);
            if (_player.BillionShotLit)
            {
				pinGod.SetLampState("jackpot_0", 2);
				pinGod.SetLampState("jackpot_1", 2);
				pinGod.SetLampState("jackpot_2", 2);
				pinGod.SetLampState("jackpot_3", 2);
			}
            else
            {
				pinGod.SetLampState("jackpot_0", 0);
				pinGod.SetLampState("jackpot_1", 0);
				pinGod.SetLampState("jackpot_2", 0);
				pinGod.SetLampState("jackpot_3", 0);
			}
		}
	}

	/// <summary>
	/// Adds a ball save scene to the tree and removes
	/// </summary>
	/// <param name="time">removes the scene after the time</param>
	private void DisplayBallSaveScene(float time = 2f)
	{
		var ballSaveScene = _ballSaveScene.Instance<BallSave>();
		ballSaveScene.SetRemoveAfterTime(time);
		AddChild(_ballSaveScene.Instance());
	}
}
