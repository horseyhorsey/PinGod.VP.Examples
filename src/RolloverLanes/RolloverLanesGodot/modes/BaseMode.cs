using Godot;

public class BaseMode : Control
{
	[Export] string BALL_SAVE_SCENE = "res://addons/PinGodGame/Modes/ballsave/BallSave.tscn";

	private PinGodGame pinGod;
	private Game game;
	private PackedScene _ballSaveScene;
	private BallStackPinball _ballStackSaucer;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		game = GetParent().GetParent() as Game;

		_ballSaveScene = GD.Load<PackedScene>(BALL_SAVE_SCENE);
		_ballStackSaucer = GetNode<BallStackPinball>(nameof(BallStackPinball));
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

	    /// <summary>
    /// Saucer "kicker" active.
    /// </summary>
    private void OnBallStackPinball_SwitchActive()
    {
        if (!pinGod.IsTilted && pinGod.GameInPlay)
        {
            pinGod.AddPoints(150);

            if (!pinGod.IsMultiballRunning)
            {
                //enable multiball and start timer on default timeout (see BaseMode scene, BallStackPinball)
                pinGod.IsMultiballRunning = true;
                _ballStackSaucer.Start();

                game?.CallDeferred("AddMultiballSceneToTree");
                return;
            }
        }

        //no multiball running or game not in play
        _ballStackSaucer.Start(1f);
    }

    private void OnBallStackPinball_timeout() => _ballStackSaucer.SolenoidPulse();
}
