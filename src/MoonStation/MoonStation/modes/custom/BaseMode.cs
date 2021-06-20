using Godot;
using static Godot.GD;

public class BaseMode : Control
{
	private PinGodGame pinGod;
	private Game game;
	int minScore = 50;
	int medScore = 100;
	int bigScore = 250;
	int massiveScore = 1000;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		game = GetParent().GetParent() as Game;
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
			Print("attract: starting game. started?", pinGod.StartGame());
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
		Print("game: ball started");
		if (pinGod.AudioManager.MusicEnabled)
		{
			Print("playing music");
			pinGod.AudioManager.PlayMusic(pinGod.AudioManager.Bgm);
		}
	}
	public void UpdateLamps() { }
}
