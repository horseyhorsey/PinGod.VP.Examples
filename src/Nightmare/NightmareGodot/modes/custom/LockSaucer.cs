using Godot;
using System.Linq;

/// <summary>
/// Middle lock saucer. Awards can be multiple and then displayed in order delayed
/// </summary>
public class LockSaucer : Control
{
    int awardIndex = 0;
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
                    AwardSuperJackpot(); //award this first because the jackpot turns this on if ready
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

        pinGod.SolenoidOn("vpcoil", 0);
        game.UpdateLamps();
        pinGod.LogInfo("lock saucer coil exit");
    }

    private void AwardBallLock()
    {
        if (player.BallsLocked < 3 && !player.BallLockEnabled)
        {
            player.BallsLocked++;
            pinGod.LogInfo("AwardBallLock-BallsLocked:", player.BallsLocked);
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
                    game.OnDisplayMessage("BALL LOCK\nENABLED", 4.55f);
                    game.resumeBgmTimer.Stop();
                    game.PlayThenResumeMusic("mus_balllockenabled", 4.55f);
                    awardTimer.Start(4.55f);
                    UpdateLamps();
                    pinGod.LogInfo("ball lock enabled, jackpot ready");

                    pinGod.SolenoidOn("vpcoil", 3); //lampshow vp
                    return;
                default:
                    break;
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

            game.OnDisplayMessage("EXTRA\nBALL", 5.6f);

            game.PlayThenResumeMusic("mus_extraball", 5.6f);
            awardTimer.Start(2.4f);

            pinGod.SolenoidOn("vpcoil", 8); // lampshow vp
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
            
            var bonus = player.Bonus;            
            pinGod.AddPoints((int)bonus);            
            pinGod.LogInfo("scoring bonus from saucer:" + bonus);

            var frames = PinGodHelper.CreateBonusZeroCountdown(bonus, false);
            game.StartBonusDisplay();
            awardTimer.Start(frames.Count() * 0.1f);

            pinGod.SolenoidOn("vpcoil", 6); //lampshow vp
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
            player.JackpotLit = false;

            //add points and reset
            var jackpotTotal = player.JackpotValue * player.Multiplier;
            pinGod.AddPoints(jackpotTotal);
            game.OnDisplayMessage($"JACKPOT {player.Multiplier} X {player.JackpotValue.ToScoreString()}\n {jackpotTotal.ToScoreString()}", 5f);

            game.resumeBgmTimer.Stop();
            game.PlayThenResumeMusic("mus_jackpot", 5f);

            player.ResetJackpotValue();
            if (player.BallLockEnabled)
            {
                //todo: enable super jackpot for a few seconds
                game.DisableBallLocks(true); 
            }                

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
            player.SuperJackpotLit = false;
            var superTotal = player.JackpotValue * player.Multiplier * 2;
            pinGod.AddPoints(superTotal);
            pinGod.LogInfo("awarded super jackpot, ", superTotal);
            awardTimer.Start(3.5f);
            game.OnDisplayMessage("SUPER JACKPOT\n" + superTotal.ToScoreString());


            game.resumeBgmTimer.Stop();
            game.PlayThenResumeMusic("mus_jackpot", 4f);

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
        //todo: check this is right
        player.BallLockEnabled = false;
        player.RightLock = false;
        player.LeftLock = false;
        player.SuperJackpotLit = false;
        player.BallsLocked = 0;
    }

    private void ProcessSwitch()
    {
        pinGod.SolenoidPulse("flasher_top_left");
        //todo: lampshow
        awardTimer.Start(0.1f);        
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

        if(player.ExtraBalls > 0 && !pinGod.BallSaveActive)
            pinGod.SetLampState("shoot_again", 1);
    }
}
