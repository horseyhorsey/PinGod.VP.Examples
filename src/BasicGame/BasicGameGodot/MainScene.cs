using Godot;
using System.Threading.Tasks;
using static Godot.GD;

public class MainScene : Node2D
{	
	[Export]
	private int _startButtonNum = 19;
	[Export]
	private int _CreditButtonNum = 2;

	private Control pauseLayer;
	private Node2D attractnode;	

	public override void _Ready()
	{
		//show a pause menu when pause enabled.
		pauseLayer = GetNode("CanvasLayer/PauseControl") as Control;
		pauseLayer.Hide();
		PauseMode = PauseModeEnum.Process;

		//attract mod already in the tree, get the instance so we can free it when game started
		attractnode = GetNode("Modes/Attract") as Node2D;		
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
	public void EndGame()
	{
		GetTree().ReloadCurrentScene();
	}

	public override void _Input(InputEvent @event)
	{
		//Pause / Unpause game.
		if (@event.IsActionPressed("pause"))
		{
			pauseLayer.Show();
			GetTree().Paused = true;
		}
		else if (@event.IsActionReleased("pause"))
		{
			pauseLayer.Hide();
			GetTree().Paused = false;
		}

		if (@event.IsActionPressed("sw"+_startButtonNum)) //Start button. See PinGod.vbs for Standard switches
		{
			if (GameGlobals.StartGame())
			{
				StartGame();
				//pulse ball from trough		
				GameGlobals.BallInPlay = 1;
				OscService.PulseCoilState(1);
			}
		}
		if (@event.IsActionPressed("sw"+_CreditButtonNum)) //Coin button. See PinGod.vbs for Standard switches
		{
			GameGlobals.Credits++;
		}

		//Check if the last trough switch was enabled
		if (@event.IsActionPressed("sw"+Trough.TroughSwitches[Trough.TroughSwitches.Length-1]))
		{
			//end the ball in play?
			if (Trough.IsTroughFull() && !Trough.BallSaveActive)
			{
				if (GameGlobals.EndOfBall())
				{
					GameGlobals.EndOfGame();
					EndGame();
				}
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
		GetNode("Modes").AddChild(packedScene.Instance());
	}
}
