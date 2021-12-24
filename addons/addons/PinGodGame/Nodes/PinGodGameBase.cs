using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public abstract class PinGodGameBase : Node
{
	#region Signals
	[Signal] public delegate void BallDrained();
	[Signal] public delegate void BallEnded(bool lastBall);
	[Signal] public delegate void BallSaved();
	[Signal] public delegate void BallSaveEnded();
	[Signal] public delegate void BallSaveStarted();
	[Signal] public delegate void BonusEnded();
	[Signal] public delegate void CreditAdded();
	[Signal] public delegate void GameEnded();
	[Signal] public delegate void GamePaused();
	[Signal] public delegate void GameResumed();
	[Signal] public delegate void GameStarted();
	[Signal] public delegate void GameTilted();
	[Signal] public delegate void ModeTimedOut(string title);
	[Signal] public delegate void MultiBallEnded();
	[Signal] public delegate void MultiballStarted();
	[Signal] public delegate void PlayerAdded();
	[Signal] public delegate void ScoreEntryEnded();
	[Signal] public delegate void ScoresUpdated();
	[Signal] public delegate void ServiceMenuEnter();
	[Signal] public delegate void ServiceMenuExit();
	#endregion

	#region Public Properties - Standard Pinball / Players
	public byte FlippersEnabled = 0;
	public bool InBonusMode = false;
	public bool IsMultiballRunning = false;
	public byte MaxPlayers = 4;
	const string AUDIO_MANAGER = "res://addons/PinGodGame/Audio/AudioManager.tscn";
    public AudioManager AudioManager { get; protected set; }

    private MainScene mainScene;

    public byte BallInPlay { get; set; }
    /// <summary>
    /// Is ball save active
    /// </summary>
    public bool BallSaveActive { get; internal set; }
    public BallSearchOptions BallSearchOptions { get; set; }
    public Timer BallSearchTimer { get; set; }
    /// <summary>
    /// Flag used for ball saving on the initial launch. If ball enters the shooterlane after a ball save then this is to stop the ballsave starting again
    /// </summary>
    public bool BallStarted { get; internal set; }
    public byte CurrentPlayerIndex { get; set; }
    public GameData GameData { get; internal set; }
    public bool GameInPlay { get; set; }
    public GameSettings GameSettings { get; internal set; }
    public bool IsBallStarted { get; internal set; }
    public bool IsTilted { get; set; }
    public PinGodLogLevel LogLevel { get; set; } = PinGodLogLevel.Info;
    public PinGodPlayer Player { get; private set; }
    public List<PinGodPlayer> Players { get; set; }
    public byte Tiltwarnings { get; set; }
    #endregion

    internal Trough _trough;
    /// <summary>
    /// Update lamps overlay
    /// </summary>
    protected LampMatrix _lampMatrixOverlay = null;

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
	public PinGodGameBase()
	{
		Players = new List<PinGodPlayer>();        
	}

	#region Godot overrides
	public override void _EnterTree()
	{
		LogInfo("pingod:enter tree");

		CmdArgs = GetCommandLineArgs();
		LoadDataFile();
		LoadSettingsFile();
		SetUpWindow();

		AudioServer.SetBusVolumeDb(0, GameSettings.MasterVolume);
		//AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), GameSettings.MusicVolume);        

		//get trough from tree
		_trough = GetNode("Trough") as Trough;
		if (_trough == null)
			LogWarning("trough not found");

		//create and add ball search timer
		BallSearchTimer = new Timer() { Autostart = false, OneShot = false };
		BallSearchTimer.Connect("timeout", this, "OnBallSearchTimeout");
		this.AddChild(BallSearchTimer);

		gameLoadTimeMsec = OS.GetTicksMsec();

		AudioManager = GetNode<AudioManager>("AudioManager");

		mainScene = GetNodeOrNull<MainScene>("/root/MainScene");		
	}

    public override void _ExitTree()
    {
        if (_recordPlayback == RecordPlaybackOption.Record)
        {
            LogInfo("pingodbase: exit tree, saving recordings");
            SaveRecording();
        }
        else LogInfo("pingodbase: exit tree");
    }

    public override void _Input(InputEvent @event)
    {
        //quits the game. ESC
        if (@event.IsActionPressed("quit"))
        {
            LogInfo("quit request");
            SetGameResumed();            
            if (mainScene != null)
            {
                try
                {
                    var tree = mainScene?.GetTree();
                    tree.Paused = false;
                    tree.Quit(0);
                }
                catch (Exception ex)
				{ 
					LogWarning("main scene disposed. " + ex.Message);
					GetTree().Quit(0);
				}
            }
            else
            {
                GetTree().Quit(0);
            }
        }

        if (@event.IsActionPressed("toggle_border"))
        {
            OS.WindowBorderless = !OS.WindowBorderless;
            if (OS.WindowBorderless)
            {
                GameSettings.Display.X = OS.WindowPosition.x;
                GameSettings.Display.Y = OS.WindowPosition.y;
                GameSettings.Display.Width = OS.WindowSize.x;
                GameSettings.Display.Height = OS.WindowSize.y;
                OS.WindowResizable = false;
            }
            else
            {
                OS.WindowResizable = true;
            }
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
            LogInfo("pingodbase: process loop ended recording playback");
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

    public override void _Ready()
    {
        base._Ready();

		BallSearchOptions = GetNode<MachineConfig>("MachineConfig")?.BallSearchOptions;

        //name the lamp matrix
        if (_lampMatrixOverlay != null)
        {
            foreach (var item in Machine.Lamps)
            {
                _lampMatrixOverlay.SetLabel(item.Value.Num, item.Key);
            }
        }

		//pingod.vp controller coil 0, sets game running on the controller
		SolenoidOn("died", 1);
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
    #endregion

    public virtual ulong GetElapsedGameTime => gameEndTime - gameStartTime;

	public virtual long GetTopScorePoints => GameData?.HighScores?
			.OrderByDescending(x => x.Scores).FirstOrDefault().Scores ?? 0;

    public Dictionary<string, string> CmdArgs { get; private set; }

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
	/// Adds credits to the GameData and emits <see cref="CreditAdded"/> signal
	/// </summary>
	/// <param name="amt"></param>
	public virtual void AddCredits(byte amt)
	{
		GameData.Credits += amt;
		EmitSignal(nameof(CreditAdded));
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

	public virtual int BallsInPlay() => _trough?.BallsInPlay() ?? 0;

	/// <summary>
	/// Creates a new <see cref="PlayerBasicGame"/>. Override this for your own players
	/// </summary>
	/// <param name="name"></param>
	public virtual void CreatePlayer(string name)
	{
		Players.Add(new PinGodPlayer() { Name = name, Points = 0 });
	}

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

	public virtual void EnableFlippers(byte enabled)
	{
		FlippersEnabled = enabled;
		this.SolenoidOn("flippers", enabled);
	}

	/// <summary>
	/// Ends the current ball. Changes the player <para/>
	/// Sends <see cref="BallEnded"/> signal
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
				if (BallInPlay > GameSettings.BallsPerGame)
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
    public virtual void LoadDataFile() => GameData = GameData.Load();

    [Obsolete("override the other methods for loading data, settings and setting up window")]
    /// <summary>
    /// Runs <see cref="LoadSettingsFile"/>, <see cref="LoadDataFile"/>, <see cref="SetUpWindow"/>
    /// </summary>
    public virtual void LoadSettingsAndData()
    {
		LogError("don't use, already init in ready");
    }

    public virtual void LoadSettingsFile() => GameSettings = GameSettings.Load();

    public virtual void LogDebug(params object[] what) => Logger.LogDebug(what);
    public virtual void LogError(string message = null, params object[] what) => Logger.LogError(message, what);
	public virtual void LogInfo(params object[] what) => Logger.LogInfo(what);
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
	/// Uses the <see cref="AudioManager.PlaySfx(string)"/>
	/// </summary>
	/// <param name="name"></param>
	/// <param name="pos"></param>
	public virtual void PlaySfx(string name)
	{
		AudioManager.PlaySfx(name);
	}

	/// <summary>
	/// Quits the game, saves data / settings and cleans up
	/// </summary>
	/// <param name="saveData">save game on exit?</param>
	public virtual void Quit(bool saveData = true)
	{
		//send game ended, died
		SolenoidOn("died", 0);

		if (saveData)
		{
			SaveGameData();
			SaveGameSettings();
		}

		memMapping?.Dispose(); //dispose invokes stop as well
	}

	/// <summary>
	/// Reset player warnings and tilt
	/// </summary>
	public virtual void ResetTilt()
	{
		Tiltwarnings = 0;
		IsTilted = false;
	}

	public virtual void SaveGameData() => GameData.Save(GameData);

	public virtual void SaveGameSettings() => GameSettings.Save(GameSettings);

	public virtual void SaveRecording()
	{
		if (_recordPlayback == RecordPlaybackOption.Record)
		{
			_recordFile?.Close();
			LogInfo("pingodbase: recording file closed");
		}
	}

	public virtual void SetBallSearchReset()
	{
		BallSearchTimer.Start(BallSearchOptions?.SearchWaitTime ?? 10);
		LogDebug("ball search timer reset.");
	}

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

	public virtual void SetLampState(string name, byte state)
	{
		if (!LampExists(name)) return;
		var lamp = Machine.Lamps[name];
		if (_lampMatrixOverlay != null) { _lampMatrixOverlay.SetState(lamp.Num, state); }
		lamp.State = state;
	}

	public virtual void SetLedState(string name, byte state, int color = 0)
	{
		if (!LedExists(name)) return;
		var led = Machine.Leds[name];
		led.State = state;
		led.Colour = color > 0 ? color : led.Colour;
		if (_lampMatrixOverlay != null) { _lampMatrixOverlay.SetState(led.Num, state); }
	}

	/// <summary>
	/// Sets led states from System.Drawing Color
	/// </summary>
	/// <param name="name"></param>
	/// <param name="state"></param>
	/// <param name="colour"></param>
	/// <param name="sendUpdate"></param>
	public virtual void SetLedState(string name, byte state, System.Drawing.Color? colour = null)
	{
		if (!LedExists(name)) return;
		var c = colour.HasValue ?
			System.Drawing.ColorTranslator.ToOle(colour.Value) : Machine.Leds[name].Colour;
		SetLedState(name, state, c);
	}

	/// <summary>
	/// Sets led state from godot color
	/// </summary>
	/// <param name="name"></param>
	/// <param name="state"></param>
	/// <param name="colour"></param>
	/// <param name="sendUpdate"></param>
	public virtual void SetLedState(string name, byte state, Color? colour = null)
	{
		if (!LedExists(name)) return;
		var c = colour.HasValue ?
			System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(colour.Value.r8, colour.Value.g8, colour.Value.b8))
			: Machine.Leds[name].Colour;
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
	/// <param name="sendUpdate"></param>
	public virtual void SetLedState(string name, byte state, byte r, byte g, byte b)
	{
		if (!LedExists(name)) return;
		var c = System.Drawing.Color.FromArgb(r, g, b);
		var ole = System.Drawing.ColorTranslator.ToOle(c);
		SetLedState(name, state, ole);
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

		LogDebug("pingodbase: setup playback?: ", _recordPlayback.ToString());
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
            LogDebug("pingodbase: game recording on");
        }
	}

    /// <summary>
    /// Sets up the window size and position from saved machine settings. See <see cref="Godot.OS"/>
    /// </summary>
    public virtual void SetUpWindow()
    {
        if (!Engine.EditorHint)
        {
            if (GameSettings.Display?.Width > 0)
            {
                if (!GameSettings.Display.NoWindow)
                {
                    LogDebug("pingodbase: setting display settings");
                    OS.WindowPosition = new Vector2(GameSettings.Display.X, GameSettings.Display.Y);

                    if (GameSettings.Display.FullScreen)
                    {
                        OS.WindowFullscreen = true;
                    }
                    else
                    {
                        OS.WindowSize = new Vector2(GameSettings.Display.Width, GameSettings.Display.Height);
                        OS.SetWindowAlwaysOnTop(GameSettings.Display.AlwaysOnTop);
                    }
                }
            }
        }
    }

    public virtual void SolenoidOn(string name, byte state)
    {
		if (!SolenoidExists(name)) return;
		Machine.Coils[name].State = state;
	}

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
	/// Attempts to start a game. If games in play then add more players until <see cref="MaxPlayers"/> <para/>
	/// </summary>
	/// <returns>True if the game was started</returns>
	public virtual bool StartGame()
	{
		LogInfo("base:start game");
		if (IsTilted)
		{
			LogInfo("base: Cannot start game when game is tilted");
			return false;
		}

		if (!GameInPlay && GameData.Credits > 0) //first player start game
		{
			LogInfo("starting game, checking trough...");
			if (!_trough.IsTroughFull()) //return if trough isn't full. TODO: needs debug option to remove check
			{
				LogInfo("Trough not ready. Can't start game with empty trough.");
				BallSearchTimer.Start(1);
				return false;
			}

			Players.Clear(); //clear any players from previous game
			GameInPlay = true;

			//remove a credit and add a new player
			GameData.Credits--;

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
			LogDebug($"signal: player added. {Players.Count}");
			EmitSignal(nameof(PlayerAdded));
		}

		return false;
	}

	/// <summary>
	/// Sets MultiBall running and trough to send a <see cref="MultiballStarted"/>
	/// </summary>
	/// <param name="numOfBalls">Number of balls to save. A 2 ball multiball would be 2</param>
	/// <param name="ballSaveTime"></param>
	public virtual void StartMultiBall(byte numOfBalls, byte ballSaveTime, float pulseTime = 0)
	{
		_trough.StartMultiball(numOfBalls, ballSaveTime, pulseTime);
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
		if (Player.ExtraBalls > 0)
		{
			Player.ExtraBalls--;
			Player.ExtraBallsAwarded++;
			LogInfo("base: player shoot again");
		}
		_trough.PulseTrough();
		EnableFlippers(1);
	}

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
	/// Invokes UpdateLamps on all groups marked as Mode within the scene tree.
	/// </summary>
	/// <param name="sceneTree"></param>
	public virtual void UpdateLamps(SceneTree sceneTree, string group = "Mode", string method = "UpdateLamps") => sceneTree.CallGroup(group, method);

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
