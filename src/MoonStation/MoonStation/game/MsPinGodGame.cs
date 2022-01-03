using MoonStation.game;

/// <summary>
/// A custom version of PinGodGame
/// </summary>
public class MsPinGodGame : PinGodGame
{
    public int Multiplier { get; set; }

    /// <summary>
    /// Override PinGodGames AddPoints to add multiplier to score and also add bonus here. <para/>
    /// </summary>
    /// <param name="points"></param>
    /// <param name="emitUpdateSignal"></param>
    public override long AddPoints(long points, bool emitUpdateSignal = true)
    {
        var totalPoints = points * Multiplier;
        base.AddPoints(points * Multiplier, emitUpdateSignal);
        AddBonus(totalPoints / 5);
        return totalPoints;
    }

    /// <summary>
    /// override to create our own player type for this game
    /// </summary>
    /// <param name="name"></param>
    public override void CreatePlayer(string name)
    {
        Players.Add(new MsPlayer() { Name = name, Points = 0 });
    }

    /// <summary>
    /// Loads the custom <see cref="MsGameData"/> class into our settings
    /// </summary>
    public override void LoadDataFile()
    {
        GameData = GameData.Load<MsGameData>();
    }

    /// <summary>
    /// Loads the custom <see cref="MsGameSettings"/> class into our settings
    /// </summary>
    public override void LoadSettingsFile()
    {
        GameSettings = GameSettings.Load<MsGameSettings>();
    }

    public override void SaveGameData()
    {
        GameData.Save(GameData as MsGameData);
    }

    public override void SaveGameSettings()
    {
        GameSettings.Save(GameSettings as MsGameSettings);
    }

    public void SetMusicOff()
    {
        var settings = GameSettings as MsGameSettings;
        AudioManager.MusicEnabled = false;
        settings.MusicEnabled = false;
        AudioManager.Bgm = "off";
    }

    public void SetMusicOn(string menu)
    {
        var settings = GameSettings as MsGameSettings;
        settings.MusicEnabled = true;
        settings.Music = menu;
        AudioManager.Bgm = menu;
        AudioManager.MusicEnabled = true;
        LogDebug("selected music", AudioManager.Bgm);
    }

    /// <summary>
    /// setup music if on
    /// </summary>
    public override void Setup()
    {
        //base setup
        base.Setup();

        var set = GameSettings as MsGameSettings;
        if (set.MusicEnabled)
        {
            var music = set.Music;
            LogWarning("music ", music);
            SetMusicOn(music);
        }
    }
}