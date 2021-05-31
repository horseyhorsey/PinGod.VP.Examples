using Godot;
using System;
using static Godot.GD;

public class Crater : Control
{
	private Timer saucerTimer;
	private AnimationPlayer player;
	private PinGodGame pinGod;

	public AudioStreamPlayer AudioStream { get; private set; }

	public override void _EnterTree()
	{
		saucerTimer = new Timer() { OneShot = true, Autostart = false };
		AddChild(saucerTimer);
		saucerTimer.Connect("timeout", this, "_timeout");

		player = this.GetNode("AnimationPlayer") as AnimationPlayer;

		pinGod = GetNode("/root/PinGodGame") as PinGodGame;

		AudioStream = GetNode("AudioStreamPlayer") as AudioStreamPlayer;
	}

	/// <summary>
	/// Crater saucer. Start timer and exit when finished.
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (pinGod.SwitchOn("crater_saucer", @event))
		{
			Visible = true;
			Print("sw: saucer");
			pinGod.SolenoidOn("lampshow_1", 1);
			saucerTimer.Start(2.0f);			
			player.Stop();
			this.Show(); 
			player.Play();
			AudioStream?.Play();
		}
	}

	private void _timeout()
	{
		Print("crater time out");
		pinGod.SolenoidPulse("crater_saucer");
		pinGod.SolenoidOn("lampshow_1", 0);
		Hide();		
	}
}
