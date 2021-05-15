using Godot;
using System.Threading.Tasks;
using static Godot.GD;

public class MainScene : Node2D
{	
	[Export] private int _startButtonNum = 19;
	[Export] private int _CreditButtonNum = 2;	

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
		//pulse ball from trough
		GameGlobals.BallInPlay = 1;
		OscService.PulseCoilState(1);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("pause"))
		{			
			pauseLayer.Show();
			GetTree().Paused = true;
			gameGlobal.SetGamePaused();
		}
		else if (@event.IsActionReleased("pause"))
		{			
			pauseLayer.Hide();
			GetTree().Paused = false;
			gameGlobal.SetGameResumed();			
		}

		if (@event.IsActionPressed("sw"+_startButtonNum)) //Start button. See PinGod.vbs for Standard switches
		{
			gameGlobal.StartGame();
		}

		if (@event.IsActionPressed("sw"+_CreditButtonNum)) //Coin button. See PinGod.vbs for Standard switches
		{
			gameGlobal.AddCredits(1);			
		}

		//Check if the last trough switch was enabled
		if (@event.IsActionPressed("sw"+Trough.TroughSwitches[Trough.TroughSwitches.Length-1]))
		{
			//end the ball in play?
			if (Trough.IsTroughFull() && !Trough.BallSaveActive)
			{
				if(gameGlobal.EndOfBall())
					gameGlobal.EndOfGame();
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
