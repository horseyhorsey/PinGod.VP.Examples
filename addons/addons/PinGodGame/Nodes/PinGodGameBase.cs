using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Godot.GD;

public enum BallSearchSignalOption
{
    None,
    Reset,
    Off,
    On
}

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
	[Signal] public delegate void BallStarted();
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
    public bool LogSwitchEvents = false;
    public byte MaxPlayers = 4;
    /// <summary>
    /// Sends via implementation <see cref="OscService"/>
    /// </summary>
    public IPinballSendReceive PinballSender = new OscService();

    public static byte Tiltwarnings { get; set; }
    public AudioManager AudioManager { get; protected set; } = new AudioManager();
    public byte BallInPlay { get; set; }
    /// <summary>
	/// Is ball save active
	/// </summary>
	public bool BallSaveActive { get; internal set; }
    public byte CurrentPlayerIndex { get; set; }
    public GameData GameData { get; private set; }
    public bool GameInPlay { get; set; }
    public GameSettings GameSettings { get; private set; }
    public bool IsBallStarted { get; internal set; }
    public bool IsTilted { get; set; }
    public PinGodPlayer Player { get; private set; }
    public List<PinGodPlayer> Players { get; private set; }
    #endregion

    protected MemoryMap memMapping;
    private Trough _trough;
    private uint gameEndTime;
    private uint gameLoadTimeMsec;
    private uint gameStartTime;
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
        Print("pingod:enter tree");
        _trough = GetNode("/root/Trough") as Trough;
        if (_trough == null)
            Print("trough not found");
    }
    public override void _Notification(int what)
    {
		if (what == NotificationWmQuitRequest)
		{
			if (PinballSender.Record)
			{
				PinballSender.SaveRecording();
			}
		}
	}
    #endregion

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
    public virtual void AddPoints(int points, bool emitUpdateSignal = true)
	{
		if (Player != null)
		{
			Player.Points += points;
			if(emitUpdateSignal)
				EmitSignal(nameof(ScoresUpdated));
		}
	}
    public virtual void BallSearchSignals(BallSearchSignalOption searchResetOption = BallSearchSignalOption.None)
    {
        Print("signals", searchResetOption.ToString());
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
		if(Machine.Lamps?.Count > 0)
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

		EnableFlippers(0);

		if (Players.Count > 0)
		{
			Print("end of ball. current ball:" + BallInPlay);
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

				Print("ball in play " + BallInPlay);
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
		if(saveData)
        {
			GameData.Save(GameData);
			GameSettings.Save(GameSettings);
		}

		memMapping?.Dispose(); //dispose invokes stop as well
		Print("exiting pinball sender");
		PinballSender.Stop(); //don't remove, game won't close from VP otherwise
		Print("exited pinball sender");
	}
	/// <summary>
	/// Reset player warnings and tilt
	/// </summary>
	public virtual void ResetTilt()
	{
		Tiltwarnings = 0;
		IsTilted = false;
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
    public abstract void Setup();
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
		Print("base:start game");
		if (IsTilted)
		{
			Print("base: Cannot start game when game is tilted");
			return false;
		}

		if (!GameInPlay && GameData.Credits > 0) //first player start game
        {
            Print("starting game, checking trough...");
            if (!_trough.IsTroughFull()) //return if trough isn't full. TODO: needs debug option to remove check
            {
                Print("Trough not ready. Can't start game with empty trough.");
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
            Print("signal: player 1 added");
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
			Print($"signal: player added. {Players.Count}");
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
		Print("base:starting new ball");
		GameData.BallsStarted++;
		ResetTilt();
		Player = Players[CurrentPlayerIndex];		
		if(Player.ExtraBalls > 0)
        {
			Player.ExtraBalls--;
			Print("base: player shoot again");
		}
		_trough.PulseTrough();
		EnableFlippers(1);
		EmitSignal(nameof(BallStarted));
	}
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
        var result = Machine.Switches[swName].IsOff(inputEvent);
        if (LogSwitchEvents && result)
        {
            Print("swOff:" + swName);
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
                if (LogSwitchEvents)
                {
                    Print("swOff:" + sw);
                }
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
		var result = Machine.Switches[swName].IsOn(inputEvent);
		if (LogSwitchEvents && result) Print("swOn:" + swName);
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
			if (LogSwitchEvents) Print("swOn:" + sw);
			if (SwitchOn("plunger_lane")) //TODO
			{
				BallSearchSignals(resetOption);
			}
		}
		return pressed;
	}

    /// <summary>
    /// Invokes UpdateLamps on all groups within the scene tree.
    /// </summary>
    /// <param name="sceneTree"></param>
    public virtual void UpdateLamps(SceneTree sceneTree, string group = "update_lamps", string method = "UpdateLamps") => sceneTree.CallGroup(group, method);
    #endregion

    private bool LampExists(string name)
    {
        if (!Machine.Lamps.ContainsKey(name))
        {
            PrintErr("ERROR:no lamp found for: ", name);
            return false;
        }

        return true;
    }
    private bool LedExists(string name)
    {
        if (!Machine.Leds.ContainsKey(name))
        {
            PrintErr("ERROR:no led found for: ", name);
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
			PrintErr("ERROR:no solenoid found for: ", name);
			return false;
		}

		return true;
	}
	private bool SwitchExists(string name)
    {
        if (!Machine.Switches.ContainsKey(name))
        {
			PrintErr("ERROR:no switch found for: ", name);
			return false;
		}

		return true;
	}
}
