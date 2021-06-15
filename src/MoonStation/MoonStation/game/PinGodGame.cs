using Godot;
using static Godot.GD;

/// <summary>
/// Godot singleton (AUTOLOAD) to hold Common pinball variables and methods. Add everything you want scenes to use globally here. <see cref="PinGodGameBase"/>
/// </summary>
public class PinGodGame : PinGodGameBase
{	
	const string COIN_SFX = "res://assets/audio/sfx/credit.wav";
	const string AUDIO_MANAGER = "res://addons/PinGodGame/Audio/AudioManager.tscn";

	#region Exports
	[Export] bool _write_machine_states = true;
	[Export] int _write_machine_states_delay = 10;

	[Export] Godot.Collections.Dictionary<string, byte> _coils = new Godot.Collections.Dictionary<string, byte>();
	[Export] Godot.Collections.Dictionary<string, byte> _switches = new Godot.Collections.Dictionary<string, byte>();
	[Export] Godot.Collections.Dictionary<string, byte> _lamps = new Godot.Collections.Dictionary<string, byte>();
	[Export] Godot.Collections.Dictionary<string, byte> _leds = new Godot.Collections.Dictionary<string, byte>();

	[Export] public string _trough_entry_switch_name = "trough_4";
	[Export] public string[] _trough_switches = { "trough_1", "trough_2", "trough_3", "trough_4" };
	[Export] public string[] _early_save_switches = { "outlane_l", "outlane_r" };
	[Export] public string _trough_solenoid = "trough";
	[Export] public string _auto_plunge_solenoid = "auto_plunge";
	[Export] public string _plunger_lane_switch = "plunger_lane";
	[Export] public string _ball_save_lamp = "";
	[Export] public string _ball_save_led = "shoot_again";
	[Export] public byte _ball_save_seconds = 8;
	[Export] public byte _ball_save_multiball_seconds = 8;
	[Export] public byte _number_of_balls_to_save = 1;
	#endregion

	public override void _EnterTree()
	{
		base._EnterTree();
		Print("PinGod: entering tree. Setup");
		Setup();
	}

	/// <summary>
	/// Save game data / settings before exit. Stops the switch receiver
	/// </summary>
	public override void _ExitTree()
	{
		base.Quit();
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

	public override void Setup()
	{
		//create and get ref to the audiomanager scene
		var audioMan = Load(AUDIO_MANAGER) as PackedScene;
		AddChild(audioMan.Instance());
		AudioManager = GetNode("AudioManager") as AudioManager;
		Print("PinGod: audiomanager loaded.", AudioManager != null);

		//set default music
		AudioManager.Bgm = "techno";

		//load audio streams, music / sfx / vox
		AddAudioStreams();
		//add custom coils and switches for this game
		AddCustomMachineItems();

		Connect(nameof(ServiceMenuEnter), this, "OnServiceMenuEnter");

		//setup and run writing memory states for other application to access
		if (_write_machine_states)
		{
			memMapping = new MemoryMap();
			Print("pingod:writing machine states is enabled");
			memMapping.Start(_write_machine_states_delay);
		}

		PinballSender.Start();
	}

	#region Custom Machine Items

	private void AddCustomMachineItems()
	{
		AddCustomSwitches();
		AddCustomSolenoids();
		AddCustomLeds();
		AddCustomLamps();
	}
	/// <summary>
	/// Add extra solenoids to the <see cref="Machine.Coils"/>. Invoked when ready
	/// </summary>
	void AddCustomSolenoids()
	{
		foreach (var coil in _coils)
		{
			Machine.Coils.Add(coil.Key, new PinStateObject(coil.Value));
			Print($"pingod: added coil {coil.Key}-{coil.Value}");
		}
		_coils.Clear();
	}
	/// <summary>
	/// Add switches to the <see cref="Machine.Switches"/>. Invoked when ready
	/// </summary>
	void AddCustomSwitches()
	{
		foreach (var sw in _switches)
		{
			Machine.Switches.Add(sw.Key, new Switch(sw.Value));
			Print($"pingod: added switch {sw.Key}-{sw.Value}");
		}
		_switches.Clear();
	}
	/// <summary>
	/// Add custom leds to the <see cref="Machine.Leds"/>. Invoked when ready
	/// </summary>
	void AddCustomLeds()
	{
		foreach (var led in _leds)
		{
			Machine.Leds.Add(led.Key, new PinStateObject(led.Value));
			Print($"pingod: added led {led.Key}-{led.Value}");
		}
		_leds.Clear();
	}
	/// <summary>
	/// Add custom lamps to the <see cref="Machine.Lamps"/>. Invoked when ready
	/// </summary>
	void AddCustomLamps()
	{
		foreach (var lamp in _lamps)
		{
			Machine.Lamps.Add(lamp.Key, new PinStateObject(lamp.Value));
			Print($"pingod: added lamp {lamp.Key}-{lamp.Value}");
		}
		_lamps.Clear();
	}
	#endregion

	void AddAudioStreams()
	{
		//add music for the game
		AudioManager.AddMusic("res://assets/audio/music/ms-music-dnb.ogg", "dnb");
		AudioManager.AddMusic("res://assets/audio/music/ms-music-techno.ogg", "techno");

		//add sfx for the game
		AudioManager.AddSfx(COIN_SFX, "credit");
		AudioManager.AddSfx("res://assets/audio/sfx/Laser_Shoot3-horsepin.wav", "spinner");
		AudioManager.AddSfx("res://assets/audio/sfx/dropT-horsepin.wav", "drops");
		AudioManager.AddSfx("res://assets/audio/sfx/dropTComplete-horsepin.wav", "drops_complete");
		AudioManager.AddSfx("res://assets/audio/sfx/craterhit-horsepin.wav", "crater");
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