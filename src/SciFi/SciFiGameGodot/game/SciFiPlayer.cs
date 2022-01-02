public enum GameOption
{
    Off,
    Complete,
    Ready,    
}

public class SciFiPlayer : PinGodPlayer
{    
    public int AlienBonus { get; private set; }
    public int DefenderBonus { get; private set; }

    /// <summary>
    /// Light 50,000 pts, 1 = Light Special, 2 = Extra Ball..
    /// </summary>
    public int SciFiAwardLevel { get; set; }
    public bool[] Bank3 { get; private set; }
    public bool[] Bank4 { get; private set; }
    public bool[] Scifi { get; private set; }
    public bool InvasionReady { get; internal set; }
    public bool AlienBaneReady { get; internal set; }
    public bool ArmadaReady { get; internal set; }
    public bool DockEnabled { get; internal set; }
    public GameOption Dock50K { get; internal set; }
    public GameOption DockSpecial { get; internal set; }
    public GameOption DockExtraBall { get; internal set; }
    public GameOption LeftPowerUpLit { get; internal set; }
    public GameOption SpawnEnabled { get; set; }
    public GameOption InvasionEnabled { get; set; }
    /// <summary>
    /// Alien Bane lights everything and enables the Multi-ball lock
    /// </summary>
    public GameOption AlienBaneEnabled { get; set; }
    public GameOption ArmadaEnabled { get; set; }    
    public bool LeftLockLit { get; internal set; }    

    const byte MaxBonusMultiplier = 6;

    public SciFiPlayer()
    {
        ResetBanks();
        ResetSciFi();
    }

    internal void AddAlienBonus(int value)
    {
        AlienBonus += value;
        AlienBonus = AlienBonus > 20 ? 20 : AlienBonus;
    }

    internal void ResetNewBall()
    {
        SciFiAwardLevel = 0;
        BonusMultiplier = 1;
        ResetBonus();
        ResetBanks();
    }

    internal void AddDefenderBonus(int value)
    {
        DefenderBonus += value;
        DefenderBonus = DefenderBonus > 20 ? 20 : DefenderBonus;
    }

    public bool BanksCompleted()
    {
        for (int i = 0; i < Bank3.Length; i++)
        {
            if (!Bank3[i])
                return false;
        }

        for (int i = 0; i < Bank4.Length; i++)
        {
            if (!Bank4[i])
                return false;
        }

        return true;
    }

    public bool IsSciFiTargetsComplete()
    {
        for (int i = 0; i < Scifi.Length; i++)
        {
            if (!Scifi[i])
                return false;
        }

        return true;
    }

    public void ResetBanks()
    {
        this.Bank3 = new bool[3] { false, false, false };
        this.Bank4 = new bool[4] { false, false, false, false };        
    }

    public bool AddBonusMultiplier()
    {
        if(BonusMultiplier < MaxBonusMultiplier)
        {
            BonusMultiplier++;
            return true;
        }

        return false;
    }

    public void ResetSciFi()
    {
        this.Scifi = new bool[5] { false, false, false, false, false };
    }

    internal void ResetBonus()
    {
        AlienBonus = 0; 
        DefenderBonus = 0;
        SciFiAwardLevel = 0;
        ExtraBallsAwarded = 0;
    }
}
