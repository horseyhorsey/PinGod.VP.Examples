using Godot;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using static Godot.GD;

/// <summary>
/// Default game settings for the machine / game. <para/>
/// %AppData%\Godot\app_userdata
/// </summary>
public class GameSettings
{
    /// <summary>
    /// File saved in the game folder in users roaming GODOT 
    /// </summary>
    [NotMapped]
    const string GAME_SETTINGS_FILE = "user://settings.save";

    public byte BallSaveTime { get; set; } = 12;

    public byte BallsPerGame { get; set; } = 3;

    public DisplaySettings Display { get; set; } = new DisplaySettings();

    public string Language { get; set; }

    public PinGodLogLevel LogLevel { get; set; } = PinGodLogLevel.Error;

    /// <summary>
    /// Read from memory map?
    /// </summary>
    public bool MachineStatesRead { get; set; } = true;

    /// <summary>
    /// Write states to memory map?
    /// </summary>
    public bool MachineStatesWrite { get; set; } = true;

    /// <summary>
    /// Delay to write to memory. 10 default, 1 high CPU, 500 too much...
    /// </summary>
    public int MachineStatesWriteDelay { get; set; } = 10;    

    /// <summary>
    /// Decibel volume. minus values. -80 lowest
    /// </summary>
    public float MasterVolume { get; set; } = 0;

    public byte MaxExtraBalls { get; set; } = 2;

    public byte MaxHiScoresCount { get; set; } = 5;

    public bool MusicEnabled { get; set; } = true;

    /// <summary>
    /// Decibel volume. minus values. -80 lowest
    /// </summary>
    public float MusicVolume { get; set; } = -6;

    public bool SfxEnabled { get; set; } = true;

    /// <summary>
    /// Decibel volume. minus values. -80 lowest
    /// </summary>
    public float SfxVolume { get; set; } = -6;

    public bool VoiceEnabled { get; set; } = true;

    /// <summary>
    /// Decibel volume. minus values. -80 lowest
    /// </summary>
    public float VoiceVolume { get; set; } = -6;

    /// <summary>
    /// A command switch number for the memory map so user can send a byte over a switch number <para/>
    /// Set this id greater than > 0. 0 is default for game state
    /// </summary>
    public int VpCommandSwitchId { get; set; } = -1;

    /// <summary>
    /// De-serializes settings from json if Type is <see cref="GameSettings"/>
    /// </summary>
    public static T DeserializeSettings<T>(string gameSettingsJson) where T : GameSettings => JsonConvert.DeserializeObject<T>(gameSettingsJson);

    /// <summary>
	/// Loads game settings file from the user directory. Creates a new save file if there isn't one available
	/// </summary>
	public static T Load<T>() where T : GameSettings
    {
        var settingsSave = new File();
        var err = settingsSave.Open(GAME_SETTINGS_FILE, File.ModeFlags.Read);        
        T gS = Activator.CreateInstance<T>();
        if (err != Error.FileNotFound)
        {
            gS = DeserializeSettings<T>(settingsSave.GetLine());
            settingsSave.Close();
            Print("game settings loaded from file");
        }
        else
        {

            Save<T>(gS);
            Print("new game settings created " + gS.BallsPerGame);
        }

        return gS;
    }

    /// <summary>
	/// Loads game settings file from the user directory. Creates a new save file if there isn't one available
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
    /// Saves generic <see cref="GameSettings"/>
    /// </summary>
    public static void Save<T>(T settings) where T : GameSettings
    {
        var saveGame = new File();
        saveGame.Open(GAME_SETTINGS_FILE, File.ModeFlags.Write);
        saveGame.StoreLine(JsonConvert.SerializeObject(settings));
        saveGame.Close();
    }

    /// <summary>
    /// Saves <see cref="GameSettings"/>
    /// </summary>
    public static void Save(GameSettings settings)
    {
        var saveGame = new File();
        saveGame.Open(GAME_SETTINGS_FILE, File.ModeFlags.Write);
        saveGame.StoreLine(JsonConvert.SerializeObject(settings));
        saveGame.Close();
    }
}
