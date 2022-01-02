/// <summary>
/// Add player properties here. Rename the class and file if you wish
/// </summary>
public class PlayboyPlayer : PinGodPlayer
{
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
    /// retained ball to ball
    /// </summary>
    public bool SuperBonus { get; set; }

    /// <summary>
    /// Advance the bonus times
    /// </summary>
    /// <param name="times"></param>
    public void AdvanceBonus(byte times)
    {
        BonusTimes += times;
        if (!SuperBonus && BonusTimes >= 20)
            SuperBonus = true;
    }

    public void Reset()
    {
        BonusTimes = 1;        
        GrottoComplete = false;
        KeyFeatureComplete = 0;
        SuperBonus = false;
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
