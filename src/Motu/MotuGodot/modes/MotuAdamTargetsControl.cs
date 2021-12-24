using System;

public class MotuAdamTargetsControl : PinballTargetsControl
{
    private MotuPinGodGame _game;
    private AdamTargetsMode _adamTargetsMode;

    public override void _EnterTree()
    {
        base._EnterTree();

        _game = (pinGod as MotuPinGodGame);
    }

    /// <summary>
    /// Get the players
    /// </summary>
    public override void _Ready()
    {
        base._Ready();

        _adamTargetsMode = GetParent<AdamTargetsMode>();
    }

    void OnBallStarted()
    {
        var player = _game?.CurrentPlayer();
        _targetValues = player.AdamTargets;
        pinGod.LogInfo("adam loaded player targets");        
    }

    void OnBallDrained()
    {
        var player = _game?.CurrentPlayer();
        player.AdamTargets = _targetValues;
        pinGod.LogInfo("adam saved player targets");
    }

    /// <summary>
    /// Don't process switch if Ripper multi-ball running
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public override bool SetTargetComplete(int index)
    {
        var player = _game?.CurrentPlayer();
        if(player != null)
        {
            if (!player.IsRipperMultiballRunning && !player.IsSorceressRunning)
            {
                if (player.TeelaCount >= 1)
                {
                    AddAdamTargetsBonusRound(player);
                    player.TeelaCount = 0;
                    return true;
                }

                bool result =  base.SetTargetComplete(index);
                if (result)
                {
                    player.SetLadder("adam", MotuLadder.PartComplete);
                }
                return result;
            }            
        }
        
        return false;
    }

    public override void TargetsCompleted(bool reset = true)
    {
        base.TargetsCompleted(reset);

        AddAdamTargetsBonusRound(_game.CurrentPlayer(), 1);
    }

    private void AddAdamTargetsBonusRound(MotuPlayer player, int numOfModesToStart = 1)
    {
        //todo: play seq AdamTargets
        player.SetLadder("adam", MotuLadder.Complete);
        pinGod.LogInfo($"starting ({numOfModesToStart}) adam bonus rounds");
        for (int i = 0; i < numOfModesToStart; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                if (!player.AdamModesRunning[ii])
                {
                    player.AdamModesRunning[ii] = true;
                    _adamTargetsMode.StartAdamBonusRound(player, ii);
                    break;
                }
            }
        }

        ResetTargets();
    }

    public override void UpdateLamps()
    {
        base.UpdateLamps();
        var player = _game.CurrentPlayer();
        //adam target bonuses
        if (player.IsAdamDoubleScoring) pinGod.SetLampState("adam_double", 2);
        else pinGod.SetLampState("adam_double", 0);
        if (player.IsAdamSuperBumperRunning) pinGod.SetLampState("adam_bump", 2);
        else pinGod.SetLampState("adam_bump", 0);
        if (player.IsAdamSuperSpinnersRunning) pinGod.SetLampState("adam_spin", 2);
        else pinGod.SetLampState("adam_spin", 0);
    }
}