using Godot;
using System.Collections.Generic;
using static Godot.GD;
using System.Linq;

/// <summary>
/// Adds all Canvas Items in the "AttractLayers". These cycle on a timer and can be cycled with Flipper actions
/// </summary>
public class Attract : Node2D
{
	const byte SceneChangeTime = 5;
	[Export] byte _scene_change_secs = SceneChangeTime;
	int _currentScene = 0; int _lastScene = 0;
	private PinGodGame pinGodGame;

	private Timer timer;
	List<CanvasItem> Scenes = new List<CanvasItem>();

	public override void _EnterTree()
	{
		pinGodGame = (GetNode("/root/PinGodGame") as PinGodGame);
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
	/// Checks for start button presses to Start a game.
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (pinGodGame.SwitchOn("start", @event))
		{
			var started = pinGodGame.StartGame();
			if (started)
			{
				pinGodGame.SolenoidPulse("disable_shows");
			}
			Print("attract: starting game. started?", started);
		}
		if (pinGodGame.SwitchOn("flipper_l", @event))
		{
			timer.Stop();
			ChangeLayer(false);
			timer.Start(_scene_change_secs);
		}
		if (pinGodGame.SwitchOn("flipper_r", @event))
		{
			timer.Stop();
			ChangeLayer(true);
			timer.Start(_scene_change_secs);
		}
	}

	private void ChangeLayer(bool forward = false)
	{
		if (Scenes?.Count < 1) return;

		//check if lower higher than our attract layers
		_currentScene = forward ? _currentScene + 1 : _currentScene - 1;
		Print("change layer forward: ", forward, " scene", _currentScene);

		_currentScene = _currentScene > Scenes?.Count - 1 ? 0 : _currentScene;
		_currentScene = _currentScene < 0 ? Scenes?.Count - 1 ?? 0 : _currentScene;

		//hide the last layer and show new index
		Scenes[_lastScene].Hide(); //Scenes[_lastScene].Visible = false;
		Scenes[_currentScene].Show();// Scenes[_currentScene].Visible = true;

		_lastScene = _currentScene;
	}

	public override void _Ready()
	{
		Print("Attract loaded");
		pinGodGame.SolenoidPulse("disable_shows");
	}

	/// <summary>
	/// Just switches the scenes visibility on a timer. Plays lamp seq in VP
	/// </summary>
	private void _on_Timer_timeout()
	{
		CallDeferred(nameof(ChangeLayer), true);
		pinGodGame.SolenoidPulse("lampshow_1");
	}
}
