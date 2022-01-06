public interface IPlayer
{
    string Initials { get; set; }
    string Name { get; set; }
    long Points { get; set; }
    long Bonus { get; set; }
    int BonusMultiplier { get; set; }
    int ExtraBalls { get; set; }
    int ExtraBallsAwarded { get; set; }
    int ExtraBallsReady { get; set; }
}