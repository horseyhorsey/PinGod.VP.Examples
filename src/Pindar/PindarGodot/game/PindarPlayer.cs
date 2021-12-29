using System;
using System.Collections.Generic;

/// <summary>
/// Add player properties here. Rename the class and file if you wish
/// </summary>
public class PindarPlayer : PinGodPlayer
{
    /// <summary>
    /// Bonus advance. B advances 3 bonus
    /// </summary>
    public readonly Dictionary<string, int> AERollovers = new Dictionary<string, int>()
    {
        { "A", 0 }, {"B", 0 }, {"C", 0 }, {"D", 0 }, {"E", 0 }
    };

    /// <summary>
    /// Pindar drops
    /// </summary>
    public readonly Dictionary<string, int> Dar = new Dictionary<string, int>()
    {
        { "D", 0 }, {"A", 0 }, {"R", 0 }
    };

    /// <summary>
    /// Pindar drops
    /// </summary>
    public readonly Dictionary<string, int> Pin = new Dictionary<string, int>()
    {
        { "P", 0 }, {"I", 0 }, {"N", 0 }
    };

    /// <summary>
    /// Pindar drops
    /// </summary>
    public Dictionary<string, int> SpotTargets = new Dictionary<string, int>()
    {
        { "1", 0 }, {"2", 0 }, {"3", 0 }, {"4", 0 }
    };

    /// <summary>
    /// The current arrow for moving shots
    /// </summary>
    public int SpotTargetArrowNumber { get; set; } = 1;

    public int BonusCount { get; set; } = 1;

    public int BonusMultiplier { get; set; }=1;

    public int DarsCompleted { get; set; }

    /// <summary>
    /// Max out so player cannot have more than 1 ball per game
    /// </summary>
    public int ExtraBallsAwardedThisBall { get; set; }
    public bool IsBumperLit { get; internal set; }

    public bool IsSpinnerLit { get; private set; }

    public MagnetValue MagnetValue { get; set; }

    public int PinsCompleted { get; set; }

    public int SpotTargetsCompleteCount { get; set; }

    public int PintargetsCompleteCount { get; private set; }

    public int EjectHoleValue { get; internal set; } = Constant._1K;

    public int PinDarCompleted { get; internal set; }

    /// <summary>
    /// Advance bonus max 29
    /// </summary>
    /// <param name="val"></param>
    public void AdvanceBonus(int val)
    {
        BonusCount += val;
        if (BonusCount > 29)
            BonusCount = 29;

        IsSpinnerLit = BonusCount > 10 && BonusCount < 20;
    }

    public void ResetBonusCount() => BonusCount = 1;

    internal bool AdvanceDar(string swName)
    {
        Dar[swName] = 1;
        if (!Dar.ContainsValue(0))
        {
            DarsCompleted++;
            ResetDar();
            Dar[swName] = 1; //set the last target complete after reset as this stays down in game
            return true;
        }

        return false;
    }

    internal void AdvanceMagnetPitValue()
    {
        if (MagnetValue < MagnetValue.magnet_50k)
        {
            foreach (MagnetValue item in Enum.GetValues(typeof(MagnetValue)))
            {
                if (MagnetValue < item)
                {
                    MagnetValue = item;
                    break;
                }
            }
        }
    }

    internal bool AdvancePin(string swName)
    {
        Pin[swName] = 1;
        if (!Pin.ContainsValue(0))
        {
            PinsCompleted++;
            ResetPin();
            Pin[swName] = 1; //set the last target complete after reset as this stays down in game
            return true;
        }

        return false;
    }

    internal void ResetTopLanes()
    {
        AERollovers["A"] = 0;
        AERollovers["B"] = 0;
        AERollovers["C"] = 0;
        BonusMultiplier++;
    }

    internal bool SetLaneComplete(string swName, int value)
    {
        AERollovers[swName] = value;
        return TopLanesComplete();
    }

    internal bool TopLanesComplete()
    {
        if (AERollovers["A"] > 0 && AERollovers["B"] > 0 && AERollovers["C"] > 0)
        {
            ResetTopLanes();
            return true;
        }

        return false;
    }
    private void ResetDar()
    {
        Dar["D"] = 0;
        Dar["A"] = 0;
        Dar["R"] = 0;
    }

    private void ResetPin()
    {
        Pin["P"] = 0;
        Pin["I"] = 0;
        Pin["N"] = 0;
    }

    internal bool AreSpotTargetsComplete() => !SpotTargets.ContainsValue(0);

    internal void AdvanceMultiplier()
    {
        BonusMultiplier++;
        if (BonusMultiplier > 3)
            BonusMultiplier = 3;
    }

    internal void ResetSpotTargets()
    {
        SpotTargets = new Dictionary<string, int>()
        { 
            { "1", 0 }, {"2", 0 }, {"3", 0 }, {"4", 0 }
        };
    }

    internal void ResetNewBall()
    {
        BonusMultiplier = 1;
        PinsCompleted = 0;
        DarsCompleted = 0;
        SpotTargetsCompleteCount = 0;
        BonusCount = 1;
        MagnetValue = MagnetValue.magnet_1k;
        EjectHoleValue = 0;
    }

    internal void AdvanceEjectHoleValue()
    {
        if (EjectHoleValue < Constant._10K)
            EjectHoleValue = Constant._10K;
        else if (EjectHoleValue < Constant._15K)
            EjectHoleValue = Constant._15K;
        else if (EjectHoleValue < Constant._15K)
        {
            EjectHoleValue = 369; //extra ball
        }
    }
}
