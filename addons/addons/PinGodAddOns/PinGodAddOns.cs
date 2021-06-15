#if TOOLS
using Godot;
using System;

[Tool]
public class PinGodAddOns : EditorPlugin
{
	const string ROOT_DIR = "addons/PinGodAddOns/";

	/// <summary>
	/// Initialization of the plugin goes here. Add the new type with a name, a parent type, a script and an icon.
	/// </summary>
	public override void _EnterTree()
	{
		var script = GD.Load<Script>(ROOT_DIR+"Labels/BlinkingLabel.cs");
		AddCustomType("BlinkingLabel", "Label", script, null);

		script = GD.Load<Script>(ROOT_DIR + "VideoPlayers/VideoPlayerPinball.cs");
		AddCustomType("VideoPlayerPinball", "VideoPlayer", script, null);

		script = GD.Load<Script>(ROOT_DIR + "Timers/BallStackPinball.cs");
		AddCustomType("BallStackPinball", "Timer", script, null);
	}

	/// <summary>
	/// Clean-up of the plugin goes here. Always remember to remove it from the engine when deactivated.
	/// </summary>
	public override void _ExitTree()
	{
		RemoveCustomType("BlinkingLabel");
		RemoveCustomType("BallStackPinball");
		RemoveCustomType("VideoPlayerPinball");
	}
}
#endif
