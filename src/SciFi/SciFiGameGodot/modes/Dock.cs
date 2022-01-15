using Godot;

/// <summary>
/// Complete SciFi targets to light dock
/// </summary>
public class Dock : Control
{
	private SciFiPinGodGame pinGod;
	private Timer kickerTimer;
	private Control layers;
	private BlinkingLabel dockLabel;
	private Game game;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as SciFiPinGodGame;
		kickerTimer = GetNode("KickerTimer") as Timer;
		layers = GetNode("ViewLayers") as Control;
		dockLabel = GetNode("ViewLayers/BlinkingLabel") as BlinkingLabel;

		layers.Visible = false;

		game = GetParent().GetParent() as Game;

		layers.GetNode<VideoPlayerPinball>(nameof(VideoPlayerPinball)).Stream = pinGod.GetResource("cosmic_5") as VideoStreamTheora;
	}

	public override void _Input(InputEvent @event)
	{
		if (pinGod.IsTilted && kickerTimer.IsStopped())
		{
			kickerTimer.Start(1.5f);
		}

		if (pinGod.SwitchOn("bs_right", @event))
		{			
			kickerTimer.Start(1.3f);
			pinGod.PlaySfx("eject");

			CallDeferred("ProcessDockAward");
		}

		//scifi targets
		if (pinGod.SwitchOn("Scifi", @event))
		{
			CheckSciFiTargets(0);
		}
		if (pinGod.SwitchOn("sCifi", @event))
		{
			CheckSciFiTargets(1);
		}
		if (pinGod.SwitchOn("scIfi", @event))
		{
			CheckSciFiTargets(2);
		}
		if (pinGod.SwitchOn("sciFi", @event))
		{
			CheckSciFiTargets(3);
		}
		if (pinGod.SwitchOn("scifI", @event))
		{
			CheckSciFiTargets(4);
		}
	}

	/// <summary>
	/// Check targets complete and reset the targets. Sets Docking award variables.
	/// </summary>
	private void CheckSciFiTargets(int targetIndex)
	{
		pinGod.AddPoints(1000);
		pinGod.PlaySfx("scifi_hit");
		var player = pinGod.GetSciFiPlayer();
		player.Scifi[targetIndex] = true;
		if (player.IsSciFiTargetsComplete())
		{
			pinGod.LogInfo("targets: sci fi completed.");
			pinGod.AddPoints(1000);

			if (!player.DockEnabled)
			{
				player.DockEnabled = true;
				pinGod.PlaySfx("dock_lit");				
				switch (player.SciFiAwardLevel)
				{
					case 0:
						player.Dock50K = GameOption.Ready;
						pinGod.LogInfo("dock lit 50k");
						break;
					case 1:
						player.DockSpecial = GameOption.Ready;
						pinGod.LogInfo("dock lit special");
						break;
					case 2:
						if (player.ExtraBallsAwarded <= 0)
						{
							player.DockExtraBall = GameOption.Ready;
							pinGod.LogInfo("dock lit extra ball");
						}
						else
						{
							player.DockSpecial = GameOption.Ready;
							player.SciFiAwardLevel = 1;
							pinGod.LogInfo("dock reset level");
						}
						break;
					default:
						break;
				}

				pinGod.UpdateDockLamps();
			}			
		}

		pinGod.UpdateSciFiLamps();
	}

	private void ProcessDockAward()
	{
		//show display layers if no multiball
		if (!pinGod.IsMultiballRunning)
			layers.Visible = true;

		var p = pinGod.GetSciFiPlayer();
		if (p.DockEnabled)
		{
			dockLabel.Text = "DOCKED";			
			if (p.Dock50K == GameOption.Ready)
			{
				dockLabel.Text = $"DOCKED\r\nAWARDED\r\n50,000";
				pinGod.LogInfo("player docked 50K");
				p.Dock50K = GameOption.Complete;
				pinGod.PlaySfx("bonus");
				pinGod.AddPoints(50000);
				p.SciFiAwardLevel = 1;
			}

			if (p.DockSpecial == GameOption.Ready)
			{
				dockLabel.Text = $"DOCKED\r\nSPECIAL LIT";
				pinGod.LogInfo("player docked special lit");
				p.DockSpecial = GameOption.Complete;
				pinGod.PlaySfx("bonus");
				p.SciFiAwardLevel = 2;
			}

			if (p.DockExtraBall == GameOption.Ready)
			{
				pinGod.AwardExtraBall();
				dockLabel.Text = $"DOCKED\r\nEXTRA BALL";
				pinGod.LogInfo("docked extra ball");
				p.DockExtraBall = GameOption.Complete;
				p.SciFiAwardLevel = 0;
				pinGod.PlaySfx("bonus");
			}

			kickerTimer.Start(1.3f);
			pinGod.UpdateDockLamps();
		}
		else
		{
			pinGod.AddPoints(1000);
		}        
	}

	private void _on_KickerTimer_timeout()
	{
		layers.Visible = false;
		var p = pinGod.GetSciFiPlayer();
		if (p.DockEnabled)
		{
			p.DockEnabled = false;
			p.ResetSciFi();
			pinGod.UpdateSciFiLamps();
		}
		pinGod.SolenoidPulse("bs_right");
	}
}
