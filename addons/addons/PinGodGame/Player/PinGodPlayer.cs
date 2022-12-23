/// <summary>
/// PinGodPlayer to hold standard pinball variables
/// </summary>
public class PinGodPlayer : IPlayer
{
    /// <summary>
    /// 
    /// </summary>
    public string Initials { get; set; } = "AAA";
    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; } = "Android AI";
    /// <summary>
    /// 
    /// </summary>
    public long Points { get; set; } = 0;
    /// <summary>
    /// 
    /// </summary>
    public long Bonus { get; set; } = 0;
    /// <summary>
    /// 
    /// </summary>
    public int BonusMultiplier { get; set; } = 1;    
    /// <summary>
    /// 
    /// </summary>
    public int ExtraBalls { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int ExtraBallsAwarded { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int ExtraBallsReady { get; set; }
}