using Godot;

public class Orbits : Control
{
	private PinGodGame pinGod;
	private VideoPlayerPinball video;
	private Game game;
	private Label label;
	int loops = 0;
	int loop_seed_value = 100000;

	[Export] VideoStreamTheora[] _videos = null;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		video = GetNode("VideoPlayerPinball") as VideoPlayerPinball;
		game = GetParent().GetParent() as Game;
		label = GetNode("CenterContainer/Label") as Label;
		this.Visible = false;
	}

	public override void _Input(InputEvent @event)
	{
		if (pinGod.GameInPlay) return;
		if (pinGod.IsTilted) return;

		if (!pinGod.IsMultiballRunning)
		{
			var orbitLastChanged = pinGod.GetLastSwitchChangedTime("orbit_m");
			var shooterLastChanged = pinGod.GetLastSwitchChangedTime("plunger_lane");
			if (pinGod.SwitchOn("orbit_m", @event))
			{
				if (orbitLastChanged < 3500)
				{
					//TODO: Flash all lamps
					loops++;
					var points = loops * loop_seed_value;
					long pointsAdded = game?.AddPoints(points) ?? points;
					label.Text = "LOOPS " + loops + "\n\r" + pointsAdded.ToScoreString();
					PlayOrbitScene(0);
					//video layers
					//brody_slow_ahead + orbit_sound
					//show loop count and points
				}
				else if (shooterLastChanged > 1500)
				{
					loops = 1;
					game?.AddPoints(loop_seed_value);
				}
				else
				{
					loops = 1;
					game?.AddPoints(950);
				}
			}
		}
	}
	private void _on_VideoPlayerPinball_finished()
	{
		label.Visible = false;
		this.Visible = false;
	}

	void PlayOrbitScene(int index)
	{
		var stream = _videos[index];
		video.Stream = stream;
		video.Play();
		video.Visible = true;
		label.Visible = true;
		this.Visible = true;
		//_clearLayersTimer.Stop();
		//_label.Text = "KICKBACK";		
		//_videoPlayer.Play();
		//ShowLayers(true);
	}
}
