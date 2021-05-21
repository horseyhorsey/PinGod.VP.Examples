using Godot;
using System.Collections.Generic;
using System.Linq;
using static Godot.GD;

public abstract class PinGodGameBase : Node
{
	#region Signals
	[Signal] public delegate void BallEnded(bool lastBall);
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
	[Signal] public delegate void PlayerAdded();
	[Signal] public delegate void ModeTimedOut(string title);
	[Signal] public delegate void MultiballStarted();
	[Signal] public delegate void ServiceMenuEnter();
	[Signal] public delegate void ServiceMenuExit();
	[Signal] public delegate void ScoresUpdated();
	[Signal] public delegate void ScoreEntryEnded();
	#endregion

	#region Public Properties - Standard Pinball / Players
	public bool GameInPlay { get; set; }
	public bool InBonusMode = false;
	public bool IsMultiballRunning = false;
	public bool IsTilted { get; set; }
	public byte BallInPlay { get; set; }
	public byte CurrentPlayerIndex { get; set; }
	public byte FlippersEnabled = 0;
	public byte MaxPlayers = 4;
	public bool LogSwitchEvents = false;
	private uint gameLoadTimeMsec;
	private uint gameStartTime;
	private uint gameEndTime;

	public static byte Tiltwarnings { get; set; }
	public List<PinGodPlayer> Players { get; private set; }
	public PinGodPlayer Player { get; private set; }
	public GameData GameData { get; private set; }
	public GameSettings GameSettings { get; private set; }
	public AudioManager AudioManager { get; protected set; } = new AudioManager();
	#endregion

	public PinGodGameBase()
	{
		Players = new List<PinGodPlayer>();

		LoadSettingsAndData();

		gameLoadTimeMsec = OS.GetTicksMsec();

		AudioServer.SetBusVolumeDb(0, GameSettings.MasterVolume);
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), GameSettings.MusicVolume);
	}

	#region Public Methods
	/// <summary>
	/// Adds points to the current player
	/// </summary>
	/// <param name="points"></param>
	public virtual void AddPoints(int points)
	{
		if (Player != null)
		{
			Player.Points += points;
			EmitSignal(nameof(ScoresUpdated));
		}
	}

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

	/// <summary>
	/// Disables all lamps in <see cref="Lamps"/> <para/>
	/// TODO: Change so we can send array of lamp nums in one hit over address.
	/// </summary>
	public virtual void DisableAllLamps()
	{
		foreach (var lamp in Machine.Lamps)
		{
			OscService.SetLampState(lamp.Value, 0);
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

	public virtual uint GetElapsedGameTime => gameEndTime - gameStartTime;

	public virtual long GetTopScorePoints => GameData?.HighScores?
		.OrderByDescending(x => x.Scores).FirstOrDefault().Scores ?? 0;

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
			if (!Trough.IsTroughFull()) //return if trough isn't full. TODO: needs debug option to remove check
			{
				Print("Trough not ready. Can't start game with empty trough.");
				EmitSignal(nameof(BallSearchReset));
				return false;
			}

			Players.Clear(); //clear any players from previous game
			GameInPlay = true;

			//remove a credit and add a new player
			GameData.Credits--;
			Players.Add(new PlayerBasicGame() { Name = $"P{Players.Count + 1}", Points = 0 });
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
			Players.Add(new PlayerBasicGame() { Name = $"P{Players.Count + 1}", Points = 0 });
			Print($"signal: player added. {Players.Count}");
			EmitSignal(nameof(PlayerAdded));
		}

		return false;
	}

	/// <summary>
	/// Starts a new ball, changing to next player, enabling flippers and ejecting trough and sending <see cref="BallStarted"/>
	/// </summary>
	public virtual void StartNewBall()
	{
		Print("base:starting new ball");
		GameData.BallsStarted++;
		ResetTilt();
		Player = Players[CurrentPlayerIndex];
		EnableFlippers(1);
		SolenoidPulse("trough", 100);
		EmitSignal(nameof(BallStarted));
	}

	/// <summary>
	/// Sets MultiBall running to send a <see cref="MultiballStarted"/>
	/// </summary>
	/// <param name="numOfBalls"></param>
	/// <param name="ballSaveTime"></param>
	public virtual void StartMultiBall(byte numOfBalls, byte ballSaveTime)
	{		
		Trough.BallSaveMultiballSeconds = ballSaveTime;
		Trough.NumOfBallToSave = numOfBalls;
		IsMultiballRunning = true;
		EmitSignal(nameof(MultiballStarted));
	}

	public virtual void SolenoidOn(string name, int state) { OscService.SetCoilState(Machine.Coils[name], state) ; }

	public virtual void SolenoidPulse(string name, byte pulse = 125) { OscService.PulseCoilState(Machine.Coils[name], pulse); }

	/// <summary>
	/// Use in Godot _Input events. Checks a switches input event by friendly name from switch collection <para/>
	/// "coin", @event
	/// </summary>
	/// <param name="swName"></param>
	/// <param name="inputEvent"></param>
	/// <returns></returns>
	public virtual bool SwitchOn(string swName, InputEvent inputEvent)
	{		
		if (Machine.Switches.ContainsKey(swName))
		{
			var result = Machine.Switches[swName].IsOn(inputEvent);
			if (LogSwitchEvents && result) Print("swOn:" + swName);
			return result;
		}

		PrintErr("no switch found in dict for", swName);
		return false;
	}

	/// <summary>
	/// Checks a switches input event by friendly name. <para/>
	/// If the "coin" switch is still held down then will return true
	/// </summary>
	/// <param name="swName"></param>
	/// <returns>on / off</returns>
	public virtual bool SwitchOn(string swName)
	{		
		if (Machine.Switches.ContainsKey(swName)) 
		{
			return Machine.Switches[swName].IsOn(); 
		}

		PrintErr("no switch found in dict for", swName);
		return false;
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
			if (SwitchOn("plunger_lane"))
			{
				BallSearchSignals(resetOption);
			}
		}
		return pressed;
	}

	/// <summary>
	/// Is the switch you're checking a trough switch? <see cref="Trough.TroughSwitches"/>
	/// </summary>
	/// <param name="input"></param>
	/// <returns>The number of the trough switch. 0 not found</returns>
	public virtual byte IsTroughSwitch(InputEvent input)
	{
		for (int i = 0; i < Trough.TroughSwitches.Length; i++)
		{
			if (input.IsActionPressed("sw"+ Trough.TroughSwitches[i]))
			{
				return Trough.TroughSwitches[i];
			}
		}
		return 0;
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
		if (Machine.Switches.ContainsKey(swName)) 
		{
			var result = Machine.Switches[swName].IsOff(inputEvent);
			if(LogSwitchEvents && result)
			{
				Print("swOff:" + swName);
			}
			return result; 
		}

		PrintErr("no switch found in dict for", swName);
		return false;
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
			if (SwitchOn("plunger_lane"))
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

	#endregion

	private void LoadSettingsAndData()
	{
		GameData = GameData.Load();
		GameSettings = GameSettings.Load();
	}
}

public enum BallSearchSignalOption
{
	None,
	Reset,
	Off,
	On
}
