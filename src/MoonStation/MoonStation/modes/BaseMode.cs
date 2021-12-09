using Godot;
using static MoonStation.GameGlobals;

/// <summary>
/// A base mode is a mode that is generally always running.
/// </summary>
public class BaseMode : Control
{
	/// <summary>
	/// Where our ball scene save is. This is set to use the default from the addons but here to easily be swapped out. * Exported variables can be changed in Godots scene inspector
	/// </summary>
	[Export] string BALL_SAVE_SCENE = "res://addons/PinGodGame/Modes/ballsave/BallSave.tscn";

	private PackedScene _ballSaveScene;        
	private Game game;    
    private MoonStationPinGodGame pinGod;
	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as MoonStationPinGodGame;
		game = GetParent().GetParent() as Game;

		_ballSaveScene = GD.Load<PackedScene>(BALL_SAVE_SCENE);
	}

	/// <summary>
	/// Basic input switch handling and scoring
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		//game not in play or is tilted so we return
		if (!pinGod.GameInPlay || pinGod.IsTilted ) return;	

		if (pinGod.SwitchOn("start", @event))
		{
			pinGod.LogDebug("attract: starting game. started?", pinGod.StartGame());
		}

		// Flipper switches set here to reset the ball search timer
		if (pinGod.SwitchOn("flipper_l", @event)) { }
		if (pinGod.SwitchOn("flipper_r", @event)) { }

		// in lanes / out lanes
		if (pinGod.SwitchOn("inlane_l", @event))
		{
			pinGod.AddPoints(SMALL_SCORE);
			pinGod.AudioManager.PlaySfx("spinner");
		}
		if (pinGod.SwitchOn("inlane_r", @event))
		{
			pinGod.AddPoints(SMALL_SCORE);
			pinGod.AudioManager.PlaySfx("spinner");
		}
		if (pinGod.SwitchOn("outlane_l", @event))
		{
			pinGod.AddPoints(MED_SCORE);
			pinGod.AudioManager.PlaySfx("spinner");
		}
		if (pinGod.SwitchOn("outlane_r", @event))
		{
			pinGod.AddPoints(MED_SCORE);			
			pinGod.AudioManager.PlaySfx("spinner");
		}

		//slingshots
		if (pinGod.SwitchOn("sling_l", @event))
		{
			pinGod.AddPoints(MIN_SCORE);
			pinGod.AudioManager.PlaySfx("spinner");
		}
		if (pinGod.SwitchOn("sling_r", @event))
		{
			pinGod.AddPoints(MIN_SCORE);
			pinGod.AudioManager.PlaySfx("spinner");
		}

		//spinner
		if (pinGod.SwitchOn("spinner", @event))
		{
			pinGod.AddPoints(MIN_SCORE);
			pinGod.AudioManager.PlaySfx("spinner");
		}

		//bumpers L and R
		if (pinGod.SwitchOn("bumper_l", @event) || pinGod.SwitchOn("bumper_r", @event))
		{
			pinGod.AddPoints(SMALL_SCORE);
			pinGod.AudioManager.PlaySfx("spinner");
		}

		//targets
		if (pinGod.SwitchOn("top_left_target", @event))
		{
			pinGod.AddPoints(LARGE_SCORE);
			pinGod.AudioManager.PlaySfx("spinner");
		}
	}

	#region Mode Group Methods

	//This mode is added to a group named Group in the BaseMode scene
	//These methods will be called by the game if the the scene is in the "Mode" group

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

	/// <summary>
	/// When ball is started
	/// </summary>
	public void OnBallStarted()
	{
		pinGod.LogInfo("game: ball started");
		if (pinGod.AudioManager.MusicEnabled)
		{
			pinGod.AudioManager.PlayMusic(pinGod.AudioManager.Bgm);
		}
	}
	public void UpdateLamps() { }
	#endregion


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
	/// Helper to start MultiBall. Set the game to "IsMultiballRunning", fire the saucer coil to exit ball, add a scene
	/// </summary>
	private void StartMultiball()
    {
		if (!pinGod.IsMultiballRunning)
		{
			pinGod.IsMultiballRunning = true;
			pinGod.SolenoidPulse("mball_saucer");

			//add the multiball scene from the game
			game?.CallDeferred("AddMultiballSceneToTree");            
		}
		else
		{
			//already in multiball
			pinGod.SolenoidPulse("mball_saucer");
		}
	}

}
