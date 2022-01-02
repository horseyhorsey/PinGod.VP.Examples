using Godot;
using System;

public class BaseMode : Control
{ 
	private PinGodGame pinGod;
	private Game game;
	private PackedScene _playawardScene;
	private PlayboyPlayer _player;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		game = GetParent().GetParent() as Game;		
		_playawardScene = ResourceLoader.Load("res://scenes/PlayboyAward.tscn") as PackedScene;
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
		if (pinGod.SwitchOn("flipper_l", @event))
		{
		}
		if (pinGod.SwitchOn("flipper_r", @event))
		{
		}
		if (pinGod.SwitchOn("outlane_l", @event))
		{
			pinGod.AddPoints(Game.MINSCORE);
            if (!pinGod.BallSaveActive)
            {
				AudioServer.SetBusEffectEnabled(1, 0, true);
			}
			if (_player.OutlanesLit)
			{
				pinGod.AddPoints(25000);
				_player.OutlanesLit = false;
			}
		}
		if (pinGod.SwitchOn("inlane_l", @event))
		{
			pinGod.AddPoints(Game.MINSCORE);
			pinGod.PlaySfx("horse-disco-pop");
		}
		if (pinGod.SwitchOn("inlane_r", @event))
		{
			pinGod.AddPoints(Game.MINSCORE);
			pinGod.PlaySfx("horse-disco-pop");
		}
		if (pinGod.SwitchOn("outlane_r", @event))
		{
			pinGod.AddPoints(Game.MINSCORE);
			if (!pinGod.BallSaveActive)
			{
				AudioServer.SetBusEffectEnabled(1, 0, true);
			}
			if (_player.OutlanesLit)
            {
				pinGod.AddPoints(25000);
				_player.OutlanesLit = false;
			}				
		}
		if (pinGod.SwitchOn("sling_l", @event))
		{
			pinGod.AddPoints(Game.MINSCORE);
		}
		if (pinGod.SwitchOn("sling_r", @event))
		{
			pinGod.AddPoints(Game.MINSCORE);
		}
		if (pinGod.SwitchOn("loop_r", @event))
		{			
			ProcessRightLoop();
		}
		if (pinGod.SwitchOn("bumper_1", @event))
		{
			pinGod.AddPoints(Game.MINSCORE);
			pinGod.PlaySfx("horse-disco-pop");
		}
		if (pinGod.SwitchOn("bumper_2", @event))
		{
			pinGod.AddPoints(Game.MINSCORE);
			pinGod.PlaySfx("horse-disco-pop");
		}
		if (pinGod.SwitchOn("bumper_3", @event))
		{
			pinGod.AddPoints(Game.MINSCORE);
			pinGod.PlaySfx("horse-disco-pop");
		}
		if (pinGod.SwitchOn("extra_ball", @event))
		{
			_player?.AdvanceBonus(1);

			if (_player != null)
			{
				pinGod.AddPoints(Game.MINSCORE);
				if (_player.LeftSpecialLit)
				{
					pinGod.AddPoints(25000);
					pinGod.PlaySfx("horse-disco-laser");
					_player.LeftSpecialLit = false;
				}
				if (_player.ExtraBallLit)
				{
					_player.ExtraBalls++;
					_player.ExtraBallLit = false;
					pinGod.PlaySfx("horse-disco-laser");
					AddPlayboyAwardScene("EXTRA BALL");
				}
			}

			UpdateLamps();
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		pinGod.StopMusic();
	}

	private void ProcessRightLoop()
	{
		_player?.AdvanceBonus(1);
		pinGod.PlaySfx("horse-disco-laser");
		if (_player != null)
		{
			pinGod.LogInfo("base: advanced bonus - " + _player.BonusTimes);

			if (_player.SpecialRightLit)
			{
				pinGod.AddPoints(5000); //5k special				
			}
		}
		else { pinGod.AddPoints(Game.MINSCORE); }

		UpdateLamps();
	}

	private void StartMultiball()
	{
		if (!pinGod.IsMultiballRunning)
		{
			pinGod.IsMultiballRunning = true;
			pinGod.SolenoidPulse("mball_saucer");
			game?.CallDeferred("AddMultiballSceneToTree");            
		}
		else
		{
			//already in multiball
			pinGod.SolenoidPulse("mball_saucer");
		}
	}

	public void OnBallStarted()
    {
        _player = pinGod.Player as PlayboyPlayer;
		_player.Reset();

        PlaymusicForScore();
    }

    private void PlaymusicForScore()
    {
        if (_player.Points > 68999)
            pinGod.PlayMusic("cook_loop_2");
        else
            pinGod.PlayMusic("cook_loop_1");
    }

    public void OnBallDrained()
	{
		pinGod.LogInfo("base: ball drained");
		AudioServer.SetBusEffectEnabled(1, 0, false);
		pinGod.StopMusic();
	}

	public void OnBallSaved()
	{
		if (!pinGod.IsMultiballRunning)
		{
			pinGod.LogDebug("ballsave: ball_saved");
			CallDeferred(nameof(AddPlayboyAwardScene), "BALL SAVED");
		}
		else
		{
			pinGod.LogDebug("skipping save display in multiball");
		}
	}

	/// <summary>
	/// todo: use this scene elsewhere so can be used other modes
	/// </summary>
	/// <param name="awardText"></param>
	private void AddPlayboyAwardScene(string awardText = "BALL SAVED")
	{
		var scene = _playawardScene.Instance() as PlayboyAward;
		scene.SetAwardText(awardText);
		AddChild(scene);
	}

	public void UpdateLamps() 
	{
		if(_player != null)
		{
			pinGod.SetLampState("arrow_r", _player.SpecialRightLit ? (byte)1 : (byte)0);
			pinGod.SetLampState("arrow_special", _player.LeftSpecialLit ? (byte)1 : (byte)0);

            if (_player.OutlanesLit)
            {
				pinGod.SetLampState("left_25k", 1);
				pinGod.SetLampState("special_r", 1);
			}
            else
            {
				pinGod.SetLampState("left_25k", 0);
				pinGod.SetLampState("special_r", 0);
			}
			
			pinGod.SetLampState("extra_ball", _player.ExtraBallLit ? (byte)1 : (byte)0);

			game.UpdateBonusLamps();			
		}
	}
}
