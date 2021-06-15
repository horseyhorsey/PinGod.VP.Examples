/// <summary>
/// PinGodPlayer to hold standard pinball variables
/// </summary>
public abstract class PinGodPlayer : IPlayer
{
    public string Initials { get; set; } = "AAA";
    public string Name { get; set; } = "Android AI";
    public long Points { get; set; } = 0;
    public long Bonus { get; set; } = 0;
    public int ExtraBalls { get; set; }
}