using Godot;

public class BaseMode : Control
{
	[Export] string BALL_SAVE_SCENE = "res://addons/PinGodGame/Modes/ballsave/BallSave.tscn";

	private PinGodGame pinGod;
	private Game game;
	private PackedScene _ballSaveScene;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
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
		if (pinGod.SwitchOn("flipper_l", @event))
		{
		}
		if (pinGod.SwitchOn("flipper_r", @event))
		{
		}
		if (pinGod.SwitchOn("outlane_l", @event))
		{
			pinGod.AddPoints(100);
		}
		if (pinGod.SwitchOn("inlane_l", @event))
		{
			pinGod.AddPoints(100);
		}
		if (pinGod.SwitchOn("inlane_r", @event))
		{
			pinGod.AddPoints(100);
		}
		if (pinGod.SwitchOn("outlane_r", @event))
		{
			pinGod.AddPoints(100);
		}
		if (pinGod.SwitchOn("sling_l", @event))
		{
			pinGod.AddPoints(50);
		}
		if (pinGod.SwitchOn("sling_r", @event))
		{
			pinGod.AddPoints(50);
		}
		if (pinGod.SwitchOn("mball_saucer", @event))
		{
			pinGod.AddPoints(150);
			StartMultiball();
		}
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
