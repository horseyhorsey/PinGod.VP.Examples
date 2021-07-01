using Godot;
using static Godot.GD;

public class BaseMode : Control
{
    int bigScore = 250;
    private Game game;
    int medScore = 100;
    int minScore = 50;
    private PinGodGame pinGod;
	private PackedScene _ballSaveScene;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		game = GetParent().GetParent() as Game;

		_ballSaveScene = GD.Load<PackedScene>(Game.BALL_SAVE_SCENE);
	}

	/// <summary>
	/// Basic input switch handling
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (!pinGod.GameInPlay) return;
		//game is tilted, don't process other switches when tilted

		if (pinGod.IsTilted) return;

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
		if (pinGod.SwitchOn("outlane_l", @event))
		{
			pinGod.AddPoints(bigScore);
			pinGod.AudioManager.PlaySfx("spinner");
		}
		if (pinGod.SwitchOn("inlane_l", @event))
		{
			pinGod.AddPoints(medScore);
			pinGod.AudioManager.PlaySfx("spinner");
		}
		if (pinGod.SwitchOn("inlane_r", @event))
		{
			pinGod.AddPoints(medScore);
			pinGod.AudioManager.PlaySfx("spinner");
		}
		if (pinGod.SwitchOn("outlane_r", @event))
		{
			pinGod.AddPoints(bigScore);
			pinGod.AudioManager.PlaySfx("spinner");
		}
		if (pinGod.SwitchOn("sling_l", @event))
		{
			pinGod.AddPoints(minScore);
			pinGod.AudioManager.PlaySfx("spinner");
		}
		if (pinGod.SwitchOn("sling_r", @event))
		{
			pinGod.AddPoints(minScore);
			pinGod.AudioManager.PlaySfx("spinner");
		}
		if (pinGod.SwitchOn("spinner", @event))
		{
			pinGod.AddPoints(medScore);
			pinGod.AudioManager.PlaySfx("spinner");
		}
		if (pinGod.SwitchOn("bumper_l", @event) || pinGod.SwitchOn("bumper_r", @event))
		{
			pinGod.AddPoints(medScore);
			pinGod.AudioManager.PlaySfx("spinner");
		}
		if (pinGod.SwitchOn("top_left_target", @event))
		{
			pinGod.AddPoints(medScore);
			pinGod.AudioManager.PlaySfx("spinner");
		}
	}

    public void OnBallStarted()
    {
		pinGod.LogInfo("game: ball started");
        if (pinGod.AudioManager.MusicEnabled)
        {
            pinGod.AudioManager.PlayMusic(pinGod.AudioManager.Bgm);
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

}
