public class TroughOptions
{
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

    public string[] Switches { get; }
    public string Coil { get; }
    public string PlungerLaneSw { get; }
    public string AutoPlungerCoil { get; }
    public string[] EarlySaveSwitches { get; }
    public int BallSaveSeconds { get; set; }
    public int MballSaveSeconds { get; set; }
    public string BallSaveLamp { get; }
    public string BallSaveLed { get; }
    public int NumBallsToSave { get; set; }
}