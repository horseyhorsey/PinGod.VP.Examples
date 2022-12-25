using Godot;
using System.Collections.Generic;

/// <summary>
/// A basic attract mode that can start a game and cycle scenes with flippers. Add scenes into the "AttractLayers" in scene tree
/// </summary>
public class Attract : Node
{
	/// <summary>
	/// The amount of time to change a scene
	/// </summary>
	[Export] byte _scene_change_secs = SceneChangeTime;

	/// <summary>
	/// 
	/// </summary>
	[Export] float[] _sceneTimes = null;

	#region Fields
	const byte SceneChangeTime = 5;
	int _currentScene = 0;
	int _lastScene = 0;
	/// <summary>
	/// access to the <see cref="PinGodGame"/> singleton
	/// </summary>
	protected PinGodGame pinGod;
	List<CanvasItem> Scenes = new List<CanvasItem>();
	private Timer timer;
	#endregion

	/// <summary>
	/// Sets up timer for cycling scenes in the AttractLayers tree. Stops ball searching
	/// </summary>
	public override void _EnterTree()
	{
		pinGod = (GetNode("/root/PinGodGame") as PinGodGame);
		timer = (GetNode("AttractLayerChangeTimer") as Timer);
		timer.WaitTime = _scene_change_secs;

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

		pinGod.SetBallSearchStop();
	}

	/// <summary>
	/// Just logs attract loaded
	/// </summary>
	public override void _Ready()
	{
		pinGod.LogInfo("Attract loaded");		
	}

	/// <summary>
	/// Checks for start button presses to Start a game. Players can cycle the scenes with flippers
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (pinGod.SwitchOn("start", @event))
        {
			CallDeferred(nameof(StartGame));
        }
        if (pinGod.SwitchOn("flipper_l", @event))
		{
			CallDeferred("ChangeLayer", true);
		}
		if (pinGod.SwitchOn("flipper_r", @event))
		{
			CallDeferred("ChangeLayer", false);
		}
	}

    private void StartGame()
    {
        var started = pinGod.StartGame();
        if (started)
        {
            OnGameStartedFromAttract();
        }
        pinGod.LogInfo("attract: starting game. started?", started);
    }

	/// <summary>
	/// What scene index are we on
	/// </summary>
	/// <returns></returns>
    public int GetCurrentSceneIndex() => _currentScene;

	/// <summary>
	/// stops the attract cycle timer
	/// </summary>
    public virtual void OnGameStartedFromAttract() 
	{
		pinGod.LogInfo("attract: game started");
		timer.Stop();
	}

    /// <summary>
    /// Switches the scenes visibility on a timer. Plays lamp seq in VP
    /// </summary>
    private void _on_Timer_timeout()
	{
		CallDeferred("ChangeLayer", false);
	}

    /// <summary>
    /// Changes the attract layer. Cycles the AttractLayers in the scene
    /// </summary>
    /// <param name="reverse">Cycling in reverse?</param>
    public virtual void ChangeLayer(bool reverse = false)
	{
		if (Scenes?.Count < 1) return;

		timer.Stop();

		//check if lower higher than our attract layers
		_currentScene = reverse ? _currentScene - 1 : _currentScene + 1;
		pinGod.LogDebug("change layer reverse: ", reverse, " scene", _currentScene);

		_currentScene = _currentScene > Scenes?.Count - 1 ? 0 : _currentScene;
		_currentScene = _currentScene < 0 ? Scenes?.Count - 1 ?? 0 : _currentScene;

		//hide the last layer and show new index
		Scenes[_lastScene].Hide(); //Scenes[_lastScene].Visible = false;
		Scenes[_currentScene].Show();// Scenes[_currentScene].Visible = true;

		_lastScene = _currentScene;

		float delay = _scene_change_secs;
		if (_sceneTimes?.Length > 0)
        {
			if(_currentScene <= _sceneTimes.Length)
            {
				delay = _sceneTimes[_currentScene];
            }
        }

		timer.Start(delay);
	}
}
