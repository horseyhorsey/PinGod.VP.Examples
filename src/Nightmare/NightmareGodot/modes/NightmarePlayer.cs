public class NightmarePlayer : PinGodPlayer
{
    public bool BallLockEnabled { get; internal set; }
    public int BallsLocked { get; internal set; }
    public bool BonusHeld { get; set; }
    /// <summary>
    /// Hurry, 250k, 500k, 750k, 1 million
    /// </summary>
    public bool[] CoffinStack { get; internal set; } = new bool[5];
    /// <summary>
	/// ExtraHour, ScoreBonus, HoldBonus, Extraball, 10million
	/// </summary>
    public byte[] CrossStack { get; internal set; } = new byte[5];    
    public int CrossValue { get; internal set; }
    public bool DoubleBonus { get; internal set; }
    public bool ExtraBallLit { get; internal set; }
    public int GraveYardValue { get; set; }
    public bool IsRemixMode { get; set; }
    public bool JackpotLit { get; internal set; }
    public int JackpotValue { get; internal set; }
    public bool LaneExtraBallLit { get; internal set; }
    public bool LanePanicLit { get; internal set; }
    public bool LanesLit { get; internal set; }
    public bool LeftLock { get; internal set; }
    public int LeftTargetsCompleted { get; internal set; }
    public bool MidnightRunning { get; internal set; }
    public int MidnightTimesPlayed { get; set; }
    public int Multiplier { get; set; }
    public bool[] Multipliers { get; internal set; } = new bool[5];
    public bool[] MushroomTargets { get; set; } = new bool[3];
    public bool MysterySpinLit { get; set; }
    public bool RightLock { get; internal set; }
    public bool[] RipTargets { get; set; } = new bool[3];
    public int RomanValue { get; set; }
    public bool SaucerBonus { get; internal set; }
    public bool ScoreBonusLit { get; internal set; }
    public int SpinTimesPlayed { get; set; }
    public bool SuperJackpotLit { get; internal set; }
    public bool RunForLifeOn { get; internal set; }
    public int CoffinValue { get; internal set; }
    public bool HurryUpRunning { get; internal set; }
}