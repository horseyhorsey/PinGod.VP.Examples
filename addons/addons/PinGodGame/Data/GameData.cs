using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Game data for the machine. %AppData%\Godot\app_userdata <para/>
/// You can inherit this class and use the generic save, load so should work for all games.
/// </summary>
public class GameData
{
	/// <summary>
	/// File saved in the game folder in users roaming GODOT 
	/// </summary>
	[NotMapped]
	const string GAME_DATA_FILE = "user://gamedata.save";

	/// <summary>
	/// Total Balls played
	/// </summary>
	public int BallsPlayed { get; set; }
	/// <summary>
	/// Total Balls started
	/// </summary>
	public int BallsStarted { get; set; }
	/// <summary>
	/// Credits saved in machine
	/// </summary>
	public byte Credits { get; set; }
	public int GamesFinished { get; set; }
	public int GamesPlayed { get; set; }
	public int GamesStarted { get; set; }
	public List<HighScore> HighScores { get; set; } = new List<HighScore>();
	/// <summary>
	/// Total times tilted
	/// </summary>
	public int Tilted { get; set; }
	public ulong TimePlayed { get; set; }

	/// <summary>
	/// De-serializes gamedata from json if Type is <see cref="GameData"/>
	/// </summary>
	public static T DeserializeGameData<T>(string gameSettingsJson) where T : GameData => JsonConvert.DeserializeObject<T>(gameSettingsJson);

    /// <summary>
    /// Loads the <see cref="GAME_DATA_FILE"/>
    /// </summary>
    /// <returns></returns>
    public static GameData Load()
	{
		var saveGame = new File();
		var err = saveGame.Open(GAME_DATA_FILE, File.ModeFlags.Read);
		GameData gameData = new GameData();
		if (err != Error.FileNotFound)
		{
			gameData = JsonConvert.DeserializeObject<GameData>(saveGame.GetLine());
			saveGame.Close();
		}
		else
		{
			Save(gameData);
		}

		return gameData;
	}

	/// <summary>
	/// Loads generic game data file from the user directory. Creates a new game data file if there isn't one available
	/// </summary>
	public static T Load<T>() where T : GameData
	{
		var dataSave = new File();
		var err = dataSave.Open(GAME_DATA_FILE, File.ModeFlags.Read);
		T gD = default(T);
		if (err != Error.FileNotFound)
		{
			gD = DeserializeGameData<T>(dataSave.GetLine());
			dataSave.Close();
			GD.Print("GameData loaded from file");
		}
		else
		{
			Save(gD);
			GD.Print("new GameData created");
		}

		return gD;
	}

	/// <summary>
	/// Saves <see cref="GameData"/>
	/// </summary>
	public static void Save(GameData gameData)
	{
		var saveGame = new File();
		saveGame.Open(GAME_DATA_FILE, File.ModeFlags.Write);
		saveGame.StoreLine(JsonConvert.SerializeObject(gameData));
		saveGame.Close();
	}

	/// <summary>
	/// Saves generic <see cref="GameData"/>
	/// </summary>
	public static void Save<T>(T gameData) where T : GameData
	{
		var saveGame = new File();
		saveGame.Open(GAME_DATA_FILE, File.ModeFlags.Write);
		saveGame.StoreLine(JsonConvert.SerializeObject(gameData));
		saveGame.Close();
	}
}
