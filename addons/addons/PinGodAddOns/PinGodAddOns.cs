#if TOOLS
using Godot;

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
		AddCustomType(nameof(BlinkingLabel), nameof(Label), script, null);

		script = GD.Load<Script>(ROOT_DIR + "VideoPlayers/VideoPlayerPinball.cs");
		AddCustomType(nameof(VideoPlayerPinball), nameof(VideoPlayer), script, null);

		script = GD.Load<Script>(ROOT_DIR + "Timers/BallStackPinball.cs");
		AddCustomType(nameof(BallStackPinball), nameof(Timer), script, null);

		script = GD.Load<Script>(ROOT_DIR + "Lanes/PinballLanesNode.cs");
		AddCustomType(nameof(PinballLanesNode), nameof(Node), script, null);

		script = GD.Load<Script>(ROOT_DIR + "Targets/PinballTargetsBank.cs");
		AddCustomType(nameof(PinballTargetsBank), nameof(Node), script, null);
	}

	/// <summary>
	/// Clean-up of the plugin goes here. Always remember to remove it from the engine when deactivated.
	/// </summary>
	public override void _ExitTree()
	{
		RemoveCustomType(nameof(BlinkingLabel));
		RemoveCustomType(nameof(BallStackPinball));		
		RemoveCustomType(nameof(PinballLanesNode));
		RemoveCustomType(nameof(PinballTargetsBank));
		RemoveCustomType(nameof(VideoPlayerPinball));
	}
}
#endif
