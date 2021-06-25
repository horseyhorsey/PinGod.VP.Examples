using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.GD;

public abstract class PinGodGameBase : Node
{
    #region Signals
    [Signal] public delegate void BallDrained();
    [Signal] public delegate void BallEnded(bool lastBall);
    [Signal] public delegate void BallSaved();
    [Signal] public delegate void BallSaveEnded();
    [Signal] public delegate void BallSaveStarted();
    [Signal] public delegate void BallSearchReset();
    [Signal] public delegate void BallSearchStop();
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
    /// <summary>
    /// Sends via implementation <see cref="OscService"/>
    /// </summary>
    public IPinballSendReceive PinballSender = new OscService();

    public PinGodLogLevel LogLevel { get; set; } = PinGodLogLevel.Info;

    const string AUDIO_MANAGER = "res://addons/PinGodGame/Audio/AudioManager.tscn";
    public static byte Tiltwarnings { get; set; }
    public AudioManager AudioManager { get; protected set; } = new AudioManager();
    public byte BallInPlay { get; set; }
    /// <summary>
	/// Is ball save active
	/// </summary>
	public bool BallSaveActive { get; internal set; }
    public BallSearchOptions BallSearchOptions { get; set; }
    public Timer BallSearchTimer { get; set; }
    public byte CurrentPlayerIndex { get; set; }
    public GameData GameData { get; private set; }
    public bool GameInPlay { get; set; }
    public GameSettings GameSettings { get; private set; }
    public bool IsBallStarted { get; internal set; }
    public bool IsTilted { get; set; }
    public PinGodPlayer Player { get; private set; }
    public List<PinGodPlayer> Players { get; private set; }
    #endregion

    internal Trough _trough;
    protected MemoryMap memMapping;
    private uint gameEndTime;
    private uint gameLoadTimeMsec;
    private uint gameStartTime;
    /// <summary>
    /// recording actions
    /// </summary>
    protected StringBuilder stringBuilder;
    private bool _playbackEnabled;
    private Queue<PlaybackEvent> _playbackQueue;

    public PinGodGameBase()
    {
        Players = new List<PinGodPlayer>();

        LoadSettingsAndData();

        gameLoadTimeMsec = OS.GetTicksMsec();

        AudioServer.SetBusVolumeDb(0, GameSettings.MasterVolume);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), GameSettings.MusicVolume);
    }

    #region Godot overrides
    public override void _EnterTree()
    {
        LogInfo("pingod:enter tree");
        _trough = GetNode("/root/Trough") as Trough;
        if (_trough == null)
            LogWarning("trough not found");

        BallSearchTimer = new Timer() { Autostart = false };
        BallSearchTimer.Connect("timeout", this, "_OnBallSearchTimeout");
    }

    public override void _ExitTree()
    {
        LogInfo("pingodbase: exit tree");
        if (PinballSender.Record)
        {
            SaveRecording();
        }
    }

    public override void _Process(float delta)
    {
        if (!_playbackEnabled)
        {
            SetProcess(false);
            LogInfo("pingodbase: process loop ended playback switches is not enabled");
            return;
        }
        else
        {
            if (_playbackQueue?.Count <= 0)
            {
                LogInfo("pingodbase: playback events ended");
                _playbackEnabled = false;
                return;
            }

            var time = OS.GetTicksMsec() - gameStartTime;
            var nextEvt = _playbackQueue.Peek().Time;
            if(nextEvt >= time)
            {
                var pEvent = _playbackQueue.Dequeue();
                var ev = new InputEventAction() { Action = pEvent.EvtName, Pressed = pEvent.State };
                Input.ParseInputEvent(ev);
                LogDebug("pingodbase: playback evt ", pEvent.EvtName);
            }
        }        
    }

    #endregion

    private void _OnBallSearchTimeout()
    {
        if (BallSearchOptions?.SearchCoils?.Length > 0)
        {
            for (int i = 0; i < BallSearchOptions?.SearchCoils.Length; i++)
            {
                SolenoidPulse(BallSearchOptions?.SearchCoils[i]);
            }
        }
        else
        {
            BallSearchTimer.Stop();
        }
    }

    #region Public Methods
    public virtual uint GetElapsedGameTime => gameEndTime - gameStartTime;
    public virtual long GetTopScorePoints => GameData?.HighScores?
            .OrderByDescending(x => x.Scores).FirstOrDefault().Scores ?? 0;
    
    /// <summary>
    /// Adds bonus to the current player
    /// </summary>
    /// <param name="points"></param>
    public virtual void AddBonus(int points)
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
    protected void AddCustomMachineItems(Godot.Collections.Dictionary<string, byte> coils, Godot.Collections.Dictionary<string, byte> switches,
        Godot.Collections.Dictionary<string, byte> lamps, Godot.Collections.Dictionary<string, byte> leds)
    {
        foreach (var coil in coils)
        {
            Machine.Coils.Add(coil.Key, new PinStateObject(coil.Value));            
        }
        var itemAddResult = string.Join(", ", coils.Keys);
        LogDebug($"pingod: added coils {itemAddResult}");
        coils.Clear();

        foreach (var sw in switches)
        {
            if (BallSearchOptions.StopSearchSwitches?.Any(x => x == sw.Key) ?? false)
            {
                Machine.Switches.Add(sw.Key, new Switch(sw.Value, BallSearchSignalOption.Off));
            }
            else
            {
                Machine.Switches.Add(sw.Key, new Switch(sw.Value, BallSearchSignalOption.Reset));
            }
        }

        itemAddResult = string.Join(", ", switches.Keys);
        LogDebug($"pingod: added switches {itemAddResult}");
        switches.Clear();

        foreach (var lamp in lamps)
        {
            Machine.Lamps.Add(lamp.Key, new PinStateObject(lamp.Value));
        }
        itemAddResult = string.Join(", ", lamps.Keys);
        LogDebug($"pingod: added lamps {itemAddResult}");
        lamps.Clear();

        foreach (var led in leds)
        {
            Machine.Leds.Add(led.Key, new PinStateObject(led.Value));
        }
        LogDebug($"pingod: added leds {string.Join(", ", leds.Keys)}");
        leds.Clear();
    }
    /// <summary>
    /// Adds points to the current player
    /// </summary>
    /// <param name="points"></param>
    /// <param name="emitUpdateSignal">Sends a <see cref="ScoresUpdated"/> signal if set. See <see cref="ScoreMode"/> for use</param>
    public virtual void AddPoints(int points, bool emitUpdateSignal = true)
    {
        if (Player != null)
        {
            Player.Points += points;
            if (emitUpdateSignal)
                EmitSignal(nameof(ScoresUpdated));
        }
    }
    public virtual void BallSearchSignals(BallSearchSignalOption searchResetOption = BallSearchSignalOption.None)
    {
        LogDebug("signals", searchResetOption.ToString());
        switch (searchResetOption)
        {
            case BallSearchSignalOption.On:
            case BallSearchSignalOption.None:
                break;
            case BallSearchSignalOption.Reset:
                EmitSignal(nameof(BallSearchReset));
                break;
            case BallSearchSignalOption.Off:
                EmitSignal(nameof(BallSearchStop));
                break;
            default:
                break;
        }
    }
    public virtual int BallsInPlay() => _trough?.BallsInPlay() ?? 0;

    /// <summary>
    /// Creates a new <see cref="PlayerBasicGame"/>. Override this for your own players
    /// </summary>
    /// <param name="name"></param>
    public virtual void CreatePlayer(string name)
    {
        Players.Add(new PlayerBasicGame() { Name = name, Points = 0 });
    }

    /// <summary>
    /// Disables all <see cref="Lamps"/> and <see cref="Leds"/>
    /// </summary>
    /// <param name="sendUpdate">Runs <see cref="UpdateLampStates"/> and <see cref="UpdateLedStates"/></param>
    public virtual void DisableAllLamps()
    {
        if (Machine.Lamps?.Count > 0)
        {
            foreach (var lamp in Machine.Lamps)
            {
                lamp.Value.State = 0;
            }
        }
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
        this.EmitSignal(nameof(GameEnded));
    }
    public virtual ulong GetLastSwitchChangedTime(string sw) => Machine.Switches[sw].TimeSinceChange();

    /// <summary>
    /// Detect if the input `isAction` found in the given switchNames
    /// </summary>
    /// <param name="switchNames"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public virtual bool IsSwitch(string[] switchNames, InputEvent input)
    {
        for (int i = 0; i < switchNames.Length; i++)
        {
            var sw = Machine.Switches[switchNames[i]];
            if (input.IsAction(sw.ToString()))
            {
                return true;
            }
        }

        return false;
    }
    public virtual void LogDebug(params object[] what) 
    {
        if (LogLevel <= PinGodLogLevel.Debug)
        {
            Print(what);
        }
    }
    public virtual void LogInfo(params object[] what) 
    { 
        if(LogLevel <= PinGodLogLevel.Info)
        {
            Print(what);
        }
    }
    public virtual void LogWarning(string message = null, params object[] what) 
    {
        if (LogLevel <= PinGodLogLevel.Warning)
        {
            Print(what ?? new object[] { message });
            PushWarning(message);
        }
    }
    public virtual void LogError(string message = null, params object[] what) 
    {
        if (LogLevel <= PinGodLogLevel.Warning)
        {
            PrintErr(what ?? new object[] { message });
            PushError(message);
        }
    }
    public virtual void OnBallDrained(SceneTree sceneTree, string group = "Mode", string method = "OnBallDrained") => sceneTree.CallGroup(group, method);
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
    /// Quits the game, cleans up
    /// </summary>
    /// <param name="saveData">save game on exit?</param>
    public virtual void Quit(bool saveData = true)
    {
        if (saveData)
        {
            GameData.Save(GameData);
            GameSettings.Save(GameSettings);
        }

        memMapping?.Dispose(); //dispose invokes stop as well
        LogInfo("exiting pinball sender");
        PinballSender.Stop(); //don't remove, game won't close from VP otherwise
        LogInfo("exited pinball sender");
    }

    /// <summary>
    /// Reset player warnings and tilt
    /// </summary>
    public virtual void ResetTilt()
    {
        Tiltwarnings = 0;
        IsTilted = false;
    }

    public virtual void SaveRecording()
    {
        if (PinballSender.Record)
        {
            LogInfo("saving record file");
            var userDir = OS.GetUserDataDir();
            var file = DateTime.Now.ToFileTime() + ".record";
            var d = new Directory();
            if (!d.DirExists(userDir + $"/recordings/)"))
                d.MakeDir(userDir + $"/recordings/");

            var final = userDir + $"/recordings/{file}";
            using (var txtFile = System.IO.File.CreateText(final))
            {
                txtFile.Write(stringBuilder.ToString());
            }
            stringBuilder.Clear();
            LogInfo("record file saved: ", final);
        }        
    }
    public virtual void SetBallSearchReset()
    {
        BallSearchTimer.Start(BallSearchOptions?.SearchWaitTime ?? 10);
        LogDebug("ball search reset");
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
        Machine.Lamps[name].State = state;
    }
    public virtual void SetLedState(string name, byte state, int color = 0)
    {
        if (!LedExists(name)) return;
        Machine.Leds[name].State = state;
        Machine.Leds[name].Colour = color > 0 ? color : Machine.Leds[name].Colour;
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
            System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(colour.Value.r8, colour.Value.b8, colour.Value.g8))
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
    /// Sets up the AudioManager to play standard music and Sfx
    /// </summary>
    public virtual void Setup()
    {
        //create and get ref to the audiomanager scene
        var audioMan = Load(AUDIO_MANAGER) as PackedScene;
        AddChild(audioMan.Instance());
        AudioManager = GetNode("AudioManager") as AudioManager;
        //set to false, no music in this particular game
        AudioManager.MusicEnabled = false;
        AudioManager.Bgm = string.Empty;
        LogInfo("PinGod: audiomanager loaded.", AudioManager != null);
    }

    /// <summary>
    /// Sets up recording or playback from a .recording file
    /// </summary>
    /// <param name="playbackEnabled"></param>
    /// <param name="recordingEnabled"></param>
    /// <param name="playbackfile"></param>
    public virtual void SetUpRecordingsOrPlayback(bool playbackEnabled, bool recordingEnabled, string playbackfile)
    {
        _playbackEnabled = playbackEnabled;
        LogDebug("setting up recording / playback");
        if (playbackEnabled)
        {
            if (string.IsNullOrWhiteSpace(playbackfile))
            {
                LogWarning("set a playback file from user directory eg: user://recordings/26232613.record");
                _playbackEnabled = false;
            }
            else
            {
                LogInfo("running playback file: ", playbackfile);
                var pBackFile = new File();
                if (pBackFile.Open(playbackfile, File.ModeFlags.Read) == Error.FileNotFound)
                {
                    _playbackEnabled = false;
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
                    LogInfo(_playbackQueue.Count, " playback events queued. First event: ", _playbackQueue.Peek().EvtName);
                }
            }
        }
        else if (recordingEnabled)
        {
            PinballSender.Record = true;
            stringBuilder = new System.Text.StringBuilder();
            LogDebug("game switch recording on");
        }
    }
    public class PlaybackEvent
    {
        public PlaybackEvent(string evtName, bool state, uint time)
        {
            EvtName = evtName;
            State = state;
            Time = time;
        }

        public string EvtName { get; }
        public bool State { get; }
        public uint Time { get; }
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
                EmitSignal(nameof(BallSearchReset));
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
    /// <param name="numOfBalls"></param>
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

            if (PinballSender.Record)
            {
                var recordLine = $"{swName}|{false}|{OS.GetTicksMsec() - gameStartTime}";
                stringBuilder?.AppendLine(recordLine);
                LogDebug(recordLine);
            }

            if (BallSearchOptions.IsSearchEnabled)
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
    /// Wrapper for the Input.IsActionReleased
    /// </summary>
    /// <param name="sw"></param>
    /// <param name="event"></param>
    /// <param name="resetOption">Sends a BallSearch signal</param>
    /// <returns></returns>
    public virtual bool SwitchOff(byte sw, InputEvent @event, BallSearchSignalOption resetOption = BallSearchSignalOption.None)
    {
        var pressed = @event.IsActionReleased("sw" + sw);
        if (pressed)
        {
            if (SwitchOn("plunger_lane")) //TODO
            {
                LogDebug("swOff:" + sw);
                BallSearchSignals(resetOption);
            }
        }
        return pressed;
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
        if (result && BallSearchOptions.IsSearchEnabled)
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
        if (result)
        {
            LogDebug("swOn:" + swName);

            if (PinballSender.Record)
            {
                var switchTime = OS.GetTicksMsec() - gameStartTime;
                var recordLine = $"sw{sw.Num}|{true}|{switchTime}";
                stringBuilder?.AppendLine(recordLine);
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
    /// Wrapper for the Input.IsActionPressed. Checks actions prefixed with "sw"
    /// </summary>
    /// <param name="sw"></param>
    /// <param name="event"></param>
    /// <param name="resetOption">Sends a BallSearch signal</param>
    /// <returns></returns>
    public virtual bool SwitchOn(byte sw, InputEvent @event, BallSearchSignalOption resetOption = BallSearchSignalOption.None)
    {
        var pressed = @event.IsActionPressed("sw" + sw);
        if (pressed)
        {
            LogDebug("swOn:" + sw);
            if (SwitchOn("plunger_lane")) //TODO
            {
                BallSearchSignals(resetOption);
            }
        }
        return pressed;
    }
    /// <summary>
    /// Invokes UpdateLamps on all groups marked as Mode within the scene tree.
    /// </summary>
    /// <param name="sceneTree"></param>
    public virtual void UpdateLamps(SceneTree sceneTree, string group = "Mode", string method = "UpdateLamps") => sceneTree.CallGroup(group, method);
    #endregion

    private bool LampExists(string name)
    {
        if (!Machine.Lamps.ContainsKey(name))
        {
            LogError("ERROR:no lamp found for: ", name);
            return false;
        }

        return true;
    }
    private bool LedExists(string name)
    {
        if (!Machine.Leds.ContainsKey(name))
        {
            LogError("ERROR:no led found for: ", name);
            return false;
        }

        return true;
    }
    private void LoadSettingsAndData()
    {
        GameData = GameData.Load();
        GameSettings = GameSettings.Load();
    }
    private bool SolenoidExists(string name)
    {
        if (!Machine.Coils.ContainsKey(name))
        {
            LogError("ERROR:no solenoid found for: ", name);
            return false;
        }

        return true;
    }
    private bool SwitchExists(string name)
    {
        if (!Machine.Switches.ContainsKey(name))
        {
            LogError("ERROR:no switch found for: ", name);
            return false;
        }

        return true;
    }
}

public enum PinGodLogLevel
{
    Debug,
    Info,    
    Warning,
    Error
}