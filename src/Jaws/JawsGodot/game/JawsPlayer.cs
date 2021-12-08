using System;
using System.Linq;

public class JawsPlayer : PinGodPlayer
{
    #region Properties
    public long BarrelMballBest { get; set; }
    public bool BarrelMballComplete { get; set; }
    public int BarrelMballCompleteCount { get; set; }
    public long BarrelMballCurrentScore { get; set; }
    public bool BarrelsOn { get; set; }
    public int BarrelStage { get; set; }
    public bool[] BarrelTargets { get; private set; }
    public int BarrelTargetsComplete { get; set; }
    public bool BarrelTargetsUp { get; internal set; }
    public long BruceMballBest { get; set; }
    public int BruceMballCompleteCount { get; set; }
    public bool BruceMballComplete { get; set; }
    public long BruceMballCurrentScore { get; set; }
    public int BonusBarrel { get; set; }
    public int BonusBruce { get; set; }
    public int BonusSkipper { get; set; }
    public bool[] DropTargets { get; private set; }
    public int OrcaCount { get; set; }
    public bool OrcaLocksActive { get; set; }
    public long OrcaMballBest { get; set; }
    public bool OrcaMballComplete { get; set; }
    public int OrcaMballCompleteCount { get; set; }
    public long OrcaMballCurrentScore { get; set; }
    public bool[] QuintActivated { get; private set; }
    public int QuintCount { get; set; }
    public bool QuintLockEnabled { get; set; }
    public int QuintLocksComplete { get; set; }
    public bool SharkLockEnabled { get; set; }
    public int SharkLocksComplete { get; set; }
    #endregion

    public JawsPlayer()
    {
        ResetBarrelTargets();
        ResetDropTargets();
        QuintActivated = new bool[3];
        //QuintActivated = new bool[3] { true, true, true } ; //testing
    }

    public void ResetBarrelTargets() => BarrelTargets = new bool[4];
    /// <summary>
    /// Sets all to true
    /// </summary>
    public void ResetDropTargets() => DropTargets = new bool[6] { true, true, true, true, true, true };

    internal bool IsActivatorComplete()
    {
        return QuintActivated.All(x => x == true);
    }

    /// <summary>
    /// Resets all bruce variables
    /// </summary>
    internal void ResetBruce()
    {
        SharkLocksComplete = 0;
        SharkLockEnabled = false;
        QuintLockEnabled = false;
        QuintLocksComplete = 0;
        QuintActivated = new bool[3];
        BruceMballCurrentScore = 0;
        BruceMballComplete = true;
    }

    internal void ResetNewBall()
    {
        this.ResetDropTargets();
        this.BarrelTargetsUp = false;
        this.BarrelsOn = false;
    }

    internal void ResetOrca()
    {
        OrcaMballCurrentScore = 0;
        OrcaCount = 0;
        OrcaLocksActive = false;       
    }
}
