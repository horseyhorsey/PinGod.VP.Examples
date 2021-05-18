public class GameSettings
{
    public static byte BallsPerGame { get; set; } = 3;
    public byte MaxHiScoresCount { get; set; } = 5;
    public float MasterVolume { get; set; } = 0;
    /// <summary>
    /// Decibel volume. -1 - 2 to turn down the main bus
    /// </summary>
    public float MusicVolume { get; set; } = 0;
}