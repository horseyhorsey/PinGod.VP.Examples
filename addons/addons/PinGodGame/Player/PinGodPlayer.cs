/// <summary>
/// PinGodPlayer to hold standard pinball variables
/// </summary>
public class PinGodPlayer : IPlayer
{
    public string Initials { get; set; } = "AAA";
    public string Name { get; set; } = "Android AI";
    public long Points { get; set; } = 0;
    public long Bonus { get; set; } = 0;
    public int BonusMultiplier { get; set; } = 1;    
    public int ExtraBalls { get; set; }
    public int ExtraBallsAwarded { get; set; }
    public int ExtraBallsReady { get; set; }
}