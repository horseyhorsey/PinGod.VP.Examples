using Godot;

public class BaseMode : Control
{
	[Export] string BALL_SAVE_SCENE = "res://addons/PinGodGame/Modes/ballsave/BallSave.tscn";

	private SciFiPinGodGame pinGod;
	private Game game;
	private PackedScene _ballSaveScene;	

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as SciFiPinGodGame;
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
			pinGod.LogDebug("attract: starting game. started?", pinGod.StartGame());
		}

		// Flipper switches set to reset the ball search timer
		if (pinGod.SwitchOn("flipper_l", @event))
		{
		}
		if (pinGod.SwitchOn("flipper_r", @event))
		{
		}

		var currentPlayer = pinGod.GetSciFiPlayer();
		//Outlane Kickbacks
		if (pinGod.SwitchOn("outlane_l", @event))
		{
			if (currentPlayer.DockSpecial == GameOption.Complete)
			{
				pinGod.AddPoints(5000); pinGod.AwardSpecial();
				currentPlayer.DockSpecial = GameOption.Off;
				pinGod.SetLampState("outlane_l", 0);
				pinGod.SetLampState("outlane_r", 0);
			}
			else
			{
				pinGod.AddPoints(3000);
			}

			currentPlayer.AddAlienBonus(2);
		}
		if (pinGod.SwitchOn("inlane_l", @event))
		{
			if (currentPlayer.InvasionEnabled == GameOption.Complete)
			{
				pinGod.AddPoints(2000);
			}
			else
				pinGod.AddPoints(1000);

			currentPlayer.AddDefenderBonus(1);

		}
		if (pinGod.SwitchOn("inlane_r", @event))
		{
			if (currentPlayer.InvasionEnabled == GameOption.Complete)
			{
				pinGod.AddPoints(2000);
			}
			else
				pinGod.AddPoints(1000);

			currentPlayer.AddDefenderBonus(1);
		}
		if (pinGod.SwitchOn("outlane_r", @event))
		{
			if (currentPlayer.DockSpecial == GameOption.Complete)
			{
				pinGod.PlaySfx("drain");
				currentPlayer.DockSpecial = GameOption.Off;
				pinGod.AddPoints(5000); pinGod.AwardSpecial();
				pinGod.SetLampState("outlane_l", 0); pinGod.SetLampState("outlane_r", 0);
			}
			else
			{
				pinGod.AddPoints(3000);
			}

			currentPlayer.AddAlienBonus(2);
		}
		if (pinGod.SwitchOn("sling_l", @event))
		{
			SlingHit();
		}
		if (pinGod.SwitchOn("sling_r", @event))
		{
			SlingHit();
		}

		if (pinGod.SwitchOn("bumper_0", @event))
		{
			pinGod.AddPoints(500);
			pinGod.PlaySfx("bumper_1");
		}
		if (pinGod.SwitchOn("bumper_1", @event))
		{
			pinGod.AddPoints(500);
			pinGod.PlaySfx("bumper_2");
		}
		if (pinGod.SwitchOn("bumper_2", @event))
		{
			pinGod.AddPoints(500);
			pinGod.PlaySfx("bumper_3");
		}
		if (pinGod.SwitchOn("spinner", @event))
		{
			if (currentPlayer.SpawnEnabled == GameOption.Complete) pinGod.AddPoints(200); else pinGod.AddPoints(100);
			pinGod.PlaySfx("spinner");
		}
	}

	private void SlingHit()
	{
		if (pinGod?.GetSciFiPlayer().SpawnEnabled == GameOption.Complete)
		{
			pinGod.AddPoints(500);
		}
		else
		{
			pinGod.AddPoints(250);
		}
		pinGod.PlaySfx("rocket_slings");
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
}
