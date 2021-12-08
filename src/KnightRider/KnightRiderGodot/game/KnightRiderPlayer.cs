/// <summary>
/// Add player properties here. Rename the class and file if you wish
/// </summary>
public class KnightRiderPlayer : PinGodPlayer
{
    public bool BillionShotLit { get; internal set; }
    public bool IsKarrRunning { get; internal set; }
    public bool IsKittRunning { get; internal set; }
    public bool IsSuperPursuitMode { get; internal set; }
    public bool IsTruckRampReady { get; internal set; }
    public bool IsVideoModeLit { get; internal set; }
    /// <summary>
    /// Yellow targets
    /// </summary>
    public byte[] KarrActiveTargets { get; set; } = new byte[3] { 2, 2, 2 };
    public bool KarrCompleteReady { get; set; }
    public byte[] KarrTargets { get; internal set; } = new byte[5] { 2, 2, 2, 2, 2 };
    /// <summary>
    /// Green targets
    /// </summary>
    public byte[] KittActiveTargets { get; set; } = new byte[3] { 2, 2, 2 };
    public int KittCompleteCount { get; internal set; }
    public bool KittCompleteReady { get; internal set; }
    public byte[] KittTargets { get; internal set; } = new byte[4];
    public bool PursuitCompleteReady { get; private set; }
    public bool[] SpecialLanes { get; set; } = new bool[2];
    /// <summary>
    /// Red targets
    /// </summary>
    public byte[] TruckActiveTargets { get; set; } = new byte[3] { 2, 2, 2 };
    public byte[] TruckLocks { get; set; } = new byte[3];
    public bool TruckCompleteReady { get; internal set; }
    public int KarrCompleteCount { get; internal set; }
    public bool ExtraBallLit { get; internal set; }
    public int TruckCompleteCount { get; internal set; }
    public int JackpotAdded { get; internal set; }

    /// <summary>
    /// Gets whether modes complete (ready) or billion shot is ready
    /// </summary>
    /// <returns></returns>
    internal bool AllModesComplete()
    {
        if (BillionShotLit) return true;
        if (KittCompleteReady && KarrCompleteReady && PursuitCompleteReady) return true;
        return false;
    }

    internal void ResetCompletedModes()
    {
        BillionShotLit = false;
        KarrCompleteReady = false; KittCompleteReady = false; TruckCompleteReady = false;
        TruckLocks = new byte[3];
        ResetKarrActive();
        ResetKittActive();
        ResetTruckActive();
    }

    /// <summary>
    /// Resets the lane targets and the upper play field targets
    /// </summary>
    internal void ResetKarrActive()
    {
        KarrTargets = new byte[5] { 2, 2, 2, 2, 2 };
        KarrActiveTargets = new byte[3] { 2, 2, 2 };
        IsKarrRunning = false;
    }

    internal bool AnyModeIsRunning() => IsKittRunning || IsKarrRunning;

    internal void ResetKittActive()
    {
        KittActiveTargets = new byte[3] { 2, 2, 2 };
        IsKittRunning = false;
    }

    internal void ResetTruckActive()
    {
        TruckActiveTargets = new byte[3] { 2, 2, 2 };
        IsSuperPursuitMode = false;
        TruckLocks = new byte[3];
    }
}
