using Godot;
using System;
using System.Collections.Generic;
using static Godot.GD;

/// <summary>
/// Godot singleton (AUTOLOAD) to hold Common pinball variables and methods. Add everything you want scenes to use globally here. <see cref="PinGodGameBase"/>
/// </summary>
public class PinGodGame : PinGodGameBase
{
	const string COIN_SFX = "res://assets/audio/sfx/credit.wav";
	AudioStreamPlayer sfxPlayer;
	Dictionary<string, AudioStream> Sounds = new Dictionary<string, AudioStream>();

	bool init = false;

    public override void _EnterTree()
    {
        base._EnterTree();
    }

    public override void _Ready()
	{
		sfxPlayer = new AudioStreamPlayer();
		Sounds.Add("credit", Load(COIN_SFX) as AudioStream);
		AddChild(sfxPlayer);
		Connect(nameof(ServiceMenuEnter), this, "OnServiceMenuEnter");
		AddCustomSwitches(); AddCustomSolenoids();
		init = true;
	}

	public override void _Input(InputEvent @event)
	{
		//Coin button. See PinGod.vbs for Standard switches
		if (SwitchOn("coin", @event))
		{
			sfxPlayer.Stream = Sounds["credit"];
			sfxPlayer.Play();
			AddCredits(1);
		}
	}

	public void AddCredits(byte amt)
	{
		GameData.Credits += amt;
		EmitSignal(nameof(CreditAdded));
	}

	/// <summary>
	/// Add extra solenoids to the <see cref="Machine.Coils"/>. Invoked when ready
	/// </summary>
	void AddCustomSolenoids()
	{
		if (!init)
		{
			Machine.Coils.Add("disable_shows",  33);
			Machine.Coils.Add("lampshow_1",		34);
			Machine.Coils.Add("lampshow_2",		35);
		}
	}

	/// <summary>
	/// Add switches to the <see cref="Machine.Switches"/>. Invoked when ready
	/// </summary>
	void AddCustomSwitches()
	{
		if (!init)
		{
			Machine.Switches.Add("mball_saucer", new Switch(27));
		}		
	}

	/// <summary>
	/// Stops any game in progress
	/// </summary>
	void OnServiceMenuEnter()
	{
		GameInPlay = false;
		ResetTilt();
		EnableFlippers(0);
	}
}
