using Godot;
using System.Threading.Tasks;
using static Godot.GD;

public class MainScene : Node2D
{	
	private Control pauseLayer;
	private Node2D attractnode;
	public  GameGlobals gameGlobal { get; private set; }

	public override void _Ready()
	{
		//show a pause menu when pause enabled.
		pauseLayer = GetNode("CanvasLayer/PauseControl") as Control;
		pauseLayer.Hide();
		PauseMode = PauseModeEnum.Process;

		//attract mod already in the tree, get the instance so we can free it when game started
		attractnode = GetNode("Modes/Attract") as Node2D;

		//save a reference to connect signals
		gameGlobal = GetNode("/root/GameGlobals") as GameGlobals;
		gameGlobal.Connect("GameStarted", this, "OnGameStarted");
		gameGlobal.Connect("GameEnded", this, "OnGameEnded");		
	}

	/// <summary>
	/// When everything is ready, remove the attract, start the game.
	/// </summary>
	public void StartGame()
	{
		//remove the attract mode
		attractnode.QueueFree();

		//load the game scene mode and add to Modes tree
		LoadSceneMode("res://modes/Game.tscn");		
	}

	/// <summary>
	/// End game, reloads the original scene, removing anything added. This could be used as a reset from VP with F3.
	/// </summary>
	public void OnGameEnded()
	{		
		GetNode("Modes/Game").QueueFree();
		GetTree().ReloadCurrentScene();		
	}

	void OnGameStarted()
	{
		StartGame();		
		GameGlobals.BallInPlay = 1;

		//pulse ball from trough
		gameGlobal.StartNewBall();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("pause"))
		{
			Print("pause");
			gameGlobal.SetGamePaused();
			pauseLayer.Show();
			GetTree().Paused = true;
		}
		else if (@event.IsActionReleased("pause"))
		{
			Print("resume");
			gameGlobal.SetGameResumed();
			pauseLayer.Hide();
			GetTree().Paused = false;
		}

		if (!GameGlobals.IsTilted)
		{
			//Start button. See PinGod.vbs for Standard switches
			if (@event.IsActionPressed("sw" + GameGlobals.StartSwitchNum)) 
			{
				gameGlobal.StartGame();
			}
		}
	}

	/// <summary>
	/// Runs a new task to load a scene and poll until finished. Display freezes in VP without this if scenes are med/large <para/>
	/// <see cref="_loaded(PackedScene)"/>, this will be added a child to the Modes node
	/// </summary>
	private void LoadSceneMode(string resourcePath)
	{
		Task.Run(() =>
		{
			var ril = ResourceLoader.LoadInteractive(resourcePath);
			PackedScene res; //the resource to return after finished loading
			Print("loading-" + resourcePath);
			//var total = ril.GetStageCount(); //total resources left to load, can be used progress bar
			while (true)
			{
				var err = ril.Poll();
				if (err == Error.FileEof)
				{
					res = ril.GetResource() as PackedScene;
					Print("resource loaded");
					break;
				}
			}

			CallDeferred("_loaded", res);
		});
	}

	void _loaded(PackedScene packedScene)
	{
		var instance = packedScene.Instance();
		GetNode("Modes").AddChild(instance);		
	}
}
