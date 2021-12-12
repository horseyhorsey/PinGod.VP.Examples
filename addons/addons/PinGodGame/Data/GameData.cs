using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Game data for the machine. <para/>
/// %AppData%\Godot\app_userdata
/// </summary>
public class GameData
{
	/// <summary>
	/// File saved in the game folder in users roaming GODOT 
	/// </summary>
	[NotMapped]
	const string GAME_DATA_FILE = "user://gamedata.save";

	public int BallsPlayed { get; set; }
	public int BallsStarted { get; set; }
	public byte Credits { get; set; }
	public int GamesFinished { get; set; }
	public int GamesPlayed { get; set; }
	public int GamesStarted { get; set; }
	public List<HighScore> HighScores { get; set; } = new List<HighScore>();
	public int Tilted { get; set; }
	public ulong TimePlayed { get; set; }

	/// <summary>
	/// De-serializes gamedata from json if Type is <see cref="GameData"/>
	/// </summary>
	public static T DeserializeGameData<T>(string gameSettingsJson) where T : GameData => JsonConvert.DeserializeObject<T>(gameSettingsJson);

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
			GD.Print("gamedata loaded from file");
		}
		else
		{
			Save(gD);
			GD.Print("new gamedata created");
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
