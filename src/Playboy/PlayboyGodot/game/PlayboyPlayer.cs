/// <summary>
/// Add player properties here. Rename the class and file if you wish
/// </summary>
public class PlayboyPlayer : PinGodPlayer
{
    public byte BonusMultiplier { get; set; } = 1;
    public byte BonusTimes { get; set; }
    public byte DropsCompleted { get; set; }
    public bool ExtraBallLit { get; set; }
    public bool GrottoComplete { get; set; }
    public byte KeyFeatureComplete { get; set; }
    public bool[] KeysCollected { get; set; }
    public bool LeftSpecialLit { get; set; }
    public bool OutlanesLit { get; set; }
    public byte PlaymateFeatureComplete { get; set; }
    public bool[] PlaymatesCollected { get; set; }
    public bool SpecialRightLit { get; set; }

    /// <summary>
    /// Advance the bonus times
    /// </summary>
    /// <param name="times"></param>
    public void AdvanceBonus(byte times)
    {
        BonusTimes += times;
    }

    public void Reset()
    {
        BonusTimes = 1;
        DropsCompleted = 0;
        GrottoComplete = false;
        KeyFeatureComplete = 0;
    }

    public void ResetKeys()
    {
        KeysCollected = new bool[] { false, false, false, false, false };
    }

    public void ResetPlaymates()
    {
        PlaymatesCollected = new bool[] { false, false, false, false, false };
    }
}
