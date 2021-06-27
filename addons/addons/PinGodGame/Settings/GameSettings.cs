using Godot;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using static Godot.GD;

/// <summary>
/// Game settings for the machine. Add anything you like here it be saved / loaded <para/>
/// TODO: 
/// </summary>
public class GameSettings
{
    /// <summary>
    /// File saved in the game folder in users roaming GODOT 
    /// </summary>
    [NotMapped]
    const string GAME_SETTINGS_FILE = "user://settings.save";
    public byte BallsPerGame { get; set; } = 3;
    public byte MaxHiScoresCount { get; set; } = 5;
    public float MasterVolume { get; set; } = 0;
    /// <summary>
    /// Decibel volume. minus values
    /// </summary>
    public float MusicVolume { get; set; } = 0;

    public Display Display { get; set; } = new Display();

    /// <summary>
	/// Loads game data from user directory. Creates a new save if doesn't exist
	/// </summary>
	public static GameSettings Load()
    {
        var settingsSave = new File();
        var err = settingsSave.Open(GAME_SETTINGS_FILE, File.ModeFlags.Read);
        Print("settings loaded: " + err.ToString());
        GameSettings gS = new GameSettings();
        if (err != Error.FileNotFound)
        {
            gS = JsonConvert.DeserializeObject<GameSettings>(settingsSave.GetLine());
            settingsSave.Close();            
        }
        else
        {
            Save(gS);
        }

        return gS;
    }

    /// <summary>
    /// Saves the <see cref="GameData"/>
    /// </summary>
    public static void Save(GameSettings settings)
    {
        var saveGame = new File();
        saveGame.Open(GAME_SETTINGS_FILE, File.ModeFlags.Write);
        saveGame.StoreLine(JsonConvert.SerializeObject(settings));
        saveGame.Close();
    }
}

public class Display
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public bool AlwaysOnTop { get; set; } = true;
    public bool LowDpi { get; set; } = false;
    public bool FullScreen { get; set; } = false;
    public bool NoWindow { get; set; }
}