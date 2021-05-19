using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using static Godot.GD;

/// <summary>
/// Game data for the machine. Add anything you like here it be saved / loaded
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
    public uint TimePlayed { get; set; }


	#region Private Methods

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
	/// Saves <see cref="GameData"/>
	/// </summary>
	public static void Save(GameData gameData)
	{
		var saveGame = new File();
		saveGame.Open(GAME_DATA_FILE, File.ModeFlags.Write);
		saveGame.StoreLine(JsonConvert.SerializeObject(gameData));
		saveGame.Close();
	}

	#endregion
}
public class HighScore
{
	public string Name { get; set; }
	public string Title { get; set; }
	public long Scores { get; set; }
	public DateTime Created { get; set; }
}
