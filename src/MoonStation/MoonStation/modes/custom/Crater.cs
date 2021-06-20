using Godot;
using static Godot.GD;

public class Crater : Control
{
	private Timer saucerTimer;
	private PinGodGame pinGod;

	public AudioStreamPlayer AudioStream { get; private set; }

	private PackedScene _moonLanderScene;
	private Node instance;

	public override void _EnterTree()
	{
		saucerTimer = new Timer() { OneShot = true, Autostart = false };
		AddChild(saucerTimer);
		saucerTimer.Connect("timeout", this, "_timeout");
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		AudioStream = GetNode("AudioStreamPlayer") as AudioStreamPlayer;
		_moonLanderScene = Load("moonstation_lander/MoonLander.tscn") as PackedScene;
	}

	/// <summary>
	/// Crater saucer. Start timer and exit when finished.
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (pinGod.GameInPlay)
		{
			if (pinGod.SwitchOn("crater_saucer", @event))
			{
				Visible = true;
				instance = _moonLanderScene.Instance();
				AddChild(instance);
				pinGod.SolenoidOn("lampshow_1", 1);
				saucerTimer.Start(5.0f);
				this.Show();
				AudioStream?.Play();
			}
		}
		else
		{
			pinGod.SolenoidPulse("crater_saucer");
		}
	}

	private void _timeout()
	{
		Print("crater time out");
		instance.QueueFree();
		RemoveChild(instance);        
		pinGod.SolenoidPulse("crater_saucer");
		pinGod.SolenoidOn("lampshow_1", 0);
		Hide();
	}
}
