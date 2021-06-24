using Godot;
public class BaseMode : Control
{
	private PinGodGame pinGod;
	private Game game;

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

	public void OnBallStarted() { }
	public void UpdateLamps() { }
}
