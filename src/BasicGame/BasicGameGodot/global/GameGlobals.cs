using Godot;
using System;
using System.Collections.Generic;
using static Godot.GD;

/// <summary>
/// Godot singleton to hold Common pinball variables. Add everything you want scenes to use globally here
/// </summary>
public class GameGlobals : PinGodGameBase
{
	AudioStreamPlayer sfxPlayer;
	Dictionary<string, AudioStream> Sounds = new Dictionary<string, AudioStream>();

	public static byte FlipperRSwitchNum { get; set; } = 9;
	public static byte FlipperLSwitchNum { get; set; } = 11;

	public override void _Ready()
	{
		sfxPlayer = new AudioStreamPlayer();
		Sounds.Add("credit", Load("res://assets/audio/sfx/credit.wav") as AudioStream);
		AddChild(sfxPlayer);

		Connect(nameof(ServiceMenuEnter), this, "OnServiceMenuEnter");
	}

	public override void _Input(InputEvent @event)
	{
		//Coin button. See PinGod.vbs for Standard switches
		if (@event.IsActionPressed("sw" + CreditButtonNum))
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
	/// Stops any game in progress
	/// </summary>
	void OnServiceMenuEnter()
	{
		GameInPlay = false;
		ResetTilt();
		EnableFlippers(0);
		Trough.DisableBallSave();
	}
}
