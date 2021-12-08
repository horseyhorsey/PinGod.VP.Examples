using Godot;

/// <summary>
/// In the original game script this is literally doing nothing....
/// </summary>
public class JackpotShot : Control
{
	private PinGodGame pinGod;
	private Timer timer;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		timer = GetNode("Timer") as Timer;
	}


	public override void _Input(InputEvent @event)
	{
		if (pinGod.SwitchOn("bs_top_right", @event))
		{
			pinGod.AddPoints(500);
			timer.Start(1.2f);
			//TODO: Flashers, Light jackpot?
		}
	}

	private void _on_Timer_timeout()
	{
		pinGod.SolenoidPulse("bs_top_right");
		pinGod.PlaySfx("eject");
	}
}
