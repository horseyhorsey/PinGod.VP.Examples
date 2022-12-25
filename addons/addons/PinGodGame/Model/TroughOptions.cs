/// <summary>
/// Trough settings
/// </summary>
public class TroughOptions
{
    /// <summary>
    /// Initializes trough mech settings for a game
    /// </summary>
    /// <param name="switches"></param>
    /// <param name="coil"></param>
    /// <param name="plungerLaneSw"></param>
    /// <param name="autoPlungerCoil"></param>
    /// <param name="earlySaveSwitches"></param>
    /// <param name="ballSaveSeconds"></param>
    /// <param name="mballSaveSeconds"></param>
    /// <param name="ballSaveLamp"></param>
    /// <param name="ballSaveLed"></param>
    /// <param name="numBallsToSave"></param>
    public TroughOptions(string[] switches, string coil, string plungerLaneSw, string autoPlungerCoil = "", string[] earlySaveSwitches = null, int ballSaveSeconds = 8, int mballSaveSeconds = 8, string ballSaveLamp = "", string ballSaveLed = "", int numBallsToSave = 1)
    {
        Switches = switches;
        Coil = coil;
        PlungerLaneSw = plungerLaneSw;
        AutoPlungerCoil = autoPlungerCoil;
        EarlySaveSwitches = earlySaveSwitches;
        BallSaveSeconds = ballSaveSeconds;
        MballSaveSeconds = mballSaveSeconds;
        BallSaveLamp = ballSaveLamp;
        BallSaveLed = ballSaveLed;
        NumBallsToSave = numBallsToSave;
    }

    /// <summary>
    /// Switch names
    /// </summary>
    public string[] Switches { get; }
    /// <summary>
    /// Main coil to pulse
    /// </summary>
    public string Coil { get; }
    /// <summary>
    /// Shooter lane, Plunger Lane switch name
    /// </summary>
    public string PlungerLaneSw { get; }
    /// <summary>
    /// Auto plunger solenoid
    /// </summary>
    public string AutoPlungerCoil { get; }
    /// <summary>
    /// Switches like out-lanes to early save
    /// </summary>
    public string[] EarlySaveSwitches { get; }
    /// <summary>
    /// Default ball save time
    /// </summary>
    public int BallSaveSeconds { get; set; }
    /// <summary>
    /// Seconds left in a multi-ball
    /// </summary>
    public int MballSaveSeconds { get; set; }
    /// <summary>
    /// Ball save lamp name
    /// </summary>
    public string BallSaveLamp { get; }
    /// <summary>
    /// Ball save led name
    /// </summary>
    public string BallSaveLed { get; }
    /// <summary>
    /// How many balls to save when in ball save
    /// </summary>
    public int NumBallsToSave { get; set; }
}