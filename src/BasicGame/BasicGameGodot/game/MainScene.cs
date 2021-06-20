using Godot;
using System;
using System.Threading.Tasks;
using static Godot.GD;

public class MainScene : Node2D
{
	const string SERVICE_MENU_SCENE = "res://modes/common/service_menu/ServiceMenu.tscn";
	const string GAME_SCENE = "res://modes/Game.tscn";

	private Control pauseLayer;
	private Control attractnode;    
	public  PinGodGame pinGod { get; private set; }
	public bool InServiceMenu { get; private set; }
	private ResourcePreloader _resourcePreLoader;

	public override void _EnterTree()
	{
		Print("Splash timer msecs", OS.GetSplashTickMsec());
		//try to catch anything unhandled here, not when ready
		AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
		PauseMode = PauseModeEnum.Process;

		//load the service menu scene and add to preloader
		_resourcePreLoader = GetNode("ResourcePreloader") as ResourcePreloader;
		var sMenu = Load(SERVICE_MENU_SCENE) as PackedScene;
		_resourcePreLoader.AddResource(SERVICE_MENU_SCENE.BaseName(), sMenu);

		//save a reference to connect signals
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		pinGod.Connect("GameStarted", this, "OnGameStarted");
		pinGod.Connect("GameEnded", this, "OnGameEnded");
		pinGod.Connect("ServiceMenuExit", this, "OnServiceMenuExit");

		//attract mod already in the tree, get the instance so we can free it when game started
		attractnode = GetNode("Modes/Attract") as Control;
		//show a pause menu when pause enabled.
		pauseLayer = GetNode("CanvasLayer/PauseControl") as Control;		
	}

	public override void _Ready()
	{
		pauseLayer.Hide();
		Print("main: ready");		
	}

	private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
	{
		GD.PrintS($"Unhandled exception {e.ExceptionObject}");
	}

	/// <summary>
	/// When everything is ready, remove the attract, start the game.
	/// </summary>
	public void StartGame()
	{
		//remove the attract mode
		attractnode.QueueFree();

		//load the game scene mode and add to Modes tree		
		LoadSceneMode(GAME_SCENE);
	}

	/// <summary>
	/// End game, reloads the original scene, removing anything added. This could be used as a reset from VP with F3.
	/// </summary>
	public async void OnGameEnded()
	{		
		//Resource is there but cannot be removed?
		//if (_resourcePreLoader.HasResource(GAME_SCENE.BaseName()))
		//{
		//	Print("found");
		//	_resourcePreLoader.RemoveResource(GAME_SCENE);
		//}
		await Task.Run(() =>
		{
			GetNode("Modes/Game").QueueFree();
			CallDeferred(nameof(Reload));
		});
	}

	void Reload() => GetTree().ReloadCurrentScene();

	void OnGameStarted()
	{
		StartGame();
	}

	void OnServiceMenuExit()
	{
		CallDeferred(nameof(Reload));
		InServiceMenu = false;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("pause"))
		{
			Print("pause");
			pinGod.SetGamePaused();
			pauseLayer.Show();
			GetTree().Paused = true;
		}
		else if (@event.IsActionReleased("pause"))
		{
			Print("resume");
			pinGod.SetGameResumed();
			pauseLayer.Hide();
			GetTree().Paused = false;
		}

		if (!pinGod.IsTilted)
		{            			
			if (pinGod.SwitchOn("enter", @event))
			{
				if (!InServiceMenu)
				{
					//enter service menu					
					InServiceMenu = true;

					Task.Run(() =>
					{
						if (pinGod.GameInPlay)
							GetNode("Modes/Game")?.QueueFree();
						else
							GetNode("Modes/Attract")?.Free();

						//load service menu into modes
						CallDeferred("_loaded", _resourcePreLoader.GetResource(SERVICE_MENU_SCENE.BaseName()));

						pinGod.EmitSignal("ServiceMenuEnter");
					});
				}				
			}
		}
	}

	Mutex m = new Mutex();

	/// <summary>
	/// Probably best to use <see cref="LoadSceneMode(string)"/> Runs a new task to load a scene and poll until finished. Display freezes in VP without this if scenes are med/large <para/>
	/// <see cref="_loaded(PackedScene)"/>, this will be added a child to the Modes node
	/// </summary>
	private void LoadSceneModeInteractive(string resourcePath)
	{
		Task.Run(() =>
		{
			m.Lock();
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
					break;
				}
			}
			
			CallDeferred("_loaded", res);

			m.Unlock();
		});
	}

	/// <summary>
	/// Runs a new task to load a scene and poll until finished. Display freezes in VP without this if scenes are med/large <para/>
	/// <see cref="_loaded(PackedScene)"/>, this will be added a child to the Modes node
	/// </summary>
	private async void LoadSceneMode(string resourcePath)
	{		
		await Task.Run(() =>
		{
			m.Lock();
			string name = resourcePath.BaseName();
			Resource res = null;
			if (!_resourcePreLoader.HasResource(name))
			{
				Print("loading mode scene resource for ", name);
				res = Load(resourcePath);
				_resourcePreLoader.AddResource(name, res);
			}
			else
			{
				Print("scene resource already exists for ", name);
				res = _resourcePreLoader.GetResource(name);
			}
			CallDeferred("_loaded", res);
			m.Unlock();
		});
	}

	void _loaded(PackedScene packedScene)
	{
		GetNode("Modes").AddChild(packedScene.Instance());
		Print("modes added: ", packedScene.ResourceName);
	}
}
