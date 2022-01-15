using Godot;

public class Orbits : Control
{
	private PinGodGame pinGod;
	private VideoPlayerPinball video;
	private Game game;
	private Label label;
	int loops = 0;
	int loop_seed_value = 250000;

	[Export] VideoStreamTheora[] _videos = null;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		video = GetNode("VideoPlayerPinball") as VideoPlayerPinball;
		game = GetParent().GetParent() as Game;
		label = GetNode("CenterContainer/Label") as Label;
		this.Visible = false;

		var res = pinGod.GetResources().Resolve("brody_slow_ahead") as VideoStreamTheora;
		_videos = new VideoStreamTheora[1];
		_videos[0] = res;
	}

	public override void _Input(InputEvent @event)
	{
		if (!pinGod.GameInPlay) return;
		if (pinGod.IsTilted) return;

		if (!pinGod.IsMultiballRunning)
		{
			var orbitLastChanged = pinGod.GetLastSwitchChangedTime("orbit_m");
			var shooterLastChanged = pinGod.GetLastSwitchChangedTime("plunger_lane");
			if (pinGod.SwitchOn("orbit_m", @event))
			{
				if (orbitLastChanged > 0 && orbitLastChanged < 4500)
				{
					//TODO: Flash all lamps
					loops++;
					var points = loops * loop_seed_value;
					long pointsAdded = game?.AddPoints(points) ?? points;
					label.Text = Tr("LOOPS") + $" {loops}\n\r{pointsAdded.ToScoreString()}";
					PlayOrbitScene(0);
					pinGod.SolenoidOn("vpcoil", 5); //todo:vp lamps
					//video layers
					//brody_slow_ahead + orbit_sound
					//show loop count and points
				}
				else if (shooterLastChanged > 1500)
				{
					pinGod.PlayVoice("quint_slow_ahead");
					loops = 1;
					game?.AddPoints(loop_seed_value);					
				}
                else
                {
					pinGod.AddPoints(Game.BASIC_SWITCH_VALUE);
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
