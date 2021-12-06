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
		}
		if (pinGod.SwitchOn("inlane_l", @event))
		{
			pinGod.AddPoints(Game.MINSCORE);
		}
		if (pinGod.SwitchOn("inlane_r", @event))
		{
			pinGod.AddPoints(Game.MINSCORE);
		}
		if (pinGod.SwitchOn("outlane_r", @event))
		{
			pinGod.AddPoints(Game.MINSCORE);
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
		}
		if (pinGod.SwitchOn("bumper_2", @event))
		{
			pinGod.AddPoints(Game.MINSCORE);
		}
		if (pinGod.SwitchOn("bumper_3", @event))
		{
			pinGod.AddPoints(Game.MINSCORE);
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
					_player.LeftSpecialLit = false;
				}
				if (_player.ExtraBallLit)
				{
					_player.ExtraBalls++;
					_player.ExtraBallLit = false;
				}
			}
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
		if (_player != null)
		{
			pinGod.LogInfo("base: advanced bonus - " + _player.BonusTimes);

			if (_player.SpecialRightLit)
			{
				pinGod.AddPoints(Game.MINSCORE * 10); //5k special
				_player.SpecialRightLit = false;
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
		pinGod.LogInfo("base: onballstarted");
		_player = pinGod.Player as PlayboyPlayer;
		_player.SpecialRightLit = false;
		_player.BonusTimes = 1;

		pinGod.PlayMusic("cook_loop_1");
	}

	public void OnBallDrained()
	{
		pinGod.LogInfo("base: ball drained");
		pinGod.StopMusic();
	}

	public void OnBallSaved()
	{
		//todo: show PlayboyAward
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
			pinGod.SetLampState("special_r", _player.SpecialRightLit ? (byte)1 : (byte)0);
			pinGod.SetLampState("arrow_special", _player.LeftSpecialLit ? (byte)1 : (byte)0);
			pinGod.SetLampState("extra_ball", _player.ExtraBallLit ? (byte)1 : (byte)0);
		}
	}	
}
