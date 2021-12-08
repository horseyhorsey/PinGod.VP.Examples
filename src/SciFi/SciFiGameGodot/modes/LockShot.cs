using Godot;

public class LockShot : Control
{
	private SciFiPinGodGame pinGod;
	private Timer timer;
	private Game game;
	private Control layers;
	private BlinkingLabel powerUpLabel;
	public VideoPlayerPinball VideoPlayer { get; private set; }
	VideoStreamTheora[] VideoStreams = new VideoStreamTheora[5];

	[Export] string _default_text = @"COMPLETING BANKS



LIGHTS POWER UP";

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as SciFiPinGodGame;
		timer = GetNode("Timer") as Timer;
		game = GetParent().GetParent() as Game;

		layers = GetNode("ViewLayers") as Control;
		powerUpLabel = GetNode("ViewLayers/BlinkingLabel") as BlinkingLabel;		
	
		var stream = new VideoStreamTheora() { File = "res://assets/videos/cosmic_7.ogv" };
		VideoStreams[0] = stream;		
		stream = new VideoStreamTheora() { File = "res://assets/videos/cosmic_6.ogv" };
		VideoStreams[1] = stream;
		stream = new VideoStreamTheora() { File = "res://assets/videos/cosmic_5.ogv" };
		VideoStreams[2] = stream;
		stream = new VideoStreamTheora() { File = "res://assets/videos/cosmic_4.ogv" };
		VideoStreams[3] = stream;
		stream = new VideoStreamTheora() { File = "res://assets/videos/cosmic_3.ogv" };
		VideoStreams[4] = stream;


		VideoPlayer = GetNode("ViewLayers/VideoPlayerPinball") as VideoPlayerPinball;
		VideoPlayer.Stream = VideoStreams[0];

		layers.Visible = false;
	}

	private void _on_Timer_timeout()
	{		
		layers.Visible = false;		
		powerUpLabel.Text = _default_text;		
		pinGod.SolenoidPulse("bs_left");
		CallDeferred("ChangeStream", 0);
	}

	public override void _Input(InputEvent @event)
	{
		//TOP SAUCER - BALL LOCK
		if (pinGod.SwitchOn("bs_left", @event))
		{
			ProcessLockKicker();
		}

		if (pinGod.SwitchOn("star_left_1", @event))
		{
			pinGod.PlaySfx("star_trigger_rollover");
			pinGod.AddPoints(250);
		}
		if (pinGod.SwitchOn("star_left_2", @event))
		{
			pinGod.PlaySfx("star_trigger_rollover");
			pinGod.AddPoints(250);
		}
		if (pinGod.SwitchOn("star_left_3", @event))
		{
			pinGod.PlaySfx("star_trigger_rollover");
			pinGod.AddPoints(250);
		}
	}

	private void ProcessLockKicker()
	{
		//show display layers if no multi-ball
		if (!pinGod.IsMultiballRunning)
			layers.Visible = true;

		if (!pinGod.IsTilted)
		{
			var player = pinGod.GetSciFiPlayer();
			if (player.LeftLockLit)
			{
				//START MULTIBALL
			}

			if (player.LeftPowerUpLit == GameOption.Ready)
			{
				player.LeftPowerUpLit = GameOption.Off;
				pinGod.PlaySfx("powerup");

				GD.Print("power up hit ", player.SpawnEnabled);

				powerUpLabel.Text = "POWER UP\r\n";
				//LightSeqGI.Play SeqRandom, 20, ,1000 TODO
				if (player.SpawnEnabled == GameOption.Ready)
				{
					GD.Print("spawn ready");
					player.SpawnEnabled = GameOption.Complete;
					player.InvasionEnabled = GameOption.Ready;
					powerUpLabel.Text += "SPAWN";
					CallDeferred("ChangeStream", 1);
				}
				else
				{
					if (player.InvasionEnabled == GameOption.Ready)
					{
						GD.Print("invasion ready");
						powerUpLabel.Text += "INVASION";
						player.InvasionEnabled = GameOption.Complete;
						player.ArmadaEnabled = GameOption.Ready;
						CallDeferred("ChangeStream", 2);
					}
					else
					{
						if (player.ArmadaEnabled == GameOption.Ready)
						{
							powerUpLabel.Text += "ARMADA";
							player.ArmadaEnabled = GameOption.Complete;
							player.AlienBaneEnabled = GameOption.Ready;
							CallDeferred("ChangeStream", 3);
						}
						else if (player.AlienBaneEnabled == GameOption.Ready)
						{
							powerUpLabel.Text += "ALIEN BANE";
							player.AlienBaneEnabled = GameOption.Complete;
							CallDeferred("ChangeStream", 4);
						}
					}
				}

				//TODO: Run setup mode, enable kickback
				game?.SetupMode();
				pinGod.UpdatePowerupModeLamps();
				game?.EnableKickback();
			}
		}

		timer.Start(1.3f);
	}

	void ChangeStream(int id)
	{
		VideoPlayer.Stream = VideoStreams[id];
		VideoPlayer.Play();
	}
}
