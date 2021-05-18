using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using static Godot.GD;

public abstract class PinGodGameBase : Node
{
	const string GAME_DATA_FILE = "user://gamedata.save";
	const string GAME_SETTINGS_FILE = "user://settings.save";

	#region Signals
	[Signal] public delegate void BallEnded();
	[Signal] public delegate void BallStarted();
	[Signal] public delegate void BonusEnded();
	[Signal] public delegate void CreditAdded();
	[Signal] public delegate void GameEnded();
	[Signal] public delegate void GamePaused();
	[Signal] public delegate void GameResumed();
	[Signal] public delegate void GameStarted();
	[Signal] public delegate void GameTilted();
	[Signal] public delegate void PlayerAdded();
	[Signal] public delegate void ServiceMenuEnter();
	[Signal] public delegate void ServiceMenuExit();
	[Signal] public delegate void ScoresUpdated();
	[Signal] public delegate void ScoreEntryEnded();
	#endregion

	#region Public Properties - Standard Pinball / Players
	public Dictionary<string, byte> Lamps { get; private set; }
	public const byte CreditButtonNum = 2;
	public const byte FlippersEnableCoil = 2;
	public const byte SlamTiltSwitch = 16;
	public const byte StartSwitchNum = 19;	
	public const byte TiltSwitchNum = 17;
	public const byte TroughSolenoid = 1;
	public static bool GameInPlay { get; set; }
	public static bool InBonusMode = false;
	public static bool IsTilted { get; set; }
	public static byte BallInPlay { get; set; }	
	public static byte Credits { get; set; }
	public static byte CurrentPlayerIndex { get; set; }
	public static byte FlippersEnabled = 0;
	public static byte MaxPlayers = 4;

	private uint gameLoadTimeMsec;
	private uint gameStartTime;
	private uint gameEndTime;

	public static byte Tiltwarnings { get; set; }
	public static List<PinGodPlayer> Players { get; set; }
	public static PinGodPlayer Player { get; set; }
	public static GameData GameData { get; private set; }
	public static GameSettings GameSettings { get; private set; }

	#endregion

	public PinGodGameBase()
	{
		Players = new List<PinGodPlayer>();
		Lamps = new Dictionary<string, byte>();		

		GameSettings = new GameSettings();
		LoadGameSettings();

		GameData = new GameData() { GamesPlayed = 0 };
		LoadGameData();

		gameLoadTimeMsec = OS.GetTicksMsec();

		AudioServer.SetBusVolumeDb(0, GameSettings.MasterVolume);
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), GameSettings.MusicVolume);
	}

	/// <summary>
	/// Save game data before exit
	/// </summary>
	public override void _ExitTree()
	{
		SaveGameData();
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

	/// <summary>
	/// Disables all lamps in <see cref="Lamps"/> <para/>
	/// TODO: Change so we can send array of lamp nums in one hit over address.
	/// </summary>
	public virtual void DisableAllLamps()
	{
		foreach (var lamp in Lamps)
		{
			OscService.SetLampState(lamp.Value, 0);
		}
	}

	public virtual void EnableFlippers(byte enabled)
	{
		FlippersEnabled = enabled;
		OscService.SetCoilState(FlippersEnableCoil, enabled);
	}

	/// <summary>
	/// Ends the current ball. Changes the player <para/>
	/// Sends <see cref="BallEnded"/> signal
	/// </summary>
	/// <returns>True if all balls finished, game is finished</returns>
	public virtual bool EndBall()
	{
		EnableFlippers(0);

		if (GameInPlay && Players.Count > 0)
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
				this.EmitSignal(nameof(BallEnded));
				return true;
			}
			else
			{
				//signal that ball has ended
				this.EmitSignal(nameof(BallEnded));
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

		if (!GameInPlay && GameGlobals.GameData.Credits > 0) //first player start game
		{
			if (!Trough.IsTroughFull()) //return if trough isn't full. TODO: needs debug option to remove check
			{
				Print("Trough not ready. Can't start game with empty trough.");
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
		OscService.PulseCoilState(TroughSolenoid);
		EmitSignal(nameof(BallStarted));
	}
	#endregion

	#region Private Methods

	/// <summary>
	/// Loads game data from user directory.
	/// </summary>
	private void LoadGameData()
	{
		var saveGame = new File();
		var err = saveGame.Open(GAME_DATA_FILE, File.ModeFlags.Read);
		Print(err.ToString());
		if (err != Error.FileNotFound)
		{
			Print("loading game data");
			GameData = JsonConvert.DeserializeObject<GameData>(saveGame.GetLine());
			saveGame.Close();
		}
		else
		{
			SaveGameData();
			Print("saved default data");
		}
	}

	/// <summary>
	/// Loads game data from user directory.
	/// </summary>
	private void LoadGameSettings()
	{
		var settingsSave = new File();
		var err = settingsSave.Open(GAME_SETTINGS_FILE, File.ModeFlags.Read);
		Print(err.ToString());
		if (err != Error.FileNotFound)
		{
			Print("loading game settings");
			GameSettings = JsonConvert.DeserializeObject<GameSettings>(settingsSave.GetLine());
			settingsSave.Close();
		}
		else
		{
			SaveGameSettings();
			Print("saved default setting");
		}
	}

	/// <summary>
	/// Saves the <see cref="GameData"/>
	/// </summary>
	private void SaveGameData()
	{
		var saveGame = new File();
		saveGame.Open(GAME_DATA_FILE, File.ModeFlags.Write);
		saveGame.StoreLine(JsonConvert.SerializeObject(GameData));
		saveGame.Close();
		Print("saved game data");
	}

	/// <summary>
	/// Saves the <see cref="GameData"/>
	/// </summary>
	private void SaveGameSettings()
	{
		var saveGame = new File();
		saveGame.Open(GAME_SETTINGS_FILE, File.ModeFlags.Write);
		saveGame.StoreLine(JsonConvert.SerializeObject(GameSettings));
		saveGame.Close();
		Print("saved game settings");
	}
	#endregion
}
