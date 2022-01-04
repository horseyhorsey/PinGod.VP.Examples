using Godot;
using Godot.Collections;
using System;

/// <summary>
/// Godot singleton (AUTOLOAD) to hold Common pinball variables and methods. Add everything you want scenes to use globally here. <see cref="PinGodGameBase"/>
/// </summary>
public class PinGodGame : PinGodGameBase
{
	#region Exports
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
		//setup base for everything and trough etc
		base._EnterTree();

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
	/// Sets up machine items from the collections, starts memory mapping and recordings
	/// </summary>
	public virtual void Setup()
	{
		Logger.LogLevel = GameSettings.LogLevel;
		LogDebug("PinGod: entering tree. Setup");

		Connect(nameof(ServiceMenuEnter), this, "OnServiceMenuEnter");		

		//setup and run writing memory states for other application to access
		if (GameSettings.MachineStatesWrite || GameSettings.MachineStatesRead)
		{
			LogInfo("pingod:writing machine states is enabled");
			memMapping = new MemoryMap();
			memMapping.Start(GameSettings.MachineStatesWriteDelay);
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
