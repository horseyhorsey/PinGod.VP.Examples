using Godot;

/// <summary>
/// Middle lock saucer. Awards can be multiple and then displayed in order delayed
/// </summary>
public class LockSaucer : Control
{
    int awardIndex = 0;
    float awardDelay = 0;
    private BallStackPinball ballStack;
    private Game game;
    private Timer awardTimer;
    private PinGodGame pinGod;
    private NightmarePlayer player;

    public override void _EnterTree()
    {
        ballStack = GetNode("BallStackPinball") as BallStackPinball;
        pinGod = GetNode("/root/PinGodGame") as PinGodGame;
        game = GetParent().GetParent() as Game;
        awardTimer = GetNode<Timer>("AwardTimer");
    }

    public void AwardTimer_timeout()
    {
        if (awardIndex < 4)
        {
            switch (awardIndex)
            {
                case 0:
                    AwardScoreBonus();
                    break;
                case 1:
                    AwardSuperJackpot();
                    break;
                case 2:
                    AwardJackpot();
                    break;
                case 3:
                    //advance ball locks - INIT - todo: different exit times for kicker
                    AwardBallLock();
                    break;
                case 4:
                    AwardExtraBall();
                    break;
                default:
                    break;
            }

            awardIndex++;
        }
        else
        {
            pinGod.LogInfo("awards ended");
            ballStack.Start(0.2f);
            awardIndex = 0;
        }        
    }

    private void _on_BallStackPinball_SwitchActive()
    {
        pinGod.LogInfo("lock saucer active");

        if (!pinGod.GameInPlay || pinGod.IsTilted)
        {
            ballStack.Start(0.3f);
            return;
        }

        ProcessSwitch();
    }
    private void _on_BallStackPinball_SwitchInActive()
    {
        // Replace with function body.
    }

    private void _on_BallStackPinball_timeout()
    {
        ballStack.SolenoidPulse();
        pinGod.PlaySfx("snd_kicker");
        //pinGod.PlaySfx("snd_lowsquirk");
        if (player.SuperJackpotLit)
        {
            //todo: disable super jackpot after a timer 4 seconds
        }
        
        game.UpdateLamps();
    }

    private void AwardBallLock()
    {
        if (player.BallsLocked < 3 && !player.BallLockEnabled)
        {
            player.BallsLocked++;
            pinGod.LogInfo("adding ball to lock,", player.BallsLocked);
            switch (player.BallsLocked)
            {
                case 1:
                    game.OnDisplayMessage("INIT LOCKS");
                    break;
                case 2:
                    game.OnDisplayMessage("LOCK OPEN");                    
                    break;
                case 3:
                    player.BallLockEnabled = true;
                    player.JackpotLit = true;
                    awardTimer.Start(2.5f);
                    game.OnDisplayMessage("BALL LOCK\nENABLED");
                    game.PlayThenResumeMusic("mus_balllockenabled", 4.55f);
                    return;
                default:
                    break;                    
                    //todo: delay music kickout 1.6
            }            
        }

        awardTimer.Start(0.1f);        
    }

    private void AwardExtraBall()
    {
        if (player.ExtraBallLit)
        {
            pinGod.LogInfo("awarding extra ball");
            player.ExtraBallLit = false;
            player.ExtraBalls++;
            game.OnDisplayMessage("EXTRA\nBALL");
            awardTimer.Start(1.6f);
            pinGod.PlayMusic("mus_extraball");
            //todo: delay music kickout 1.6
        }
        else
        {
            awardTimer.Start(0.05f);
        }
    }

    private void AwardScoreBonus()
    {
        if (player.ScoreBonusLit)
        {
            player.ScoreBonusLit = false;
            game.PauseBgm(true);
            var bonus = player.Bonus;            
            pinGod.AddPoints((int)bonus);            
            pinGod.LogInfo("scoring bonus from saucer:" + bonus); 
            game.StartBonusDisplay();
            awardTimer.Start(2f);//todo: correct time
        }
        else
        {
            awardTimer.Start(0.05f);
        }
    }

    private void AwardJackpot()
    {
        if (player.JackpotLit)
        {
            game.PauseBgm();
            player.JackpotLit = false;

            //add points and reset
            pinGod.AddPoints(player.JackpotValue);
            game.OnDisplayMessage("SUPER JACKPOT\n" + player.JackpotValue.ToScoreString());
            player.ResetJackpotValue();

            if (player.BallLockEnabled) 
                game.DisableBallLocks(true); //enable super jackpot

            awardTimer.Start(4f);
        }
        else
        {
            awardTimer.Start(0.05f);
        }
    }

    private void AwardSuperJackpot()
    {
        if (player.SuperJackpotLit)
        {
            game.PauseBgm(true);
            player.SuperJackpotLit = false;
            var superTotal = player.JackpotValue * 2;
            pinGod.AddPoints(superTotal);
            pinGod.LogInfo("awarded super jackpot, ", superTotal);
            awardTimer.Start(3.5f);
            game.OnDisplayMessage("SUPER JACKPOT\n" + superTotal.ToScoreString());
            player.ResetJackpotValue();

            //Disable locks if we have a ball already locked
            if (player.BallLockEnabled)
            {
                if (player.LeftLock || player.RightLock)
                    player.DisableBallLocks(true);
            }
        }
        else
        {
            awardTimer.Start(0.05f);
        }
    }

    private void OnBallStarted()
    {
        player = ((NightmarePinGodGame)pinGod).GetPlayer();
        player.BallLockEnabled = false;
        player.RightLock = false;
        player.LeftLock = false;
        player.SuperJackpotLit = false;
        player.BallsLocked = 0;
    }

    private void ProcessSwitch()
    {
        pinGod.SolenoidPulse("flasher_top_left");
        float delay = 0.0f;

        //AwardExtraBall();

        awardTimer.Start(0.1f);

        //UpdateLamps();
        //start a timer to exit the saucer
        //ballStack.Start(delay);
    }
    /// <summary>
    /// Updates lamps pointing to this saucer
    /// </summary>
    private void UpdateLamps()
    {
        if (player.ScoreBonusLit) pinGod.SetLampState("top_score_b", 2);
        else pinGod.SetLampState("top_score_b", 0);

        if (player.SuperJackpotLit) pinGod.SetLampState("top_super", 2);
        else pinGod.SetLampState("top_super", 0);

        if (player.JackpotLit) pinGod.SetLampState("top_jackpot", 2);
        else pinGod.SetLampState("top_jackpot", 0);

        if (player.ExtraBallLit) pinGod.SetLampState("top_xtraball", 2);
        else pinGod.SetLampState("top_xtraball", 0);

        if (player.BallsLocked == 1)
            pinGod.SetLampState("top_init", 2);
        else if (player.BallsLocked == 2)
            pinGod.SetLampState("top_init", 1);
        else if (player.BallsLocked == 3)
            pinGod.SetLampState("top_init", 0);
    }
}
