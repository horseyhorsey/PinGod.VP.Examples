using System;

public class MotuPinGodGame : PinGodGame
{
    /// <summary>
    /// override to create our own player type for this game
    /// </summary>
    /// <param name="name"></param>
    public override void CreatePlayer(string name)
    {
        var p = new MotuPlayer() { Name = name, Points = 0 };
        Players.Add(p);

        //testing
        //SetPlayerAllModesCompleted(p);
    }

    public string[] mystery_awards = new string[] {"Adv Roton", "Adv GraySkull", "Adv Man", 
        "Adv Stratos", "Lite Special", "Lite ExtraBall", "500K", "Start Stratos", "Start GraySkull"};

    private static void SetPlayerAllModesCompleted(MotuPlayer p)
    {
        p.SetLadder("beastman", MotuLadder.Complete);
        p.SetLadder("stratos", MotuLadder.Complete);
        p.SetLadder("manatarms", MotuLadder.Complete);
        p.SetLadder("orko", MotuLadder.Complete);
        p.SetLadder("grayskull", MotuLadder.Complete);
        p.SetLadder("skeletor", MotuLadder.Complete);
        p.SetLadder("ripper", MotuLadder.Complete);
        p.SetLadder("roton", MotuLadder.Complete);
        p.SetLadder("adam", MotuLadder.Complete);
        //p.SetLadder("motu", MotuLadder.PartComplete);
    }

    public MotuPlayer CurrentPlayer() => Player as MotuPlayer;

    /// <summary>
    /// Adds double scoring, fast scoring if enabled
    /// </summary>
    /// <param name="points"></param>
    /// <param name="emitUpdateSignal"></param>
    public override long AddPoints(long points, bool emitUpdateSignal = true)
    {
        var p = CurrentPlayer();        
        //Orko mode starts fast scoring. Make every switch at least 10K
        if(p?.IsFastScoringRunning ?? false && points < MotuConstant._10K)
        {
            points = MotuConstant._10K;
        }
        
        //double the score
        if (p?.IsAdamDoubleScoring ?? false)
            points = points * 2;

        //add 10% bonus
        AddBonus(points / 10);

        return base.AddPoints(points, emitUpdateSignal);
    }

    public void SkellyTargetBankController()
    {
        if (CurrentPlayer().IsRotonReady)
        {
            SkellyTargetsDown(true);
        }
        else
        {
            SkellyTargetsDown(false);
        }
    }

    /// <summary>
    /// Opens/Closes the Roton Drop Targets. False = targets up, True for down
    /// </summary>
    /// <param name="enabled"></param>
    public void SkellyTargetsDown(bool enabled)
    {
        LogDebug("Attempting to Drop Target Bank");
        if (enabled)
        {
            LogInfo("Opening skeletor - Targets going down");

            SolenoidOn("roton", 1);
            if (!SwitchOn("bank_down"))
            {
                LogInfo("Bank down not enabled pulsing");
                SolenoidPulse("skelly_targets");
            }
        }
        else
        {
            LogInfo("closing skeletor - Targets going up");
            SolenoidOn("roton", 0);

            if (!SwitchOn("bank_up"))
            {
                LogInfo("Bank is down");
                SolenoidPulse("skelly_targets");
            }
        }
    }

    /// <summary>
    /// Awards th player combo and resets it
    /// </summary>
    /// <returns>the points awarded</returns>
    internal long AwardCombo()
    {
        var points = CurrentPlayer().IHavePowerComboAmount;
        AddPoints(points);
        LogInfo("combo awarded " + points);
        CurrentPlayer().ResetCombo();
        return points;
    }

    public void PlayMusicForShotCount(int shotCnt)
    {
        if (shotCnt > 5 && AudioManager.CurrentMusic != "skele_loop_1")
        {
            PlayMusic("skele_loop_1");
        }
        else if (shotCnt == 4)
        {
            PlayMusic("skele_loop_2");
        }
        else if (shotCnt == 2)
        {
            PlayMusic("skele_loop_4");
        }
        else if (shotCnt == 0)
        {
            PlayMusic("skele_loop_3");
        }
    }
}