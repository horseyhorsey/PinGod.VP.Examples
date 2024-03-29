using Godot;
using System;
using System.Threading.Tasks;
using static Godot.GD;

/// <summary>
/// Main Scene. The Entry point
/// </summary>
public class MainScene : Node2D
{
    internal ResourcePreloader _resourcePreLoader;
    /// <summary>
    /// Path to the Game.tscn. 
    /// </summary>
    [Export] protected string _game_scene_path = "res://game/Game.tscn";
    /// <summary>
    /// Path to service menu scene
    /// </summary>
    [Export] protected string _service_menu_scene_path = "res://addons/PinGodGame/Scenes/ServiceMenu.tscn";
	private Node attractnode;
	Mutex m = new Mutex();
	private Control pauseLayer;
    private Control settingsDisplay;    
    /// <summary>
    /// Is machine is the Service Menu?
    /// </summary>
    public bool InServiceMenu { get; private set; }
    /// <summary>
    /// PinGodGame singleton
    /// </summary>
	public PinGodGame pinGod { get; private set; }

    /// <summary>
    /// Connects to <see cref="PinGodBase.GameStarted"/>, <see cref="PinGodBase.GameEnded"/>, <see cref="PinGodBase.ServiceMenuExit"/> <para/>
    /// Holds <see cref="attractnode"/>, <see cref="settingsDisplay"/>, <see cref="pauseLayer"/>
    /// </summary>
	public override void _EnterTree()
    {
        //save a reference to connect signals
        pinGod = GetNode<PinGodGame>("/root/PinGodGame");
        pinGod.LogDebug("Splash timer msecs", OS.GetSplashTickMsec());

        //try to catch anything unhandled here, not when ready
        AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
        PauseMode = PauseModeEnum.Process;

        //load preloader
        _resourcePreLoader = GetNode("ResourcePreloader") as ResourcePreloader;
        PreloadServiceMenu();
        pinGod.Connect(nameof(PinGodGame.GameStarted), this,nameof(OnGameStarted));
        pinGod.Connect(nameof(PinGodGame.GameEnded), this, nameof(OnGameEnded));
        pinGod.Connect(nameof(PinGodGame.ServiceMenuExit), this, nameof(OnServiceMenuExit));

        //attract mod already in the tree, get the instance so we can free it when game started
        attractnode = GetNode("Modes/Attract");
        //show a pause menu when pause enabled.
        pauseLayer = GetNode("CanvasLayer/PauseControl") as Control;

        settingsDisplay = GetNodeOrNull<Control>("CanvasLayer/SettingsDisplay");
    }

    /// <summary>
    /// Listens for actions on "pause", "quit", "settings" and will activate the Service menu on "enter" <para/>
    /// * Service menu will remove the Game or Attract from the scene depending on which mode currently in
    /// </summary>
    /// <param name="event"></param>
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("pause"))
        {
            if(!settingsDisplay.Visible)
                TogglePause();
            return;
        }

        if (@event.IsActionPressed("quit"))
        {
            pinGod.Quit();
            return;
        }

        if (@event.IsActionPressed("settings"))
        {
            
            if (settingsDisplay != null)
            {
                settingsDisplay.Visible = !settingsDisplay.Visible;
                if (settingsDisplay.Visible)
                {
                    if (!GetTree().Paused)
                        OnPauseGame();
                }
                else
                {
                    OnResumeGame();
                }
            }                
        }

        if (!pinGod.IsTilted)
        {
            if (pinGod.SwitchOn("enter", @event))
            {
                if (!InServiceMenu)
                {
                    if (!string.IsNullOrWhiteSpace(_service_menu_scene_path))
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
                            CallDeferred("_loaded", _resourcePreLoader.GetResource(_service_menu_scene_path.BaseName()));

                            pinGod.EmitSignal("ServiceMenuEnter");
                        });
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sets Solenoid enabled under "died"? <para/>
    /// pingod.vp controller coil 0, sets GameRunning on the controller
    /// </summary>
    public override void _Ready()
    {
        pauseLayer.Hide();
        pinGod.SolenoidOn("died", 1);
        pinGod.LogInfo("pingod base: ready, sent died coil on");
    }

    /// <summary>
    /// End game, reloads the original scene, removing anything added. This could be used as a reset from VP with F3.
    /// </summary>
    public async void OnGameEnded()
    {
        await Task.Run(() =>
        {
            GetNode("Modes/Game").QueueFree();
            CallDeferred(nameof(Reload));
        });
    }

    /// <summary>
    /// When everything is ready, remove the attract, start the game.
    /// </summary>
    public void StartGame()
    {
        //load the game scene mode and add to Modes tree		
        LoadSceneMode(_game_scene_path);
    }

    void _loaded(PackedScene packedScene)
    {
        GetNode("Modes").AddChild(packedScene.Instance());
        pinGod.LogInfo("modes added: ", packedScene.ResourceName);
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
            if (!string.IsNullOrWhiteSpace(name))
            {
                Resource res = null;
                if (!_resourcePreLoader.HasResource(name))
                {
                    pinGod.LogDebug("loading mode scene resource for ", name);
                    res = Load(resourcePath);
                    _resourcePreLoader.AddResource(name, res);
                }
                else
                {
                    pinGod.LogWarning("scene resource already exists for ", name);
                    res = _resourcePreLoader.GetResource(name);
                }
                CallDeferred("_loaded", res);
            }
            m.Unlock();

            //remove the attract mode
            attractnode.CallDeferred("queue_free");
        });
    }

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
            pinGod.LogDebug("loading-" + resourcePath);
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

    void OnGameStarted()
    {
        CallDeferred(nameof(StartGame));
    }

    private void OnPauseGame()
    {
        pinGod.LogDebug("pause");
        pinGod.SetGamePaused();
        pauseLayer.Show();
        GetTree().Paused = true;
    }

    private void OnResumeGame()
    {
        pinGod.LogDebug("resume");
        pinGod.SetGameResumed();
        pauseLayer.Hide();
        GetTree().Paused = false;
    }

    void OnServiceMenuExit()
    {
        CallDeferred(nameof(Reload));
        InServiceMenu = false;
    }

    /// <summary>
    /// add the service menu scene to preloader
    /// </summary>
    private void PreloadServiceMenu()
    {
        if (!string.IsNullOrWhiteSpace(_service_menu_scene_path))
        {
            var sMenu = Load(_service_menu_scene_path) as PackedScene;
            _resourcePreLoader.AddResource(_service_menu_scene_path.BaseName(), sMenu);
        }
    }

    void Reload() => GetTree().ReloadCurrentScene();
    private void TogglePause()
    {
        if (GetTree().Paused)
        {
            OnResumeGame();
        }
        else
        {
            OnPauseGame();
        }
    }
	private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
	{
		pinGod.LogError($"Unhandled exception {e.ExceptionObject}");
	}
}
