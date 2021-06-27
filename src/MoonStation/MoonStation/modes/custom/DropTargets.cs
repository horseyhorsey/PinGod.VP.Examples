using Godot;
using System.Collections.Generic;
using System.Linq;
using static Godot.GD;

public class DropTargets : Control
{
	private AudioStreamPlayer voicePlayer;
	private Dictionary<string, AudioStream> voices;
	private PinGodGame pinGod;
	private Game game;

	public override void _EnterTree()
	{
		//player to play voices with. WAV so they don't loop automatically
		voicePlayer = GetNode("AudioStreamPlayer") as AudioStreamPlayer;

		//load all voices to dict
		voices = new Dictionary<string, AudioStream>();
		var vDir = "res://assets/audio/voice";
		var chars = new string[] { "m", "o", "n", "s", "t", "a", "i" };
		for (int i = 0; i < chars.Length; i++)
		{
			voices.Add(chars[i], Load(vDir + $"/{chars[i]}.wav") as AudioStream);
		}

		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		pinGod.Connect("BallEnded", this, "ResetTargets");

		game = GetNode("/root/MainScene/Modes/Game") as Game;
	}

	public override void _Ready()
	{
		ResetTargets(false);
	}

	public void ResetTargets(bool lastBall)
	{
		game.Multiplier = 1;		
		ResetMoon();
		ResetStation();
		game.UpdateLamps();
	}

	private void ResetStation()
	{
		pinGod.SolenoidPulse("drops_r"); // reset station Drops
		game.StationTargets = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
	}

	private void ResetMoon()
	{
		game.MoonTargets = new byte[4] { 0, 0, 0, 0 };
		pinGod.SolenoidPulse("drops_l"); // reset moon Drops
	}

	/// <summary>
	/// Each time Moon target is hit, run a check if all complete to increase multiplier
	/// </summary>
	/// <returns></returns>
	private bool MoonCheck()
	{
		pinGod.AddPoints(250* game.Multiplier);

		if (!game.MoonTargets.Any(x => x == 0))
		{
			pinGod.LogInfo("Moon drops completed. PF multiplier added");
			game.Multiplier++;
			game.UpdateLamps();
			ResetMoon();
			return true;
		}
		return false;
	}

	/// <summary>
	/// Each time a Station target is hit, run a check if all complete to increase multiplier
	/// </summary>
	/// <returns></returns>
	private bool StationCheck()
	{
		pinGod.AddPoints(250 * game.Multiplier);

		if (!game.StationTargets.Any(x => x == 0))
		{
			pinGod.LogInfo("Station drops completed. PF multiplier added");
			game.Multiplier++;
			game.UpdateLamps();
			ResetStation();
			return true;
		}

		return false;
	}
	
	public override void _Input(InputEvent @event)
	{
		if (pinGod.SwitchOn("Moon", @event))
		{
			pinGod.LogDebug("sw: M");
			game.MoonTargets[0] = 1;
			voicePlayer.Stream = voices["m"]; voicePlayer.Play();
			MoonCheck();
		}
		if (pinGod.SwitchOn("mOon", @event))
		{
			pinGod.LogDebug("sw: O");
			game.MoonTargets[1] = 1;
			voicePlayer.Stream = voices["o"]; voicePlayer.Play();
			MoonCheck();
		}
		if (pinGod.SwitchOn("moOn", @event))
		{
			pinGod.LogDebug("sw: Ob");
			game.MoonTargets[2] = 1;
			voicePlayer.Stream = voices["o"]; voicePlayer.Play();
			MoonCheck();
		}
		if (pinGod.SwitchOn("mooN", @event))
		{
			pinGod.LogDebug("sw: N");
			game.MoonTargets[3] = 1;
			voicePlayer.Stream = voices["n"]; voicePlayer.Play();
			MoonCheck();
		}

		if (pinGod.SwitchOn("Station", @event))
		{
			pinGod.LogDebug("sw: S");
			game.StationTargets[0] = 1;
			voicePlayer.Stream = voices["s"]; voicePlayer.Play();
			StationCheck();
		}
		if (pinGod.SwitchOn("sTation", @event))
		{
			game.StationTargets[1] = 1;
			pinGod.LogDebug("sw: T");
			voicePlayer.Stream = voices["t"]; voicePlayer.Play();
			StationCheck();
		}
		if (pinGod.SwitchOn("stAtion", @event))
		{
			pinGod.LogDebug("sw: A");
			game.StationTargets[2] = 1;
			voicePlayer.Stream = voices["a"]; voicePlayer.Play();
			StationCheck();
		}
		if (pinGod.SwitchOn("staTion", @event))
		{
			pinGod.LogDebug("sw: T");
			game.StationTargets[3] = 1;
			voicePlayer.Stream = voices["t"]; voicePlayer.Play();
			StationCheck();
		}
		if (pinGod.SwitchOn("statIon", @event))
		{
			game.StationTargets[4] = 1;
			pinGod.LogDebug("sw: I");
			voicePlayer.Stream = voices["i"]; voicePlayer.Play();
			StationCheck();
		}
		if (pinGod.SwitchOn("statiOn", @event))
		{
			game.StationTargets[5] = 1;
			pinGod.LogDebug("sw: O");
			voicePlayer.Stream = voices["o"]; voicePlayer.Play();
			StationCheck();
		}
		if (pinGod.SwitchOn("statioN", @event))
		{
			game.StationTargets[6] = 1;
			pinGod.LogDebug("sw: N");
			voicePlayer.Stream = voices["n"]; voicePlayer.Play();
			StationCheck();
		}
	}
}
