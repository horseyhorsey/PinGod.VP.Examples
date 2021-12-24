using Godot;
using Godot.Collections;
using System;

/// <summary>
/// Godot singleton (AUTOLOAD) to hold Common pinball variables and methods. Add everything you want scenes to use globally here. <see cref="PinGodGameBase"/>
/// </summary>
public class PinGodGame : PinGodGameBase
{
	[Export] bool _write_machine_states = true;
	[Export] bool _read_machine_states = true;
	[Export] int _write_machine_states_delay = 10;

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
	public void Setup()
	{
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
