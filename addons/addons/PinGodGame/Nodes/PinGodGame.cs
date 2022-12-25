using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Create class inheriting this and use the PinGodGame.tscn. <para/>
/// This should be set to AutoLoad with the corresponding scene for games, see Project Settings <para/>
/// Has <see cref="Trough"/>
/// </summary>
public abstract class PinGodGame : PinGodBase
{
    #region Exports
    [Export] bool _lamp_overlay_enabled = false;
    [Export] bool _switch_overlay_enabled = false;
    [Export] bool _record_game = false;
    [Export] bool _playback_game = false;
    [Export] string _playbackfile = null;
    #endregion

    #region Public Properties - Standard Pinball / Players
    /// <summary>
    /// 
    /// </summary>
    public byte FlippersEnabled = 0;
    /// <summary>
    /// Is the game in bonus mode
    /// </summary>
	public bool InBonusMode = false;
    /// <summary>
    /// Any multi-ball running?
    /// </summary>
	public bool IsMultiballRunning = false;
    /// <summary>
    /// Maximum players on machine
    /// </summary>
	public byte MaxPlayers = 4;
	const string AUDIO_MANAGER = "res://addons/PinGodGame/Audio/AudioManager.tscn";
    private MainScene mainScene;
    /// <summary>
    /// 
    /// </summary>
    public AudioManager AudioManager { get; protected set; }
    /// <summary>
    /// Current number of the ball in play
    /// </summary>
    public byte BallInPlay { get; set; }
    /// <summary>
    /// Is ball save active
    /// </summary>
    public bool BallSaveActive { get; internal set; }

    /// <summary>
    /// Set options for game ball search
    /// </summary>
    public BallSearchOptions BallSearchOptions { get; set; }
    /// <summary>
    /// Timer node
    /// </summary>
    public Timer BallSearchTimer { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public byte BallsPerGame { get; set; }

    /// <summary>
    /// Flag used for ball saving on the initial launch. If ball enters the shooterlane after a ball save then this is to stop the ballsave starting again
    /// </summary>
    public bool BallStarted { get; internal set; }

    /// <summary>
    /// Number of current player
    /// </summary>
    public byte CurrentPlayerIndex { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public GameData GameData { get; internal set; }
    /// <summary>
    /// Game is in play?
    /// </summary>
    public bool GameInPlay { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public GameSettings GameSettings { get; internal set; }
    /// <summary>
    /// Is Ball Started?
    /// </summary>
    public bool IsBallStarted { get; internal set; }
    /// <summary>
    /// Is Game tilted?
    /// </summary>
    public bool IsTilted { get; set; }
    /// <summary>
    /// Logging levels
    /// </summary>
    public PinGodLogLevel LogLevel { get; set; } = PinGodLogLevel.Info;
    /// <summary>
    /// Base <see cref="PinGodPlayer"/>
    /// </summary>
    public PinGodPlayer Player { get; private set; }
    /// <summary>
    /// <see cref="PinGodPlayer"/> Players List for current Game
    /// </summary>
    public List<PinGodPlayer> Players { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public byte Tiltwarnings { get; set; }
    #endregion

    internal Trough _trough;
    /// <summary>
    /// Update lamps overlay. <see cref="LampMatrix"/>
    /// </summary>
    protected LampMatrix _lampMatrixOverlay = null;
    /// <summary>
    /// The memory map used to communicate states between software
    /// </summary>
    protected MemoryMap memMapping;

    private Queue<PlaybackEvent> _playbackQueue;
	/// <summary>
	/// recording actions to file using godot
	/// </summary>
	private File _recordFile;
	private RecordPlaybackOption _recordPlayback;
	private ulong gameEndTime;
	private ulong gameLoadTimeMsec;
	private ulong gameStartTime;
    private RandomNumberGenerator randomNumGenerator = new RandomNumberGenerator();

    /// <summary>
    /// Initializes and creates empty player list
    /// </summary>
    public PinGodGame()
	{
		Players = new List<PinGodPlayer>();        
	}

	#region Godot overrides

	/// <summary>
	/// Gets user cmd line args, loads data and settings, creates trough, sets up ball search and audio manager
	/// </summary>
	public override void _EnterTree()
	{
        if (!Engine.EditorHint)
        {
            LogInfo("pingod:enter tree");
            CmdArgs = GetCommandLineArgs();

            LoadSettingsFile();

            SetupWindow();

            LoadPatches();

            LoadDataFile();

            //get trough from tree
            _trough = GetNodeOrNull<Trough>("Trough");
            if (_trough == null)
                LogWarning("trough not found");

            //create and add ball search timer
            BallSearchTimer = new Timer() { Autostart = false, OneShot = false };
            BallSearchTimer.Connect("timeout", this, "OnBallSearchTimeout");
            this.AddChild(BallSearchTimer);

            Setup();
            SetupAudio();

            LogInfo("pingod:enter tree setup complete");
            gameLoadTimeMsec = OS.GetTicksMsec();
        }
    }

    private void SetupWindow()
    {
        if(GameSettings?.Display != null)
        {
            OS.WindowPosition = new Vector2(GameSettings.Display.X, GameSettings.Display.Y);
        }        

        var ontop = (bool)ProjectSettings.GetSetting("display/window/size/always_on_top");
        OS.SetWindowAlwaysOnTop(false);
        if (ontop)
        {            
            OS.SetWindowAlwaysOnTop(true);
        }

        if (GameSettings?.Display != null)
        {
            if (GameSettings.Display.FullScreen)
                OS.WindowFullscreen = true;
        }        
    }

    /// <summary>
    /// Saves recordings if enabled and runs <see cref="Quit(bool)"/>
    /// </summary>
    public override void _ExitTree()
    {
        if (_recordPlayback == RecordPlaybackOption.Record)
        {
            LogInfo("pingodbase: exit tree, saving recordings");
            SaveRecording();
        }
        else LogInfo("pingodbase: exit tree");

        Quit(true);
    }

    /// <summary>
    /// Listens for any escape action preses. Handles coin switches, adds credits. Toggle border F2 default
    /// </summary>
    /// <param name="event"></param>
    public override void _Input(InputEvent @event)
    {
        //quits the game. ESC
        if (@event.IsActionPressed("quit"))
        {
            LogInfo("quit request");
            SetGameResumed();
            GetTree().Quit(0);
            return;
        }

        if (@event.IsActionPressed("toggle_border"))
        {
            ToggleWindowBorder();
        }

		//Coin button. See PinGod.vbs for Standard switches
		if (SwitchOn("coin1", @event) || SwitchOn("coin2", @event) || SwitchOn("coin3", @event))
        {
            AudioManager.PlaySfx("credit");
            AddCredits(1);
        }
    }

    /// <summary>
    /// Processes playback events...Processing is disabled if it isn't enabled and playback is finished
    /// </summary>
    /// <param name="delta"></param>
    public override void _Process(float delta)
    {
        if (_recordPlayback != RecordPlaybackOption.Playback)
        {
            SetProcess(false);
            LogInfo("pingodbase: _Process loop ended, recordings aren't being played back");
			return;
        }
        else
        {
            if (_playbackQueue?.Count <= 0)
            {
                LogInfo("pingodbase: playback events ended");
                _recordPlayback = RecordPlaybackOption.Off;
                return;
            }

            var time = OS.GetTicksMsec() - gameLoadTimeMsec;
            var nextEvt = _playbackQueue.Peek().Time;
            if (nextEvt <= time)
            {
                var pEvent = _playbackQueue.Dequeue();
                var ev = new InputEventAction() { Action = pEvent.EvtName, Pressed = pEvent.State };
                Input.ParseInputEvent(ev);
                LogDebug("pingodbase: playback evt ", pEvent.EvtName);
            }
        }
    }

    /// <summary>
    /// Game initialized. Memory map is created here if read and write is enabled. <para/>  Gets <see cref="BallSearchOptions"/>, sets up a <see cref="_lampMatrixOverlay"/> <para/>
    /// Gets hold of the <see cref="MainScene"/> to control window size, stretch
    /// </summary>
    public override void _Ready()
    {
        base._Ready();

        LogDebug($"{nameof(PinGodGame)}: _Ready|getting {nameof(BallSearchOptions)} from MachineConfig node");
        BallSearchOptions = GetNode<MachineConfig>(nameof(MachineConfig))?.BallSearchOptions;

        LogInfo($"{nameof(PinGodGame)}: _Ready|getting MainScene");
        mainScene = GetNodeOrNull<MainScene>("/root/" + nameof(MainScene));
        
        if (_lampMatrixOverlay != null)
        {
            LogDebug($"{nameof(PinGodGame)}: _Ready|setting labels for Lamp matrix overlay lamps");
            foreach (var item in Machine.Lamps)
            {
                _lampMatrixOverlay.SetLabel(item.Value.Num, item.Key);
            }
        }

        //setup and run writing memory states for other application to access        
        if (GameSettings.MachineStatesWrite || GameSettings.MachineStatesRead)
        {            
            LogInfo($"{nameof(PinGodGame)}: writing machine states is enabled. delay: " + GameSettings.MachineStatesWriteDelay);
            LogDebug($"{nameof(PinGodGame)}: _Ready|setting up memory mapping");
            var mConfig = GetNode<MachineConfig>(nameof(MachineConfig));
            memMapping = new MemoryMap(mConfig._memCoilCount, mConfig._memLampCount, mConfig._memLedCount, mConfig._memSwitchCount, pinGodGame: this);
            memMapping.Start(GameSettings.MachineStatesWriteDelay);
        }        
    }

    #endregion


    /// <summary>
    /// Commands args incoming from <see cref="GetCommandLineArgs"/>
    /// </summary>
    public Dictionary<string, string> CmdArgs { get; private set; }

    /// <summary>
    /// Total game time
    /// </summary>
    public virtual ulong GetElapsedGameTime => gameEndTime - gameStartTime;    

    /// <summary>
    /// Adds bonus to the current player
    /// </summary>
    /// <param name="points"></param>
    public virtual void AddBonus(long points)
	{
		if (Player != null)
		{
			Player.Bonus += points;
		}
	}

	/// <summary>
	/// Adds credits to the GameData and emits <see cref="PinGodGame.CreditAdded"/> signal
	/// </summary>
	/// <param name="amt"></param>
	public virtual void AddCredits(byte amt)
	{
		GameData.Credits += amt;
		EmitSignal(nameof(PinGodGame.CreditAdded));
	}

	/// <summary>
	/// Adds points to the current player
	/// </summary>
	/// <param name="points"></param>
	/// <param name="emitUpdateSignal">Sends a <see cref="ScoresUpdated"/> signal if set. See <see cref="ScoreMode"/> for use</param>
	public virtual long AddPoints(long points, bool emitUpdateSignal = true)
	{
		if (Player != null)
		{
			Player.Points += points;
			if (emitUpdateSignal)
				EmitSignal(nameof(ScoresUpdated));
		}

		return points;
	}

    /// <summary>
    /// Gets balls in play from the <see cref="_trough"/>
    /// </summary>
    /// <returns></returns>
	public virtual int BallsInPlay() => _trough?.BallsInPlay() ?? 0;

    /// <summary>
    /// Starts the <see cref="Trough.StartBallSaver(float)"/> ball saver
    /// </summary>
    /// <param name="secs"></param>
    internal void BallSaveEnabled(float secs) => _trough?.StartBallSaver(secs);

    /// <summary>
    /// Creates a new <see cref="PinGodPlayer"/>. Override this for your own custom players
    /// </summary>
    /// <param name="name"></param>
    public virtual void CreatePlayer(string name) => Players.Add(new PinGodPlayer() { Name = name, Points = 0 });

    /// <summary>
    /// Disables all <see cref="Lamps"/>
    /// </summary>
    public virtual void DisableAllLamps()
	{
		if (Machine.Lamps?.Count > 0)
		{
			foreach (var lamp in Machine.Lamps)
			{
				lamp.Value.State = 0;
			}
		}
	}

	/// <summary>
	/// Disables all <see cref="Leds"/>
	/// </summary>
	public virtual void DisableAllLeds()
    {
		if (Machine.Leds?.Count > 0)
		{
			foreach (var led in Machine.Leds)
			{
				led.Value.State = 0;
			}
		}
	}

    /// <summary>
    /// Sets flippers solenoid on, saves state
    /// </summary>
    /// <param name="enabled"></param>
	public virtual void EnableFlippers(byte enabled)
	{
		FlippersEnabled = enabled;
		this.SolenoidOn("flippers", enabled);
	}

	/// <summary>
	/// Ends the current ball. Changes the player <para/>
	/// Emits <see cref="BallEnded"/> signal
	/// </summary>
	/// <returns>True if all balls finished, game is finished</returns>
	public virtual bool EndBall()
	{
		if (!GameInPlay) return false;

		IsBallStarted = false;
		BallStarted = false;
		EnableFlippers(0);

		if (Players.Count > 0)
		{
			LogInfo("end of ball. current ball:" + BallInPlay);
			if (Player.ExtraBalls > 0)
			{
				this.EmitSignal(nameof(BallEnded), false);
			}
			else
			{
				if (Players.Count > 1)
				{
					CurrentPlayerIndex++;
					if (CurrentPlayerIndex + 1 > Players.Count)
					{
						CurrentPlayerIndex = 0;
						BallInPlay++;
					}
				}
				else
				{
					BallInPlay++;
				}

				LogInfo("ball in play " + BallInPlay);
				GameData.BallsPlayed++;
				if (BallInPlay > BallsPerGame)
				{
					//signal that ball has ended
					this.EmitSignal(nameof(BallEnded), true);
					return true;
				}
				else
				{
					//signal that ball has ended
					this.EmitSignal(nameof(BallEnded), false);
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Game has ended, sets <see cref="GameInPlay"/> to false and Sends <see cref="GameEnded"/>
	/// </summary>
	public virtual void EndOfGame()
	{
		GameInPlay = false;
		GameData.GamesFinished++;
		ResetTilt();
		gameEndTime = OS.GetTicksMsec();
		GameData.TimePlayed = gameEndTime - gameStartTime;
        if(_trough != null)
		    _trough.BallsLocked = 0;
		this.EmitSignal(nameof(GameEnded));
	}

    /// <summary>
    /// Time in milliseconds
    /// </summary>
    /// <param name="sw"></param>
    /// <returns></returns>
    public virtual ulong GetLastSwitchChangedTime(string sw) => Machine.Switches[sw].TimeSinceChange();

    /// <summary>
    /// Gets the Resources autoloaded scene at "/root/Resources"
    /// </summary>
    /// <returns></returns>
    public virtual Resources GetResources() => GetNodeOrNull<Resources>("/root/Resources");

    /// <summary>
    /// Gets the highest score made from the <see cref="GameData.HighScores"/>
    /// </summary>
    public virtual long GetTopScorePoints => GameData?.HighScores?
            .OrderByDescending(x => x.Scores).FirstOrDefault().Scores ?? 0;

    /// <summary>
    /// Detect if the input `isAction` found in the given switchNames. Uses <see cref="SwitchOn(string, InputEvent)"/>
    /// </summary>
    /// <param name="switchNames"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public virtual bool IsSwitch(string[] switchNames, InputEvent input)
	{
		for (int i = 0; i < switchNames.Length; i++)
		{
			if(SwitchOn(switchNames[i], input))
			{
				return true;
			}
		}

		return false;
	}

    /// <summary>
    /// <see cref="GameData.Load"/>
    /// </summary>
    public virtual void LoadDataFile() => GameData = GameData.Load();

	/// <summary>
	/// Exported games load patches from res://patch/patch_{patchNum}.pck . From 1. patch_1.pck, patch_2.pck
	/// </summary>
	public virtual void LoadPatches()
	{
        try
        {
            int patchNum = 1;
            bool success;
            while (success = ProjectSettings.LoadResourcePack($"res://patch/patch_{patchNum}.pck"))
            {
                LogInfo($"patch {patchNum} loaded");
                patchNum++;
            }
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
        }
	}

    /// <summary>
    /// Does nothing, obsolete
    /// </summary>
    [Obsolete("override the other methods for loading data, settings and setting up window")]
    public virtual void LoadSettingsAndData()
    {
		LogError("don't use, already init in ready");
    }

	/// <summary>
	/// Override when using your own GameSettings
	/// </summary>
	public virtual void LoadSettingsFile() => GameSettings = GameSettings.Load();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="what"></param>
    public virtual void LogDebug(params object[] what) => Logger.LogDebug(what);
    /// <summary>
    /// Uses static <see cref="Logger.LogError(string, object[])"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="what"></param>
    public virtual void LogError(string message = null, params object[] what) => Logger.LogError(message, what);
    /// <summary>
    /// Uses static <see cref="Logger.LogInfo(object[])"/>
    /// </summary>
    /// <param name="what"></param>
	public virtual void LogInfo(params object[] what) => Logger.LogInfo(what);
    /// <summary>
    /// Uses static <see cref="Logger.LogWarning(string, object[])"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="what"></param>
	public virtual void LogWarning(string message = null, params object[] what) => Logger.LogWarning(message, what);

	/// <summary>
	/// Invokes OnBallDrained on all groups marked as Mode within the scene tree.
	/// </summary>
	/// <param name="sceneTree"></param>
	/// <param name="group"></param>
	/// <param name="method"></param>
	public virtual void OnBallDrained(SceneTree sceneTree, string group = "Mode", string method = "OnBallDrained") => sceneTree.CallGroup(group, method);

	/// <summary>
	/// Invokes OnBallSaved on all groups marked as Mode within the scene tree.
	/// </summary>
	/// <param name="sceneTree"></param>
	/// <param name="group"></param>
	/// <param name="method"></param>
	public virtual void OnBallSaved(SceneTree sceneTree, string group = "Mode", string method = "OnBallSaved") => sceneTree.CallGroup(group, method);

	/// <summary>
	/// Pulse coils in the SearchCoils when ball search times out
	/// </summary>
	public virtual void OnBallSearchTimeout()
	{
		if (BallSearchOptions.IsSearchEnabled)
		{
			if (BallSearchOptions?.SearchCoils?.Length > 0)
			{
				LogDebug("pingodbase: pulsing search coils");
				for (int i = 0; i < BallSearchOptions?.SearchCoils.Length; i++)
				{
					SolenoidPulse(BallSearchOptions?.SearchCoils[i]);
				}

				BallSearchTimer.Stop();
			}
			else
			{
				BallSearchTimer.Stop();
			}
		}
		else
		{
			BallSearchTimer.Stop();
		}
	}

    /// <summary>
    /// Invokes OnBallStarted on all groups marked as Mode within the scene tree.
    /// </summary>
    /// <param name="sceneTree"></param>
    /// <param name="group"></param>
    /// <param name="method"></param>
    public virtual void OnBallStarted(SceneTree sceneTree, string group = "Mode", string method = "OnBallStarted") => sceneTree.CallGroup(group, method);

	/// <summary>
	/// Uses the <see cref="AudioManager.PlayMusic(string, float)"/>
	/// </summary>
	/// <param name="name"></param>
	/// <param name="pos"></param>
	public virtual void PlayMusic(string name, float pos = 0)
	{
		AudioManager.PlayMusic(name, pos);
	}

    /// <summary>
    /// Uses the <see cref="AudioManager.PlaySfx(string, string)"/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="bus">defaults to bus named Sfx</param>
    public virtual void PlaySfx(string name, string bus = "Sfx")
	{
		AudioManager.PlaySfx(name, bus);
	}

    /// <summary>
    ///  <see cref="AudioManager.PlayVoice(string, string)"/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="bus">Plays on bus name</param>
    public virtual void PlayVoice(string name, string bus = "Voice")
	{
		AudioManager.PlayVoice(name, bus);
	}

	/// <summary>
	/// Quits the game, saves data / settings and cleans up
	/// </summary>
	/// <param name="saveData">save game on exit?</param>
	public virtual void Quit(bool saveData = true)
	{
		if (saveData)
        {            
            SaveWindow();

            //save override.cfg when not running editor
            if (!OS.HasFeature("editor"))
            {
                ProjectSettings.SaveCustom(Resources.WorkingDirectory + "/override.cfg");
            }            

            SaveGameData();
            SaveGameSettings();
        }

        LogInfo("saved game");

        if (GetTree().Paused)
        {
			GetTree().Paused = false;
		}

		//send game ended, died
		SolenoidOn("died", 0);
        LogInfo("sent game ended coil");

        memMapping?.Dispose(); //dispose invokes stop as well		
	}

    /// <summary>
    /// Use random number generator from range
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public int RandomNumber(int from, int to) => randomNumGenerator.RandiRange(from, to);

	/// <summary>
	/// Reset player warnings and tilt
	/// </summary>
	public virtual void ResetTilt()
	{
		Tiltwarnings = 0;
		IsTilted = false;
	}	

    /// <summary>
    /// Invokes <see cref="GameData.Save(GameData)"/>
    /// </summary>
	public virtual void SaveGameData() => GameData.Save(GameData);

    /// <summary>
    /// Invokes <see cref="GameSettings.Save(GameSettings)"/>
    /// </summary>
	public virtual void SaveGameSettings() => GameSettings.Save(GameSettings);

    /// <summary>
    /// Saves a recording file if <see cref="RecordPlaybackOption.Record"/> is set
    /// </summary>
	public virtual void SaveRecording()
	{
		if (_recordPlayback == RecordPlaybackOption.Record)
		{
			_recordFile?.Close();
			LogInfo("pingodbase: recording file closed");
		}
	}

    /// <summary>
    /// Saves window position to game display settings as position isn't in PS? Use test_width and test_height for custom to keep the original project settings
    /// </summary>
    public virtual void SaveWindow()
    {
        GameSettings.Display.X = OS.WindowPosition.x;
        GameSettings.Display.Y = OS.WindowPosition.y;
        ProjectSettings.SetSetting("display/window/size/test_width", OS.WindowSize.x);
        ProjectSettings.SetSetting("display/window/size/test_height", OS.WindowSize.y);
    }

    /// <summary>
    /// Resets the time for ball searching from <see cref="BallSearchOptions.SearchWaitTime"/>
    /// </summary>
    public virtual void SetBallSearchReset()
	{
		BallSearchTimer.Start(BallSearchOptions?.SearchWaitTime ?? 10);
		LogDebug("ball search timer reset.");
	}
    /// <summary>
    /// Stops the <see cref="BallSearchTimer"/>
    /// </summary>
	public virtual void SetBallSearchStop()
	{
		BallSearchTimer.Stop();
		LogDebug("ball search stopped");
	}

	/// <summary>
	/// Sends a signal game is paused <see cref="GamePaused"/>
	/// </summary>
	public virtual void SetGamePaused() => EmitSignal(nameof(GamePaused));

	/// <summary>
	/// Sends a signal game is resumed <see cref="GameResumed"/>
	/// </summary>
	public virtual void SetGameResumed() => EmitSignal(nameof(GameResumed));

    /// <summary>
    /// Sets lamp state and state inside the <see cref="_lampMatrixOverlay"/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="state"></param>
	public virtual void SetLampState(string name, byte state)
	{
		if (!LampExists(name)) return;
		var lamp = Machine.Lamps[name];
		if (_lampMatrixOverlay != null) { _lampMatrixOverlay.SetState(lamp.Num, state); }
		lamp.State = state;
	}

    /// <summary>
    /// Sets state of led
    /// </summary>
    /// <param name="name"></param>
    /// <param name="state"></param>
    /// <param name="color"></param>
	public virtual void SetLedState(string name, byte state, int color = 0)
	{
		if (!LedExists(name)) return;
		var led = Machine.Leds[name];
		led.State = state;
		led.Color = color > 0 ? color : led.Color;
		if (_lampMatrixOverlay != null) { _lampMatrixOverlay.SetState(led.Num, state); }
	}

	/// <summary>
	/// Sets led states from System.Drawing Color
	/// </summary>
	/// <param name="name"></param>
	/// <param name="state"></param>
	/// <param name="colour"></param>
	public virtual void SetLedState(string name, byte state, System.Drawing.Color? colour = null)
	{
		if (!LedExists(name)) return;
		var c = colour.HasValue ?
			System.Drawing.ColorTranslator.ToOle(colour.Value) : Machine.Leds[name].Color;
		SetLedState(name, state, c);
	}

	/// <summary>
	/// Sets led state from godot color
	/// </summary>
	/// <param name="name"></param>
	/// <param name="state"></param>
	/// <param name="colour"></param>
	public virtual void SetLedState(string name, byte state, Color? colour = null)
	{
		if (!LedExists(name)) return;
		var c = colour.HasValue ?
			System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(colour.Value.r8, colour.Value.g8, colour.Value.b8))
			: Machine.Leds[name].Color;
		SetLedState(name, state, c);
	}

	/// <summary>
	/// Sets led from RGB
	/// </summary>
	/// <param name="name"></param>
	/// <param name="state"></param>
	/// <param name="r"></param>
	/// <param name="g"></param>
	/// <param name="b"></param>
	public virtual void SetLedState(string name, byte state, byte r, byte g, byte b)
	{
		if (!LedExists(name)) return;
		var c = System.Drawing.Color.FromArgb(r, g, b);
		var ole = System.Drawing.ColorTranslator.ToOle(c);
		SetLedState(name, state, ole);
	}

    /// <summary>
    /// Sets the Width and Height of the window from `display/window/size` settings. Sets SetScreenStretch from aspect ratio settings in `display/window/stretch/aspect`
    /// </summary>
    public void SetMainSceneAspectRatio()
    {
        var minW = (int)ProjectSettings.GetSetting("display/window/size/width");
        var minH = (int)ProjectSettings.GetSetting("display/window/size/height");

        var asp = Enum.Parse(typeof(PinGodStretchAspect),ProjectSettings.GetSetting("display/window/stretch/aspect").ToString());

        GetNodeOrNull<MainScene>("/root/MainScene")?
            .GetTree().SetScreenStretch(SceneTree.StretchMode.Mode2d, (SceneTree.StretchAspect)(int)asp, 
                new Vector2(minW, minH));
    }

    /// <summary>
    /// Sets up machine items from the collections, starts memory mapping and recordings
    /// </summary>
    public virtual void Setup()
    {
        Logger.LogLevel = GameSettings?.LogLevel ?? PinGodLogLevel.Warning;

        LogDebug("pingod: entering tree. Setup");

        Connect(nameof(ServiceMenuEnter), this, "OnServiceMenuEnter");

        //set up recording / playback
        SetUpRecordingsOrPlayback(_playback_game, _record_game, _playbackfile);

        //remove the overlays if disabled
        SetupDevOverlays();
    }

    /// <summary>
    /// Sets up the DevOverlays Tree. Enables / Disable helper overlays for lamps and switches. SwitchOverlay and LampMatrix
    /// </summary>
    public virtual void SetupDevOverlays()
    {         
        var devOverlays = GetNode("DevOverlays");
        if (devOverlays != null)
        {
            LogDebug("pingod: setting up overlays");
            if (!_lamp_overlay_enabled)
            {
                LogInfo("pingod: removing lamp overlay");
                devOverlays.GetNode("LampMatrix").QueueFree();
            }
            else
            {
                _lampMatrixOverlay = devOverlays.GetNode("LampMatrix") as LampMatrix;
            }

            if (!_switch_overlay_enabled)
            {
                LogInfo("pingod: removing switch overlay");
                devOverlays.GetNode("SwitchOverlay").QueueFree();
            }
        }
        else { LogDebug("pingod: overlays disabled"); }
    }

    /// <summary>
    /// Sets up recording or playback from a .recording file
    /// </summary>
    /// <param name="playbackEnabled"></param>
    /// <param name="recordingEnabled"></param>
    /// <param name="playbackfile"></param>
    public virtual void SetUpRecordingsOrPlayback(bool playbackEnabled, bool recordingEnabled, string playbackfile)
    {
        _recordPlayback = RecordPlaybackOption.Off;
        if (playbackEnabled) _recordPlayback = RecordPlaybackOption.Playback;
        else if (recordingEnabled) _recordPlayback = RecordPlaybackOption.Record;

        LogDebug("pingod: setup playback?: ", _recordPlayback.ToString());
        if (_recordPlayback == RecordPlaybackOption.Playback)
        {
            if (string.IsNullOrWhiteSpace(playbackfile))
            {
                LogWarning("set a playback file from user directory eg: user://recordings/26232613.record");
                _recordPlayback = RecordPlaybackOption.Off;
            }
            else
            {
                LogInfo("running playback file: ", playbackfile);
                try
                {
                    var pBackFile = new File();
                    if (pBackFile.Open(playbackfile, File.ModeFlags.Read) == Error.FileNotFound)
                    {
                        _recordPlayback = RecordPlaybackOption.Off;
                        LogError("playback file not found, set playback false");
                    }
                    else
                    {
                        string[] eventLine = null;
                        _playbackQueue = new Queue<PlaybackEvent>();
                        while ((eventLine = pBackFile.GetCsvLine("|"))?.Length == 3)
                        {
                            bool.TryParse(eventLine[1], out var state);
                            uint.TryParse(eventLine[2], out var time);
                            _playbackQueue.Enqueue(new PlaybackEvent(eventLine[0], state, time));
                        }
                        _playbackQueue.Reverse();
                        LogInfo(_playbackQueue.Count, " playback events queued. first action: ", _playbackQueue.Peek().EvtName);
                    }
                }
                catch (Exception ex)
                {
                    LogError($"playback file failed: " + ex.Message);
                }
            }
        }
        else if (_recordPlayback == RecordPlaybackOption.Record)
        {
            var userDir = CreateRecordingsDirectory();
            _recordFile = new File();
            _recordFile.Open(playbackfile, File.ModeFlags.WriteRead);
            LogDebug("pingod: game recording on");
        }
    }   

    /// <summary>
    /// Sets a solenoid state
    /// </summary>
    /// <param name="name"></param>
    /// <param name="state"></param>
    public virtual void SolenoidOn(string name, byte state)
    {
        if (!SolenoidExists(name)) return;
        Machine.Coils[name].State = state;
    }

    /// <summary>
    /// Pulses a <see cref="Machine.Coils"/> by name. Creates a new task?
    /// </summary>
    /// <param name="name"></param>
    /// <param name="pulse"></param>
    public virtual async void SolenoidPulse(string name, byte pulse = 255)
    {
        if (!SolenoidExists(name)) return;

        var coil = Machine.Coils[name];
        await Task.Run(async () =>
        {
            coil.State = 1;
            await Task.Delay(pulse);
            coil.State = 0;
        });
    }

    /// <summary>
    /// Creates a timer and removes it rather than new tasks. Need more testing if working 100%, todo: send multiple times see what it does.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="pulse"></param>
    public virtual void SolenoidPulseTimer(string name, float pulse = 0.255f)
    {
        if (!SolenoidExists(name)) return;

        var coil = Machine.Coils[name];

        var timer = new Timer() { Autostart = false, OneShot = true, Name = $"pulsetimer_{name}", WaitTime = pulse };
        timer.Connect("timeout", this, nameof(OnSolenoidPulseTimeout), new Godot.Collections.Array(new object[] { name }), (uint)ConnectFlags.Oneshot);
        coil.State = 1;
        AddChild(timer);
        timer.Start();
    }

    /// <summary>
    /// Queue frees a `pulsetimer_name` solenoid Timer from using <see cref="Machine.Coils"/>
    /// </summary>
    /// <param name="name"></param>
    private void OnSolenoidPulseTimeout(string name)
    {        
        var pinObj = Machine.Coils[name];
        if (pinObj != null)
        {
            pinObj.State = 0;
            var timer = GetNodeOrNull<Timer>($"pulsetimer_{name}");
            //LogInfo(timer.GetSignalConnectionList("timeout"));
            //timer.Disconnect("timeout", this, nameof(OnSolenoidPulseTimeout));
            timer?.QueueFree();
        }        
    }

    /// <summary>
    /// Attempts to start a game. If games in play then add more players until <see cref="MaxPlayers"/> <para/>
    /// </summary>
    /// <returns>True if the game was started</returns>
    public virtual bool StartGame()
    {
        LogInfo($"PinGodGame:start game. BIP:{BallInPlay}, players/max:{Players.Count}/{MaxPlayers}, credits: {GameData.Credits}, inPlay:{GameInPlay}");
        if (IsTilted)
        {
            LogInfo("PinGodGame: Cannot start game when game is tilted");
            return false;
        }

        // first player start game
        if (!GameInPlay && GameData.Credits > 0) 
        {
            LogInfo("PinGodGame: starting game, checking trough...");
            if (!_trough?.IsTroughFull() ?? false) //return if trough isn't full. TODO: needs debug option to remove check
            {
                LogInfo("PinGodGame: Trough not ready. Can't start game with empty trough.");
                BallSearchTimer.Start(1);
                return false;
            }

            Players.Clear(); //clear any players from previous game
            GameInPlay = true;

            //remove a credit and add a new player
            GameData.Credits--;
            BallsPerGame = (byte)(GameSettings.BallsPerGame > 5 ? 5 : GameSettings.BallsPerGame);

            if(_trough != null)
                _trough._ball_save_seconds = (byte)(GameSettings.BallSaveTime > 20 ? 20 : GameSettings.BallSaveTime);

            CreatePlayer($"P{Players.Count + 1}");
            CurrentPlayerIndex = 0;
            Player = Players[CurrentPlayerIndex];
            LogDebug("signal: player 1 added");
            GameData.GamesStarted++;
            gameStartTime = OS.GetTicksMsec();
            EmitSignal(nameof(PlayerAdded));
            EmitSignal(nameof(GameStarted));
            return true;
        }
        //game started already, add more players until max
        else if (BallInPlay <= 1 && GameInPlay && Players.Count < MaxPlayers && GameData.Credits > 0)
        {
            GameData.Credits--;
            CreatePlayer($"P{Players.Count + 1}");
            LogInfo($"signal: player added. {Players.Count}");
            EmitSignal(nameof(PlayerAdded));
            return true;
        }

        LogInfo("PinGodGame: start game, nothing happened.");
        return false;
    }

    /// <summary>
    /// Sets MultiBall running in the trough and Emits <see cref="MultiballStarted"/>
    /// </summary>
    /// <param name="numOfBalls">Number of balls to save. A 2 ball multiball would be 2</param>
    /// <param name="ballSaveTime"></param>
    /// <param name="pulseTime">Delay for the trough in multi-ball</param>
    public virtual void StartMultiBall(byte numOfBalls, byte ballSaveTime = 20, float pulseTime = 0)
    {
        if(_trough != null)
        {
            _trough.StartMultiball(numOfBalls, ballSaveTime, pulseTime);
        }

        IsMultiballRunning = true;
        EmitSignal(nameof(MultiballStarted));
    }

    /// <summary>
    /// Starts a new ball, changing to next player, enabling flippers and ejecting trough and sending <see cref="BallStarted"/>
    /// </summary>
    public virtual void StartNewBall()
    {
        IsBallStarted = true;
        LogInfo("base:starting new ball");
        GameData.BallsStarted++;
        ResetTilt();
        Player = Players[CurrentPlayerIndex];
        if (Player.ExtraBalls > 0 && Player.ExtraBallsAwarded < GameSettings.MaxExtraBalls)
        {
            Player.ExtraBalls--;
            Player.ExtraBallsAwarded++;
            LogInfo("base: player shoot again");
        }

        if(_trough != null)
            _trough?.PulseTrough();
        else
            SolenoidPulse("trough");

        EnableFlippers(1);
    }

    /// <summary>
    /// Stop music <see cref="AudioManager.StopMusic"/>
    /// </summary>
    /// <returns></returns>
    public virtual float StopMusic() => AudioManager.StopMusic();

    /// <summary>
    /// Checks a switches input event by friendly name that is in the <see cref="Switches"/> <para/>
    /// "coin", @event
    /// </summary>
    /// <param name="swName"></param>
    /// <param name="inputEvent"></param>
    /// <returns></returns>
    public virtual bool SwitchOff(string swName, InputEvent inputEvent)
    {
        if (!SwitchExists(swName)) return false;
        var sw = Machine.Switches[swName];
        var result = sw.IsOff(inputEvent);
        if (result)
        {
            LogDebug("swOff:" + swName);

            if (_recordPlayback == RecordPlaybackOption.Record)
            {
                var recordLine = $"sw{sw.Num}|{false}|{OS.GetTicksMsec() - gameLoadTimeMsec}";
                _recordFile?.StoreLine(recordLine);
                LogDebug(recordLine);
            }

            if (BallSearchOptions.IsSearchEnabled && GameInPlay)
            {
                if (sw.BallSearch != BallSearchSignalOption.None)
                {
                    switch (sw.BallSearch)
                    {
                        case BallSearchSignalOption.Reset:
                            SetBallSearchReset();
                            break;
                        case BallSearchSignalOption.Off:
                            SetBallSearchStop();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Use in Godot _Input events. Checks a switches input event by friendly name from switch collection <para/>
    /// "coin", @event
    /// </summary>
    /// <param name="swName"></param>
    /// <param name="inputEvent"></param>
    /// <returns></returns>
    public virtual bool SwitchOn(string swName, InputEvent inputEvent)
    {
        if (!SwitchExists(swName)) return false;
        var sw = Machine.Switches[swName];
        var result = sw.IsOn(inputEvent);

        //do something with ball search if switch needs to
        if (result && BallSearchOptions.IsSearchEnabled && GameInPlay)
        {
            if (sw.BallSearch != BallSearchSignalOption.None)
            {
                switch (sw.BallSearch)
                {
                    case BallSearchSignalOption.Reset:
                        SetBallSearchReset();
                        break;
                    case BallSearchSignalOption.Off:
                        SetBallSearchStop();
                        break;
                    default:
                        break;
                }
            }
        }
        if (result) //record switch
        {
            LogDebug("swOn:" + swName);
            if (_recordPlayback == RecordPlaybackOption.Record)
            {
                var switchTime = OS.GetTicksMsec() - gameLoadTimeMsec;
                var recordLine = $"sw{sw.Num}|{true}|{switchTime}";
                _recordFile?.StoreLine(recordLine);
                LogDebug(recordLine);
            }
        }
        return result;
    }

    /// <summary>
    /// Checks a switches input event by friendly name. <para/>
    /// If the "coin" switch is still held down then will return true
    /// </summary>
    /// <param name="swName"></param>
    /// <returns>on / off</returns>
    public virtual bool SwitchOn(string swName)
    {
        if (!SwitchExists(swName)) return false;
        return Machine.Switches[swName].IsOn();
    }

    /// <summary>
    /// Toggles the border and resize of the window
    /// </summary>
    public void ToggleWindowBorder()
    {
        OS.WindowBorderless = !OS.WindowBorderless;
        OS.WindowResizable = !OS.WindowBorderless;
    }

    /// <summary>
    /// Invokes UpdateLamps on all groups marked as Mode within the scene tree. scene tree CallGroup
    /// </summary>
    /// <param name="sceneTree"></param>
    /// <param name="group"></param>
    /// <param name="method"></param>
    public virtual void UpdateLamps(SceneTree sceneTree, string group = "Mode", string method = "UpdateLamps") => sceneTree.CallGroup(group, method);

    /// <summary>
    /// Gets the audiomanager reference from tree and sets up audio buses
    /// </summary>
    protected virtual void SetupAudio()
	{
        LogInfo("setting up audio");
        AudioManager = GetNode<AudioManager>("AudioManager");

        AudioServer.SetBusVolumeDb(0, GameSettings?.MasterVolume ?? 0);
		AudioServer.SetBusVolumeDb(1, GameSettings?.MusicVolume ?? -6);
		AudioServer.SetBusVolumeDb(2, GameSettings?.SfxVolume ?? -6);
		AudioServer.SetBusVolumeDb(3, GameSettings?.VoiceVolume ?? -6);		
		AudioManager.MusicEnabled = GameSettings?.MusicEnabled ?? true;
		AudioManager.SfxEnabled = GameSettings?.SfxEnabled ?? true;
		AudioManager.VoiceEnabled = GameSettings?.VoiceEnabled ?? true;
	}
	/// <summary>
	/// Creates the recordings directory in the users folder
	/// </summary>
	/// <returns>The path to the recordings</returns>
	private string CreateRecordingsDirectory()
	{
		var userDir = OS.GetUserDataDir();
		var d = new Directory();
		var dir = userDir + $"/recordings/";
		if (!d.DirExists(dir))
			d.MakeDir(dir);

		return dir;
	}

    /// <summary>
    /// Parses user command lines args in the --key=value format
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, string> GetCommandLineArgs()
    {
        var cmd = OS.GetCmdlineArgs();
        LogInfo("cmd line available: ", cmd?.Length);
        Dictionary<string, string> _args = new Dictionary<string, string>();
        _args.Add("base_path", OS.GetExecutablePath());
        foreach (var arg in cmd)
        {
            if (arg.Contains("="))
            {
                var keyValue = arg.Split("=");
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0].LStrip("--");
                    if (!_args.ContainsKey(key))
                    {
                        _args.Add(key, keyValue[1]);
                    }
                }
            }
        }

        return _args;
    }

    private bool LampExists(string name)
	{
		if (!Machine.Lamps.ContainsKey(name))
		{
			LogError($"ERROR:no lamp found for: {name}");
			return false;
		}

		return true;
	}

	private bool LedExists(string name)
	{
		if (!Machine.Leds.ContainsKey(name))
		{
			LogError($"ERROR:no led found for: {name}");
			return false;
		}

		return true;
	}

    /// <summary>
    /// Stops any game in progress
    /// </summary>
    private void OnServiceMenuEnter()
    {
        GameInPlay = false;
        ResetTilt();
        EnableFlippers(0);
    }

    private bool SolenoidExists(string name)
	{
		if (!Machine.Coils.ContainsKey(name))
		{
			LogError($"ERROR:no solenoid found for: {name}");
			return false;
		}

		return true;
	}

	private bool SwitchExists(string name)
	{
		if (!Machine.Switches.ContainsKey(name))
		{
			LogError($"ERROR:no switch found for: {name}");
			return false;
		}

		return true;
	}	
}
