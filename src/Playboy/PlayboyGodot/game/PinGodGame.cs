using Godot;
using Godot.Collections;
using System;
using System.Linq;
using static Godot.GD;

/// <summary>
/// Godot singleton (AUTOLOAD) to hold Common pinball variables and methods. Add everything you want scenes to use globally here. <see cref="PinGodGameBase"/>
/// </summary>
public class PinGodGame : PinGodGameBase
{	
	const string COIN_SFX = "res://assets/audio/sfx/credit.wav";
	const string LAMP_MATRIX_SCENE = "res://addons/PinGodGame/Overlays/Lamps/LampMatrix.tscn";

	#region Exports
	[Export] PinGodLogLevel _logging_level = PinGodLogLevel.Info;
	[Export] bool _lamp_overlay_enabled = false;
	[Export] bool _switch_overlay_enabled = false;
	[Export] bool _record_game = false;
	[Export] bool _playback_game = false;
	[Export] string _playbackfile = null;
	[Export] bool _write_machine_states = true;
	[Export] bool _read_machine_states = true;
	[Export] int _write_machine_states_delay = 10;

	[Export] Dictionary<string, byte> _coils = new Dictionary<string, byte>();
	[Export] Dictionary<string, byte> _switches = new Dictionary<string, byte>();
	[Export] Dictionary<string, byte> _lamps = new Dictionary<string, byte>();
	[Export] Dictionary<string, byte> _leds = new Dictionary<string, byte>();

	[Export] public string[] _trough_switches = { "trough_1", "trough_2", "trough_3", "trough_4" };
	[Export] public string[] _early_save_switches = { "outlane_l", "outlane_r" };
	[Export] public string _trough_solenoid = "trough";
	[Export] public string _auto_plunge_solenoid = "auto_plunger";

	[Export] public string _plunger_lane_switch = "plunger_lane";
	[Export] public string _ball_save_lamp = "";
	[Export] public string _ball_save_led = "shoot_again";
	[Export] public byte _ball_save_seconds = 8;
	[Export] public byte _ball_save_multiball_seconds = 8;
	[Export] public byte _number_of_balls_to_save = 1;
	[Export] public bool _ball_search_enabled = true;
	[Export] public string[] _ball_search_coils;
	[Export] public string[] _ball_search_stop_switches;
	[Export] private int _ball_search_wait_time_secs = 10;
	#endregion

	/// <summary>
	/// Runs setup for everything in the `machine`. Trough, ball-search etc <see cref="Setup"/>
	/// </summary>
	public override void _EnterTree()
	{
		base._EnterTree(); //setup base for trough

		Logger.LogLevel = _logging_level;

		//trough
		_trough.TroughOptions = new TroughOptions(_trough_switches, _trough_solenoid, _plunger_lane_switch,
			_auto_plunge_solenoid, _early_save_switches, _ball_save_seconds, _ball_save_multiball_seconds, _ball_save_lamp, _ball_save_led, _number_of_balls_to_save);
		//ball search options
		BallSearchOptions = new BallSearchOptions(_ball_search_coils, _ball_search_stop_switches, _ball_search_wait_time_secs, _ball_search_enabled);

		LogDebug("PinGod: entering tree. Setup");
		Setup();
	}

	/// <summary>
	/// Save game data / settings / recordings before exit
	/// </summary>
	public override void _ExitTree()
	{
		base._ExitTree();
		Quit(true);
	}

	public override void _Ready()
	{
		base._Ready();
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event); // process window events

	}

	/// <summary>
	/// Sets up machine items from the collections, starts memory mapping
	/// </summary>
	public override void Setup()
	{
		base.Setup();

		//load audio streams, music / sfx / vox
		AddAudioStreams();

		//add custom coils and switches for this game which can be set in the PinGodGame.tscn scene UI or file
		AddCustomMachineItems(_coils, _switches, _lamps, _leds);

		Connect(nameof(ServiceMenuEnter), this, "OnServiceMenuEnter");

		//setup and run writing memory states for other application to access
		if (_write_machine_states || _read_machine_states)
		{
			LogInfo("pingod:read or write machine states is enabled");
			memMapping = new MemoryMap();
			memMapping.Start(_write_machine_states_delay);
		}

		//set up recording / playback
		SetUpRecordingsOrPlayback(_playback_game, _record_game, _playbackfile);


		//remove the overlays if disabled
		SetupDevOverlays();

	}

	/// <summary>
	/// Use to add your own player based on <see cref="PinGodPlayer"/>
	/// </summary>
	/// <param name="name"></param>
	public override void CreatePlayer(string name)
	{
		Players.Add(new PlayboyPlayer() { Name = name });
	}

	void AddAudioStreams()
	{
		//adds the default credit sound
		AudioManager.AddSfx(COIN_SFX, "credit");

		//add music for the game. Ogg to autoloop
		AudioManager.AddMusic("res://assets/audio/music/cook_loop1.ogg", "cook_loop_1");
		AudioManager.AddMusic("res://assets/audio/music/cook_loop2.ogg", "cook_loop_2");
	}

	public void GetRandomVoiceOrg()
	{
		
	}

	public void PlaySoundDrain()
	{

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

	/// <summary>
	/// Sets up the DevOverlays Tree. Enables / Disable helper overlays for lamps and switches
	/// </summary>
	private void SetupDevOverlays()
	{
		var devOverlays = GetNode("DevOverlays");
		if (!_lamp_overlay_enabled)
		{
			LogInfo("pingod: removing lamp overlay");
			devOverlays.GetNode("LampMatrix").QueueFree();
		}
		else
		{
			_lampMatrixOverlay = devOverlays.GetNode("LampMatrix") as LampMatrix;		
		}

		if (!_switch_overlay_enabled)
		{
			LogInfo("pingod: removing switch overlay");
			devOverlays.GetNode("SwitchOverlay").QueueFree();
		}
	}

}
