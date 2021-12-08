using Godot;

public class Kickback : Control
{
	private PinGodGame pinGod;
	private Game game;
	private Timer timer;

	public override void _EnterTree()
	{
		game = GetParent().GetParent() as Game;
		timer = GetNode("Timer") as Timer;
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
	}

	public override void _Input(InputEvent @event)
	{
		if (pinGod.SwitchOn("outlane_l", @event))
		{
			if (game.KickbackEnabled)
			{
				GD.Print("kickback: fire");
				timer.Stop();
				timer.Start(2f);
				pinGod.SetLampState("kickback", 2);
				pinGod.SolenoidPulse("kickback");
				pinGod.PlaySfx("bumper_2"); //TODO: Should be random sound bumper.
			}
			else
			{
				pinGod.PlaySfx("drain");
			}
		}
	}

	public void EnableKickback(bool enable)
	{
		game.KickbackEnabled = enable;
		if(!enable)
			pinGod.SetLampState("kickback", 0);
		else
			pinGod.SetLampState("kickback", 1);
	}

	private void _on_Timer_timeout()
	{
		GD.Print("kickback: timed_out");
		EnableKickback(false);
	}
}
