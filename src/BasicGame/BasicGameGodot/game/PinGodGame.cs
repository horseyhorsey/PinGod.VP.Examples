using Godot;
using static Godot.GD;

/// <summary>
/// Godot singleton (AUTOLOAD) to hold Common pinball variables and methods. Add everything you want scenes to use globally here. <see cref="PinGodGameBase"/>
/// </summary>
public class PinGodGame : PinGodGameBase
{	
	const string COIN_SFX = "res://assets/audio/sfx/credit.wav";
	const string AUDIO_MANAGER = "res://addons/PinGodGame/Audio/AudioManager.tscn";

	public override void _EnterTree()
    {
        Print("PinGod: entering tree. Loading audiomanager");

        //create and get ref to the audiomanager scene
        var audioMan = Load(AUDIO_MANAGER) as PackedScene;
        AddChild(audioMan.Instance());
        AudioManager = GetNode("AudioManager") as AudioManager;
        Print("PinGod: audiomanager loaded.", AudioManager != null);

        //set to false, no music in this particular game
        AudioManager.MusicEnabled = false;
        AudioManager.Bgm = string.Empty;

        //load audio streams, music / sfx / vox
        AddAudioStreams();
        //add custom coils and switches for this game
        AddCustomMachineItems();

        Connect(nameof(ServiceMenuEnter), this, "OnServiceMenuEnter");

        Print("Starting Pinball Sender");
        PinballSender.Start();
    }

    /// <summary>
    /// Save game data / settings before exit
    /// </summary>
    public override void _ExitTree()
	{		
		GameData.Save(GameData);
		GameSettings.Save(GameSettings);

		Print("exiting pinball sender");		
		PinballSender.Stop();
		Print("exited pinball sender");
	}

	public override void _Input(InputEvent @event)
	{
		//quits the game. ESC
		if (@event.IsActionPressed("quit"))
		{
			GetTree().Quit(0);
		}

		//Coin button. See PinGod.vbs for Standard switches
		if (SwitchOn("coin", @event))
		{
			AudioManager.PlaySfx("credit");
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
		Machine.Coils.Add("disable_shows", new PinStateObject(33));
		Machine.Coils.Add("lampshow_1", new PinStateObject(34));
		Machine.Coils.Add("lampshow_2", new PinStateObject(35));
	}

	/// <summary>
	/// Add switches to the <see cref="Machine.Switches"/>. Invoked when ready
	/// </summary>
	void AddCustomSwitches()
	{
		Machine.Switches.Add("mball_saucer", new Switch(27));
	}

	/// <summary>
	/// Add custom leds to the <see cref="Machine.Leds"/>. Invoked when ready
	/// </summary>
	void AddCustomLeds()
	{
		
	}

	/// <summary>
	/// Add custom lamps to the <see cref="Machine.Lamps"/>. Invoked when ready
	/// </summary>
	void AddCustomLamps()
	{

	}

	void AddAudioStreams()
    {
		//adds the default credit sound
		AudioManager.AddSfx(COIN_SFX, "credit");

		//add music for the game. Ogg to autoloop
		//AudioManager.AddMusic("res://assets/audio/music/mymusic.ogg", "mymusic");
	}

	private void AddCustomMachineItems()
	{
		AddCustomSwitches(); 
		AddCustomSolenoids();
		AddCustomLeds();
		AddCustomLamps();
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
