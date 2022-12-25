/// <summary>
/// 
/// </summary>
public interface IPlayer
{
    /// <summary>
    /// 
    /// </summary>
    string Initials { get; set; }
    /// <summary>
    /// 
    /// </summary>
    string Name { get; set; }
    /// <summary>
    /// 
    /// </summary>
    long Points { get; set; }
    /// <summary>
    /// 
    /// </summary>
    long Bonus { get; set; }
    /// <summary>
    /// 
    /// </summary>
    int BonusMultiplier { get; set; }
    /// <summary>
    /// 
    /// </summary>
    int ExtraBalls { get; set; }
    /// <summary>
    /// 
    /// </summary>
    int ExtraBallsAwarded { get; set; }
    /// <summary>
    /// 
    /// </summary>
    int ExtraBallsReady { get; set; }
}