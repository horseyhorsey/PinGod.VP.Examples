public class SciFiPinGodGame : PinGodGame
{
    /// <summary>
    /// override to create our own player type for this game
    /// </summary>
    /// <param name="name"></param>
    public override void CreatePlayer(string name)
    {
        Players.Add(new SciFiPlayer() { Name = name, Points = 0 });
    }

	/// <summary>
	/// When the player gets a power up in Alien Bane
	/// </summary>
	public void AwardSpecial(int score = 250000)
	{
		AddPoints(score);
		PlaySfx("bonus");
	}

	public void AwardExtraBall()
	{
		Player.ExtraBalls++;
	}

	/// <summary>
	/// Gets the current SciFi Player
	/// </summary>
	/// <returns></returns>
	public SciFiPlayer GetSciFiPlayer() => base.Player as SciFiPlayer;

	internal void UpdateDockLamps()
	{
		var p = GetSciFiPlayer();
		if (p.DockEnabled) SetLampState("dock_light", 2); else SetLampState("dock_light", 0);
		SetLampState("dock_50k", (byte)p.Dock50K);
		SetLampState("dock_special", (byte)p.DockSpecial);
		SetLampState("dock_extra_ball", (byte)p.DockExtraBall);
		if (p.DockSpecial == GameOption.Complete)
		{
			SetLampState("outlane_l", 1); SetLampState("outlane_r", 1);
		}
		else
		{
			SetLampState("outlane_l", 0); SetLampState("outlane_r", 0);
		}
	}

	private void ResetAlienLamps(byte state)
	{
		SetLampState("alien_1", state); SetLampState("alien_2", state);
		SetLampState("alien_3", state); SetLampState("alien_4", state);
		SetLampState("alien_5", state); SetLampState("alien_6", state);
		SetLampState("alien_7", state); SetLampState("alien_8", state);
		SetLampState("alien_9", state);
		SetLampState("alien_10", state); SetLampState("alien_20", state);
	}

	private void ResetDefenderLamps(byte state)
	{
		SetLampState("defend_1", state); SetLampState("defend_2", state);
		SetLampState("defend_3", state); SetLampState("defend_4", state);
		SetLampState("defend_5", state); SetLampState("defend_6", state);
		SetLampState("defend_7", state); SetLampState("defend_8", state);
		SetLampState("defend_9", state); SetLampState("defend_10", state);
		SetLampState("defend_20", state);
	}

	public void UpdateAlienLamps()
	{
		ResetAlienLamps(0);

		var p = GetSciFiPlayer();
		if (p.AlienBonus >= 20)
		{
			ResetAlienLamps(2); //blink all lamps
		}
		else if (p.AlienBonus >= 10)
		{
			SetLampState("alien_10", 1);
		}
		else
		{
			if (p.AlienBonus >= 9) SetLampState("alien_9", 1);
			if (p.AlienBonus >= 8) SetLampState("alien_8", 1);
			if (p.AlienBonus >= 7) SetLampState("alien_7", 1);
			if (p.AlienBonus >= 6) SetLampState("alien_6", 1);
			if (p.AlienBonus >= 5) SetLampState("alien_5", 1);
			if (p.AlienBonus >= 4) SetLampState("alien_4", 1);
			if (p.AlienBonus >= 3) SetLampState("alien_3", 1);
			if (p.AlienBonus >= 2) SetLampState("alien_2", 1);
			if (p.AlienBonus >= 1) SetLampState("alien_1", 1);
		}
	}

	public void UpdateDefenderLamps()
	{
		ResetDefenderLamps(0);

		var player = GetSciFiPlayer();
		if (player.DefenderBonus >= 20)
		{
			ResetDefenderLamps(2); //blink all lamps
		}
		else if (player.DefenderBonus >= 10)
		{
			SetLampState("defend_10", 1);
		}
		else
		{
			if (player.DefenderBonus >= 9) SetLampState("defend_9", 1);
			if (player.DefenderBonus >= 8) SetLampState("defend_8", 1);
			if (player.DefenderBonus >= 7) SetLampState("defend_7", 1);
			if (player.DefenderBonus >= 6) SetLampState("defend_6", 1);
			if (player.DefenderBonus >= 5) SetLampState("defend_5", 1);
			if (player.DefenderBonus >= 4) SetLampState("defend_4", 1);
			if (player.DefenderBonus >= 3) SetLampState("defend_3", 1);
			if (player.DefenderBonus >= 2) SetLampState("defend_2", 1);
			if (player.DefenderBonus >= 1) SetLampState("defend_1", 1);
		}
	}

	internal void UpdateSciFiLamps()
	{
		var p = GetSciFiPlayer();
		if (p.Scifi[0]) SetLampState("Scifi", 0); else SetLampState("Scifi", 1);
		if (p.Scifi[1]) SetLampState("sCifi", 0); else SetLampState("sCifi", 1);
		if (p.Scifi[2]) SetLampState("scIfi", 0); else SetLampState("scIfi", 1);
		if (p.Scifi[3]) SetLampState("sciFi", 0); else SetLampState("sciFi", 1);
		if (p.Scifi[4]) SetLampState("scifI", 0); else SetLampState("scifI", 1);
	}

	public void UpdatePowerupModeLamps()
	{
		var p = GetSciFiPlayer();
		var lPowerLit = (byte)p.LeftPowerUpLit;
		SetLampState("power_left", lPowerLit);
		SetLampState("star_left_1", lPowerLit);
		SetLampState("star_left_2", lPowerLit);
		SetLampState("star_left_3", lPowerLit);

		SetLampState("mode_spawn", (byte)p.SpawnEnabled);
		SetLampState("mode_invasion", (byte)p.InvasionEnabled);
		SetLampState("mode_armada", (byte)p.ArmadaEnabled);
		SetLampState("mode_alienbane", (byte)p.AlienBaneEnabled);

		if (p.SpawnEnabled == GameOption.Complete)
		{
			SetLampState("bumper_0", 1);
			SetLampState("bumper_1", 1);
			SetLampState("bumper_2", 1);
		}
		else
		{
			SetLampState("bumper_0", 0);
			SetLampState("bumper_1", 0);
			SetLampState("bumper_2", 0);
		}

		if (p.SpawnEnabled == GameOption.Complete)
		{
			SetLampState("outlane_l", 1);
			SetLampState("outlane_r", 1);
			SetLampState("bank3_1", 2);
			SetLampState("bank3_2", 2);
			SetLampState("bank3_3", 2);
		}
		if (p.ArmadaEnabled == GameOption.Complete)
		{
			SetLampState("star_left_1", 1);
			SetLampState("star_left_2", 1);
			SetLampState("star_left_3", 1);
			SetLampState("star_middle", 1);
			SetLampState("star_right", 1);
			SetLampState("bank4_1", 2);
			SetLampState("bank4_2", 2);
			SetLampState("bank4_3", 2);
			SetLampState("bank4_4", 2);
		}
		if (p.AlienBaneEnabled == GameOption.Complete)
		{
			SetLampState("mode_alienbane", 1);
			SetLampState("mode_spawn", 2);
			SetLampState("mode_invasion", 2);
			SetLampState("mode_armada", 2);
		}

	}

	/// <summary>
	/// Resets drop target lamps and banks
	/// </summary>
	public void ResetTargets()
	{
		SolenoidPulse("bank3");
		SolenoidPulse("bank4");

		SetLampState("bank3_1", 1);
		SetLampState("bank3_2", 1);
		SetLampState("bank3_3", 1);

		SetLampState("bank4_1", 1);
		SetLampState("bank4_2", 1);
		SetLampState("bank4_3", 1);
		SetLampState("bank4_4", 1);
	}

	public bool SpecialLit { get; set; }
	public bool KickbackEnabled { get; internal set; }
}