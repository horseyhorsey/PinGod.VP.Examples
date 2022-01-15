using Godot;

/// <summary>
/// Top left of table kicker / saucer.
/// </summary>
public class LockShot : Control
{
    [Export] string _default_text = @"COMPLETING BANKS

LIGHTS POWER UP";

    private Game game;
    private Control layers;
    private SciFiPinGodGame pinGod;
    private SciFiPlayer player;
    private BlinkingLabel powerUpLabel;
    private Timer timer;
    VideoStreamTheora[] VideoStreams = new VideoStreamTheora[5];
    public VideoPlayerPinball VideoPlayer { get; private set; }
    public override void _EnterTree()
    {
        pinGod = GetNode("/root/PinGodGame") as SciFiPinGodGame;
		player = pinGod.GetSciFiPlayer();

		timer = GetNode("Timer") as Timer;
        game = GetParent().GetParent() as Game;

        layers = GetNode("ViewLayers") as Control;
        powerUpLabel = GetNode("ViewLayers/BlinkingLabel") as BlinkingLabel;

        VideoPlayer = GetNode("ViewLayers/VideoPlayerPinball") as VideoPlayerPinball;

        GetResources();

        layers.Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (pinGod.IsTilted) return;
        if (player == null) return;

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

        //Left target bank3
        if (pinGod.SwitchOn("bank4_1", @event))
        {
            player.AddAlienBonus(1);
            pinGod.PlaySfx("bonus_advance");
            if (player.Bank4[0]) { pinGod.AddPoints(1000); }
            else
            {
                player.Bank4[0] = true;
                pinGod.AddPoints(2500);
            }
            pinGod.UpdateAlienLamps(); pinGod.SetLampState("bank4_1", 0);
            CheckTargets();
        }
        if (pinGod.SwitchOn("bank4_2", @event))
        {
            player.AddAlienBonus(1);
            pinGod.PlaySfx("bonus_advance");
            if (player.Bank4[1]) { pinGod.AddPoints(1000); }
            else
            {
                player.Bank4[1] = true;
                pinGod.AddPoints(2500);
            }
            pinGod.UpdateAlienLamps(); pinGod.SetLampState("bank4_2", 0);
            CheckTargets();
        }
        if (pinGod.SwitchOn("bank4_3", @event))
        {
            player.AddAlienBonus(1);
            pinGod.PlaySfx("bonus_advance");
            if (player.Bank4[2]) { pinGod.AddPoints(1000); }
            else
            {
                player.Bank4[2] = true;
                pinGod.AddPoints(2500);
            }
            pinGod.UpdateAlienLamps(); pinGod.SetLampState("bank4_3", 0);
            CheckTargets();
        }
        if (pinGod.SwitchOn("bank4_4", @event))
        {
            player.AddAlienBonus(1);
            pinGod.PlaySfx("bonus_advance");
            if (player.Bank4[3]) { pinGod.AddPoints(1000); }
            else
            {
                player.Bank4[3] = true;
                pinGod.AddPoints(2500);
            }
            pinGod.UpdateAlienLamps(); pinGod.SetLampState("bank4_4", 0);
            CheckTargets();
        }

        //Right target bank4
        if (pinGod.SwitchOn("bank3_1", @event))
        {
            player.AddDefenderBonus(1);
            pinGod.PlaySfx("bonus_advance");
            if (player.Bank3[0]) { pinGod.AddPoints(1000); }
            else
            {
                player.Bank3[0] = true;
                pinGod.AddPoints(2500);
            }

            //update lamps and check completed
            pinGod.UpdateDefenderLamps(); pinGod.SetLampState("bank3_1", 0);
            CheckTargets();
        }
        if (pinGod.SwitchOn("bank3_2", @event))
        {
            player.AddDefenderBonus(1);
            pinGod.PlaySfx("bonus_advance");
            if (player.Bank3[1]) { pinGod.AddPoints(1000); }
            else
            {
                player.Bank3[1] = true;
                pinGod.AddPoints(2500);
            }

            //update lamps and check completed
            pinGod.UpdateDefenderLamps(); pinGod.SetLampState("bank3_2", 0);
            CheckTargets();
        }
        if (pinGod.SwitchOn("bank3_3", @event))
        {
            player.AddDefenderBonus(1);
            pinGod.PlaySfx("bonus_advance");
            if (player.Bank3[2]) { pinGod.AddPoints(1000); }
            else
            {
                player.Bank3[2] = true;
                pinGod.AddPoints(2500);
            }
            pinGod.UpdateDefenderLamps(); pinGod.SetLampState("bank3_3", 0);
            CheckTargets();
        }
    }

    private void _on_Timer_timeout()
    {
        layers.Visible = false;
        powerUpLabel.Text = _default_text;
        pinGod.SolenoidPulse("bs_left");
        CallDeferred("ChangeStream", 0);
    }

    void ChangeStream(int id)
    {
        this.Visible = true;
        VideoPlayer.Stream = VideoStreams[id];
        VideoPlayer.Visible = true;
        VideoPlayer.Play();
    }

    private void CheckTargets()
    {
        if (pinGod.IsTilted) return;

        var player = pinGod.GetSciFiPlayer();
        if (player.BanksCompleted())
        {
            if (player.SpawnEnabled == GameOption.Off)
            {
                player.SpawnEnabled = GameOption.Ready;
                player.LeftPowerUpLit = GameOption.Ready;
                pinGod.LogInfo("drops completed, SPAWN ready"); //todo: show this on screen
                powerUpLabel.Text += "BANKS COMPLETE\nSPAWN READY";
                CallDeferred("ChangeStream", 1);
            }
            else
            {
                //spawn mode already active.. are we going for invasion
                if (player.InvasionEnabled == GameOption.Off)
                {
                    player.InvasionEnabled = GameOption.Ready;
                    pinGod.LogInfo("drops completed, INVASION ready");
                    powerUpLabel.Text += "BANKS COMPLETE\nINVASION READY";
                    CallDeferred("ChangeStream", 2);
                }
                //invasion mode already active.. are we going for armada
                else
                {
                    if (player.ArmadaEnabled == GameOption.Off)
                    {
                        player.ArmadaEnabled = GameOption.Ready;
                        pinGod.LogInfo("drops completed, ARMADA ready");
                        powerUpLabel.Text += "BANKS COMPLETE\nARMADA READY";
                        CallDeferred("ChangeStream", 3);
                    }
                    else
                    {
                        if (player.AlienBaneEnabled == GameOption.Off)
                        {
                            player.AlienBaneEnabled = GameOption.Ready;
                            pinGod.LogInfo("drops completed, ALIENBANE enabled");
                            powerUpLabel.Text += "BANKS COMPLETE\nALIEN BANE READY";
                            CallDeferred("ChangeStream", 4);
                        }
                        else
                        {
                            //if the player gets a power up after getting alien bane then we award a special
                            pinGod.AwardSpecial();
                            pinGod.LogInfo("drops completed, awarded special");
                        }
                    }
                }
            }

            if (player.AlienBaneEnabled != GameOption.Complete)
            {
                player.LeftPowerUpLit = GameOption.Ready;
            }

            player.ResetBanks();
            pinGod.ResetTargets();
            pinGod.UpdatePowerupModeLamps();
        }
    }

    private void GetResources()
    {
		var res = pinGod.GetResources();

		VideoStreams[0] = res.GetResource("cosmic_6") as VideoStreamTheora;
		VideoStreams[1] = res.GetResource("cosmic_5") as VideoStreamTheora;
		VideoStreams[2] = res.GetResource("cosmic_7") as VideoStreamTheora;
		VideoStreams[3] = res.GetResource("cosmic_4") as VideoStreamTheora;
		VideoStreams[4] = res.GetResource("cosmic_3") as VideoStreamTheora;

		VideoPlayer.Stream = VideoStreams[0];
    }
    void OnBallStarted()
    {
        pinGod.ResetTargets();
    }

    /// <summary>
    /// Starts modes if ready
    /// </summary>
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
				//todo: START MULTIBALL
			}

			if (player.LeftPowerUpLit == GameOption.Ready)
			{
				player.LeftPowerUpLit = GameOption.Off;
				pinGod.PlaySfx("powerup");
				pinGod.LogInfo("power up hit ", player.SpawnEnabled);

				powerUpLabel.Text = "POWER UP\r\n";
				//LightSeqGI.Play SeqRandom, 20, ,1000 TODO
				if (player.SpawnEnabled == GameOption.Ready)
				{
					pinGod.LogInfo("spawn running");
					player.SpawnEnabled = GameOption.Complete;
					powerUpLabel.Text += "SPAWN";
					CallDeferred("ChangeStream", 1);
				}
				else
				{
					if (player.InvasionEnabled == GameOption.Ready)
					{
						pinGod.LogInfo("invasion running");
						powerUpLabel.Text += "INVASION";
						player.InvasionEnabled = GameOption.Complete;
						CallDeferred("ChangeStream", 2);
					}
					else
					{
						if (player.ArmadaEnabled == GameOption.Ready)
						{
							powerUpLabel.Text += "ARMADA";
							player.ArmadaEnabled = GameOption.Complete;
							CallDeferred("ChangeStream", 3);
						}
						else if (player.AlienBaneEnabled == GameOption.Ready)
						{
							powerUpLabel.Text += "ALIEN BANE";
							player.AlienBaneEnabled = GameOption.Complete;
							pinGod.StartMultiBall(2, 25);
							CallDeferred("ChangeStream", 4);
						}
					}
				}

				game?.SetupMode();
				pinGod.UpdatePowerupModeLamps();
				game?.EnableKickback();
			}
		}

		timer.Start(1.3f);
	}
}
