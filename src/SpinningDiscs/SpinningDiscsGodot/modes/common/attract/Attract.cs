using Godot;
using System.Collections.Generic;

/// <summary>
/// Adds all Canvas Items in the "AttractLayers". These cycle on a timer and can be cycled with Flipper actions
/// </summary>
public class Attract : Control
{
	/// <summary>
	/// The amount of time to change a scene
	/// </summary>
	[Export] byte _scene_change_secs = SceneChangeTime;

	#region Fields
	const byte SceneChangeTime = 5;
	int _currentScene = 0;
	int _lastScene = 0;
	private PinGodGame pinGod;
	List<CanvasItem> Scenes = new List<CanvasItem>();
	private Timer timer;
	#endregion

	public override void _EnterTree()
	{
		pinGod = (GetNode("/root/PinGodGame") as PinGodGame);
		timer = (GetNode("AttractLayerChangeTimer") as Timer);

		var nodes = GetNode("AttractLayers").GetChildren();
		//add as canvas items as they are able to Hide / Show
		foreach (var item in nodes)
		{
			var cItem = item as CanvasItem;
			if (cItem != null)
			{
				Scenes.Add(cItem);
			}
		}
	}

	/// <summary>
	/// Checks for start button presses to Start a game. Players can cycle the scenes with flippers
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (pinGod.SwitchOn("start", @event))
		{
			var started = pinGod.StartGame();
			if (started)
			{
				pinGod.LogInfo("attract: game started");
			}
			pinGod.LogInfo("attract: starting game. started?", started);
		}
		if (pinGod.SwitchOn("flipper_l", @event))
		{
			CallDeferred("ChangeLayer", false);
		}
		if (pinGod.SwitchOn("flipper_r", @event))
		{

			CallDeferred("ChangeLayer", true);
		}
	}

	public override void _Ready()
	{
		pinGod.LogInfo("Attract loaded");
	}

	/// <summary>
	/// Switches the scenes visibility on a timer. Plays lamp seq in VP
	/// </summary>
	private void _on_Timer_timeout()
	{
		CallDeferred(nameof(ChangeLayer), true);
	}

	/// <summary>
	/// Changes the attract layer. Cycles the AttractLayers in the scene
	/// </summary>
	/// <param name="forward"></param>
	private void ChangeLayer(bool forward = false)
	{
		if (Scenes?.Count < 1) return;

		timer.Stop();

		//check if lower higher than our attract layers
		_currentScene = forward ? _currentScene + 1 : _currentScene - 1;
		pinGod.LogDebug("change layer forward: ", forward, " scene", _currentScene);

		_currentScene = _currentScene > Scenes?.Count - 1 ? 0 : _currentScene;
		_currentScene = _currentScene < 0 ? Scenes?.Count - 1 ?? 0 : _currentScene;

		//hide the last layer and show new index
		Scenes[_lastScene].Hide(); //Scenes[_lastScene].Visible = false;
		Scenes[_currentScene].Show();// Scenes[_currentScene].Visible = true;

		_lastScene = _currentScene;

		timer.Start(_scene_change_secs);
	}
}
