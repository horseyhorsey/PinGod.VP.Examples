using Godot;
using System;
using System.Linq;

public class BaseMode : Control
{
	[Export] string BALL_SAVE_SCENE = "res://addons/PinGodGame/Modes/ballsave/BallSave.tscn";

	private PinGodGame pinGod;
	private Game game;
	private PackedScene _ballSaveScene;

	public bool SkillshotReady { get; private set; }
	public bool ModeStartReady { get; private set; }

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		game = GetParent().GetParent() as Game;

		_ballSaveScene = GD.Load<PackedScene>(BALL_SAVE_SCENE);

		var _panel = GetNode<PanelContainer>("PanelContainer");
		_panel.AddColorOverride("bg_color", Color.Color8(0, 255, 0));
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
			pinGod.AddPoints(1000);
		}
		if (pinGod.SwitchOn("inlane_l", @event))
		{
			pinGod.AddPoints(1000);
		}
		if (pinGod.SwitchOn("inlane_r", @event))
		{
			pinGod.AddPoints(1000);
		}
		if (pinGod.SwitchOn("outlane_r", @event))
		{
			pinGod.AddPoints(1000);
		}
		if (pinGod.SwitchOn("sling_l", @event))
		{
			pinGod.AddPoints(1000);
		}
		if (pinGod.SwitchOn("sling_r", @event))
		{
			pinGod.AddPoints(1000);
		}

		if (pinGod.SwitchOn("scoop_m", @event))
		{
			pinGod.AddPoints(2500);
		}
		if (pinGod.SwitchOn("scoop_r", @event))
		{
			pinGod.AddPoints(2500);
		}
		if (pinGod.SwitchOn("spider_target", @event))
		{
			pinGod.AddPoints(2500);
		}

		if (pinGod.SwitchOn("ramp_l", @event))
		{
			ProcessLeftRamp();
		}
		else if (pinGod.SwitchOff("ramp_l", @event)) { }
	}

	private void ProcessBumper()
	{
		pinGod.AddPoints(5000);
		var mode = game.RotateModes();
		if(mode != null)
		{
			pinGod.LogDebug("mode rotated: " + mode.Title);
			game.AddToSpiderJackpot();
			//todo: SpiderJackpotGrows
		}
	}

	/// <summary>
	/// This switch awards a skillshot, starts a mode and wizard mode. Hit the ramp to lock the blinking mode.Then hit the left ramp a second time to start the lit mode
	/// </summary>
	private void ProcessLeftRamp()
	{
		pinGod.AddPoints(50000);

		bool skillMade = false;
		if (SkillshotReady)
		{
			SkillshotCompleted();
			skillMade = true;
		}

		if (!game.ModeRunning)
		{
			if (game._player.CompletedModes.Any(x => !x.Completed))
			{
				if (!ModeStartReady)
				{
					ModeStartReady = true;
					game._player.CanRotateModes = false;
					pinGod.LogDebug("left ramp mode locked in");

					if (!skillMade)
					{
						//todo ModeLocked
					}
				}
				else
				{
					pinGod.LogDebug("left ramp starting mode");
					game.AddSelectedGremlinsMode();
				}
			}
			else
			{
				if (!game.WizardIsStarted)
				{
					pinGod.LogDebug("left ramp starting wizard mode");
					game.AddSelectedGremlinsWizardMode();
				}
			}
		}
	}



	private void SkillshotCompleted()
	{
		//todo skill_made
		pinGod.LogDebug("skillshot made");
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

	public void OnBallDrained() { }
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
	public void OnBallStarted() { }
	public void UpdateLamps() { }

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

	private void _on_Bumper0_BumperHit(string name)
	{
		ProcessBumper();
	}

	private void _on_Bumper1_BumperHit(string name)
	{
		ProcessBumper();
	}

	private void _on_Bumper2_BumperHit(string name)
	{
		ProcessBumper();
	}
}
