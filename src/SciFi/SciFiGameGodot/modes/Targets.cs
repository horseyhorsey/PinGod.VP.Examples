using Godot;

public class Targets : Control
{
	private SciFiPinGodGame pinGod;
	private SciFiPlayer player;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as SciFiPinGodGame;
		GD.Print("targets: enter tree");
	}

	/// <summary>
	/// Checks all targets switches. These include the Drop Banks and SciFi stand up targets
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (pinGod.IsTilted) return;
		if (player == null) return;

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

	private void CheckTargets()
	{
		if (pinGod.IsTilted) return;

		var player = pinGod.GetSciFiPlayer();
		if(player.BanksCompleted())
		{
			GD.Print("drops completed");
			player.LeftPowerUpLit = GameOption.Ready;
			if(player.SpawnEnabled == GameOption.Off)
			{
				player.SpawnEnabled = GameOption.Ready;
			}
			//if (player.SpawnEnabled == GameOption.Ready)
			//{
			//	player.SpawnEnabled = GameOption.Complete;
			//	player.LeftPowerUpLit = GameOption.Ready;
			//}
			//else
			//{
			//	//spawn mode already active.. are we going for invasion
			//	if (player.InvasionEnabled == GameOption.Ready)
			//	{
			//		player.LeftPowerUpLit = GameOption.Ready;
			//		player.InvasionEnabled = GameOption.Complete;
			//	}
			//	//invasion mode already active.. are we going for armada
			//	else
			//	{
			//		if (player.ArmadaEnabled == GameOption.Ready)
			//		{
			//			player.LeftPowerUpLit = GameOption.Ready;
			//			player.ArmadaEnabled = GameOption.Complete;						
			//		}
			//		else
			//		{
			//			if (player.AlienBaneEnabled == GameOption.Ready)
			//			{							
			//				player.AlienBaneEnabled = GameOption.Complete;				
			//			}
			//			else
			//			{
			//				pinGod.AwardSpecial();
			//			}                            
			//		}
			//	}
			//}

			//if (player.AlienBaneEnabled != GameOption.Complete)
			//{				
			//	player.LeftPowerUpLit = GameOption.Ready;
			//}

			player.ResetBanks();
			pinGod.ResetTargets();
			pinGod.UpdatePowerupModeLamps();
		}
		else
		{
			GD.Print("drops checked");
		}
	}

	void OnBallStarted()
	{
		GD.Print("targets: ball started");
		player = pinGod.GetSciFiPlayer();
	}
}
