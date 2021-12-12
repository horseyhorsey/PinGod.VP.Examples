using Godot;
using Godot.Collections;

/// <summary>
/// Godot singleton (AUTOLOAD) to hold Common pinball variables and methods. Add everything you want scenes to use globally here. <see cref="PinGodGameBase"/>
/// </summary>
public class PinGodGame : PinGodGameBase
{
	[Export] bool _write_machine_states = true;
	[Export] bool _read_machine_states = true;
	[Export] int _write_machine_states_delay = 10;

	[Export] protected string COIN_SFX = "res://addons/PinGodGame/assets/audio/sfx/credit.wav";		
	[Export] protected string TILT_SFX = "res://addons/PinGodGame/assets/audio/sfx/tilt.wav";
	[Export] protected string WARN_SFX = "res://addons/PinGodGame/assets/audio/sfx/tilt_warning.wav";

	#region Exports
	[Export] PinGodLogLevel _logging_level = PinGodLogLevel.Info;
	[Export] bool _lamp_overlay_enabled = false;
	[Export] bool _switch_overlay_enabled = false;
	[Export] bool _record_game = false;
	[Export] bool _playback_game = false;
	[Export] string _playbackfile = null;

	#endregion	

	/// <summary>
	/// Runs setup for everything in the `machine`. Trough, ball-search etc <see cref="Setup"/>
	/// </summary>
	public override void _EnterTree()
	{
		base._EnterTree(); //setup base for trough

		Logger.LogLevel = _logging_level;

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

		Connect(nameof(ServiceMenuEnter), this, "OnServiceMenuEnter");

		//setup and run writing memory states for other application to access
		if (_write_machine_states || _read_machine_states)
		{
			LogInfo("pingod:writing machine states is enabled");
			memMapping = new MemoryMap();
			memMapping.Start(_write_machine_states_delay);
		}

		//set up recording / playback
		SetUpRecordingsOrPlayback(_playback_game, _record_game, _playbackfile);

		//remove the overlays if disabled
		SetupDevOverlays();		
	}

	/// <summary>
	/// Use to add your own player based on the selected class script
	/// </summary>
	/// <param name="name"></param>
	public override void CreatePlayer(string name)
	{
		base.CreatePlayer(name);
	}

	/// <summary>
	/// Adds coin SFX under credit. Use to add custom sounds and music to the <see cref="this.AudioManager"/>
	/// </summary>
	public virtual void AddAudioStreams()
	{
		////adds the default credit sound
		AudioManager.AddSfx(COIN_SFX, "credit");
		AudioManager.AddSfx(TILT_SFX, "tilt");
		AudioManager.AddSfx(WARN_SFX, "warning");

		AudioManager.AddSfx("res://addons/PinGodGame/assets/audio/sfx/beep.wav", "enter");
		AudioManager.AddSfx("res://addons/PinGodGame/assets/audio/sfx/beep_long.wav", "exit");

		////add music for the game. Ogg to autoloop
		////AudioManager.AddMusic("res://assets/audio/music/mymusic.ogg", "mymusic");
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
		if(devOverlays != null)
        {
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
}
