using Godot;
using System;

public class BaseMode : Control
{
    private PackedScene _ballSaveScene;
    private BallStackPinball _ballStackSaucer;
    private Timer _magentTimer;
    private Timer _spotTargetTimer;
    private PindarPlayer _player;
    [Export] string BALL_SAVE_SCENE = "res://addons/PinGodGame/Modes/ballsave/BallSave.tscn";

    private Game game;
    private PinGodGame pinGod;
    private PindarScoreMode _scoreMode;

    public override void _EnterTree()
    {
        pinGod = GetNode("/root/PinGodGame") as PinGodGame;
        game = GetParent().GetParent() as Game;

        _ballSaveScene = GD.Load<PackedScene>(BALL_SAVE_SCENE);
        _ballStackSaucer = GetNode<BallStackPinball>(nameof(BallStackPinball));

        _magentTimer = GetNode<Timer>("MagnetTimer");
        _spotTargetTimer = GetNode<Timer>("SpotTargetTimer");

        _scoreMode = GetParent().GetNode<PindarScoreMode>("ScoreMode");
    }

    /// <summary>
    /// Basic input switch handling
    /// </summary>
    /// <param name="event"></param>
    public override void _Input(InputEvent @event)
    {
        if (!pinGod.GameInPlay) return;
        if (pinGod.IsTilted) return; //game is tilted, don't process other switches when tilted

        if (pinGod.SwitchOn("start", @event))
        {
            pinGod.LogInfo("base: start button adding player...", pinGod.StartGame());
        }

        if (pinGod.SwitchOn("flipper_l", @event))
        {
        }

        if (pinGod.SwitchOn("flipper_r", @event))
        {
        }

        if (pinGod.SwitchOn("outlane_l", @event))
        {
            pinGod.AddPoints(Constant._5K);
            if (_player.DarsCompleted > 3)
            {
                //special
                pinGod.AddPoints(Constant._30K);
            }

            pinGod.PlaySfx("bonus3");

            if (!pinGod.BallSaveActive)
                pinGod.PlayMusic("pindar-5");
        }

        if (pinGod.SwitchOn("outlane_r", @event))
        {
            pinGod.AddPoints(Constant._5K);
            if (_player.DarsCompleted > 3)
            {
                //special
                pinGod.AddPoints(Constant._30K);
            }

            pinGod.PlaySfx("bonus3");

            if (!pinGod.BallSaveActive)
                pinGod.PlayMusic("pindar-5");
        }

        if (pinGod.SwitchOn("sling_l", @event))
        {
            pinGod.AddPoints(50);
        }

        if (pinGod.SwitchOn("sling_r", @event))
        {
            pinGod.AddPoints(50);
        }

        if (pinGod.SwitchOn("bumper_bottom", @event))
        {
            if (_player.PinsCompleted > 2)
                pinGod.AddPoints(1000);
            else
                pinGod.AddPoints(100);

            pinGod.PlaySfx("bonus_stab", "Voice");
        }

        if (pinGod.SwitchOn("bumper_left", @event))
        {
            if (_player.PinsCompleted > 0)
                pinGod.AddPoints(1000);
            else
                pinGod.AddPoints(100);

            pinGod.PlaySfx("bonus_stab", "Voice");
        }

        if (pinGod.SwitchOn("bumper_top", @event))
        {
            if (_player.PinsCompleted > 1)
                pinGod.AddPoints(1000);
            else
                pinGod.AddPoints(100);

            pinGod.PlaySfx("bonus_stab", "Voice");
        }

        if (pinGod.SwitchOn("spinner", @event))
        {
            if (_player.IsSpinnerLit)
                pinGod.AddPoints(1000);
            else
                pinGod.AddPoints(100);
        }

        if (pinGod.SwitchOn("magnet", @event))
        {
            pinGod.AddPoints((int)_player.MagnetValue);

            pinGod.SolenoidOn("magnet", 1);
            pinGod.SolenoidOn("magnet_flash", 1);
            pinGod.PlayVoice("megotyou", "Voice");

            _scoreMode.ChangeSnakeAnimation("spawn_CINEMA_4D_Main", false);

            pinGod.SolenoidOn("vpcoil", 1);
            pinGod.SolenoidOn("vpcoil", 2);
            _magentTimer.Start(2);

            //reset the value?
            if (_player.MagnetValue >= MagnetValue.magnet_20k)
            {
                pinGod.DisableAllLamps();
                _player.MagnetValue = MagnetValue.magnet_1k;                
            }

            UpdateLamps();
        }

        if (pinGod.SwitchOn("A", @event))
        {
            pinGod.AddPoints(Constant._1K);
            AdvanceBonus(1);
            if (_player.SetLaneComplete("A", 1))
            {
                _player.AdvanceMultiplier();
            }
            UpdateLamps();
        }
        if (pinGod.SwitchOn("B", @event))
        {
            pinGod.AddPoints(Constant._1K);
            AdvanceBonus(3);
            if (_player.SetLaneComplete("B", 1))
            {
                _player.AdvanceMultiplier();
            }
            UpdateLamps();
        }
        if (pinGod.SwitchOn("C", @event))
        {
            pinGod.AddPoints(Constant._1K);
            AdvanceBonus(1);
            if (_player.SetLaneComplete("C", 1))
            {
                _player.AdvanceMultiplier();
            }
            UpdateLamps();
        }

        if (pinGod.SwitchOn("inlane_l", @event))
        {
            pinGod.AddPoints(Constant._3K);
            AdvanceBonus(1);
            if (_player.AERollovers["E"] == 1)
            {
                OnInlanesComplete();
            }
            else
                _player.AERollovers["D"] = 1;

            UpdateLamps();
        }

        if (pinGod.SwitchOn("inlane_r", @event))
        {
            pinGod.AddPoints(Constant._3K);
            AdvanceBonus();
            if (_player.AERollovers["D"] == 1)
            {
                OnInlanesComplete();
            }
            else
                _player.AERollovers["E"] = 1;

            UpdateLamps();
        }

        #region Spot targets
        if (pinGod.SwitchOn("1", @event))
        {
            AdvanceBonus(1);
            AdvanceSpotTarget("1");
            if(_player.SpotTargetsCompleteCount > 3 && _player.SpotTargetArrowNumber == 1)
            {
                
            }
            UpdateLamps();
        }
        if (pinGod.SwitchOn("2", @event))
        {
            AdvanceBonus(1);
            AdvanceSpotTarget("2");
            UpdateLamps();
        }
        if (pinGod.SwitchOn("3", @event))
        {
            AdvanceBonus(1);
            AdvanceSpotTarget("3");
            UpdateLamps();
        }
        if (pinGod.SwitchOn("4", @event))
        {
            AdvanceBonus(1);
            AdvanceSpotTarget("4");
            UpdateLamps();
        } 
        #endregion

        #region Pin Drop Targets
        if (pinGod.SwitchOn("Pin", @event))
        {
            var complete = OnPinHit("P");
            pinGod.LogInfo($"P, drops complete: {complete}, times completed: {_player.PinsCompleted}");
            UpdateLamps();
        }
        if (pinGod.SwitchOn("pIn", @event))
        {
            var complete = OnPinHit("I");
            pinGod.LogInfo($"I, drops complete: {complete}, times completed: {_player.PinsCompleted}");
            UpdateLamps();
        }
        if (pinGod.SwitchOn("piN", @event))
        {
            var complete = OnPinHit("N");
            pinGod.LogInfo($"N, drops complete: {complete}, times completed: {_player.PinsCompleted}");
            UpdateLamps();
        }
        #endregion

        #region Dar drop targets
        if (pinGod.SwitchOn("Dar", @event))
        {
            var complete = OnDarHit("D");
            pinGod.LogInfo($"D, drops complete: {complete}, times completed: {_player.DarsCompleted}");
            UpdateLamps();
        }
        if (pinGod.SwitchOn("dAr", @event))
        {
            var complete = OnDarHit("A");
            pinGod.LogInfo($"A, drops complete: {complete}, times completed: {_player.DarsCompleted}");
            UpdateLamps();
        }
        if (pinGod.SwitchOn("daR", @event))
        {
            var complete = OnDarHit("R");
            pinGod.LogInfo($"R, drops complete: {complete}, times completed: {_player.DarsCompleted}");
            UpdateLamps();
        } 
        #endregion
    }

    /// <summary>
    /// completes the inlanes, resets and advances spot targets
    /// </summary>
    private void OnInlanesComplete()
    {
        _player.AERollovers["D"] = 0;
        _player.AERollovers["E"] = 0;
        foreach (var item in _player.SpotTargets)
        {
            if (item.Value == 0)
            {
                AdvanceSpotTarget(item.Key);
                break;
            }
        }
    }

    private bool OnDarHit(string swName)
    {
        if (_player.DarsCompleted > 2) pinGod.AddPoints(Constant._5K);
        else pinGod.AddPoints(Constant._1K);

        if (_player.AdvanceDar(swName))
        {
            pinGod.SolenoidPulse("dar");
            AdvancePinDarDropsCompleted();
            pinGod.PlaySfx("bonus2");
            return true;
        }

        return false;
    }

    private bool OnPinHit(string swName)
    {
        if (_player.PinsCompleted > 2) pinGod.AddPoints(Constant._5K);
        else pinGod.AddPoints(Constant._1K);

        if (_player.AdvancePin(swName))
        {
            pinGod.SolenoidPulse("pin");
            pinGod.PlaySfx("bonus2", "Voice");
            AdvancePinDarDropsCompleted();
            _player.AdvanceMagnetPitValue();
            return true;
        }

        return false;
    }

    public void OnBallDrained() 
    {
        _spotTargetTimer.Stop();
    }

    public void OnBallSaved()
    {
        pinGod.LogDebug("base: ball_saved");
        if (!pinGod.IsMultiballRunning)
        {
            pinGod.LogDebug("ballsave: ball_saved");
            //add ball save scene to tree and remove after 2 secs;
            CallDeferred(nameof(DisplayBallSaveScene), 2f);
        }
        else
        {
            pinGod.LogDebug("skipping save display in multiball");
        }
    }

    public void OnBallStarted()
    {
        _player = (pinGod.Player as PindarPlayer);
        _player.ResetNewBall();
        (pinGod as CustomPinGodGame).PlayMusicForBonus();
        _scoreMode.ChangeSnakeAnimation("crawl_CINEMA_4D_Main", true);
    }

    public void UpdateLamps()
    {
        foreach (var item in _player.AERollovers)
        {
            pinGod.SetLampState(item.Key, (byte)item.Value);
        }

        if (_player.DarsCompleted > 0)
            pinGod.SetLampState("dar", 1);
        else pinGod.SetLampState("dar", 0);

        if (_player.PinsCompleted > 0)
            pinGod.SetLampState("pin", 1);
        else pinGod.SetLampState("pin", 0);

        //bonus countdown
        UpdateBonusLamps();

        //stand up targets. todo: blink when target complete?
        foreach (var item in _player.SpotTargets)
        {
            pinGod.SetLampState(item.Key, (byte)(item.Value > 0 ? 1 : 0));
        }

        foreach (MagnetValue item in Enum.GetValues(typeof(MagnetValue)))
        {
            if (item > MagnetValue.magnet_1k)
            {
                if (_player.MagnetValue >= item) pinGod.SetLampState(item.ToString(), 1);
                else pinGod.SetLampState(item.ToString(), 0);
            }
        }

        if(_player.EjectHoleValue == 369)
        {
            pinGod.SetLampState("eject_hole_xb", 1);
            pinGod.SetLampState("eject_hole_15k", 1);
            pinGod.SetLampState("eject_hole_10k", 1);
        }
        else if (_player.EjectHoleValue > 369)
        {
            if(_player.EjectHoleValue == Constant._10K)
                pinGod.SetLampState("eject_hole_10k", 1);
            if (_player.EjectHoleValue == Constant._15K)
            {
                pinGod.SetLampState("eject_hole_10k", 1);
                pinGod.SetLampState("eject_hole_15k", 1);
            }            
        }

        if (_player.BonusMultiplier > 1)
        {
            if (_player.BonusMultiplier == 2)
            {
                pinGod.SetLampState("2x", 1);
                pinGod.SetLampState("3x", 0);
            }
            else if (_player.BonusMultiplier == 3)
            {
                pinGod.SetLampState("2x", 1);
                pinGod.SetLampState("3x", 1);
            }
        }

        if (_player.PinsCompleted > 0)
        {
            //light bumpers
            switch (_player.DarsCompleted)
            {
                case 1:
                    pinGod.SetLampState("bumper_left", 1);
                    break;
                case 2:
                    pinGod.SetLampState("bumper_left", 1);
                    pinGod.SetLampState("bumper_top", 1);
                    break;
                case 3:
                    pinGod.SetLampState("bumper_left", 1);
                    pinGod.SetLampState("bumper_top", 1);
                    pinGod.SetLampState("bumper_bottom", 1);
                    pinGod.SetLampState("pin_5k", 1);
                    break;
                case 4:
                    pinGod.SetLampState("special_l", 1);
                    pinGod.SetLampState("special_r", 1);
                    break;
                case 5:
                case 6:
                    break;
                default:
                    break;
            }
        }
    }

    private void UpdateBonusLamps()
    {
        for (int i = 0; i < 11; i++)
        {
            pinGod.SetLampState($"b_{i + 1}", 0);
        }

        if(_player.BonusCount >= 20)
        {
            pinGod.SetLampState($"b_11", 1);
            var cnt = _player.BonusCount;
            while (cnt % 10 != 0)
            {
                pinGod.SetLampState($"b_{cnt % 10}", 1);
                cnt--;                
            }
        }
        else
        {
            if(_player.BonusCount>=10)
                pinGod.SetLampState($"b_10", 1);

            var cnt = _player.BonusCount;
            while (cnt % 10 != 0)
            {
                pinGod.SetLampState($"b_{cnt % 10}", 1);
                cnt--;
            }
        }        
    }

    private void AdvanceBonus(int amt = 1)
    {
        _player.AdvanceBonus(amt);        
        pinGod.PlaySfx("bonus_advance", "Voice"); //reverb on this channel
        (pinGod as CustomPinGodGame).PlayMusicForBonus();
    }

    private void AdvancePinDarDropsCompleted()
    {
        if (_player.PinDarCompleted == 1)
        {
            _player.PinDarCompleted = 0;
            _player.AdvanceEjectHoleValue();
        }
        else _player.PinDarCompleted = 1;

    }

    private void AdvanceSpotTarget(string swName)
    {        
        _player.SpotTargets[swName] = 1;

        if (_player.SpotTargetsCompleteCount > 0)
        {
            pinGod.AddPoints(Constant._1K * 2);
        }
        else
        {
            pinGod.AddPoints(Constant._1K);
        }

        bool result = _player.AreSpotTargetsComplete();
        if (result)
        {
            _player.SpotTargetsCompleteCount++;
            _player.ResetSpotTargets();
            _player.MagnetValue = MagnetValue.magnet_50k;
            pinGod.PlayVoice("meet_me", "Voice");

            if(_player.SpotTargetsCompleteCount == 3)
                StartChasingArrows();
        }
    }

    private void StartChasingArrows()
    {
        _spotTargetTimer.Start(); //5 secs
    }

    void SpotTargetTimer_timeout()
    {
        _player.SpotTargetArrowNumber++;
        if (_player.SpotTargetArrowNumber > 4)
            _player.SpotTargetArrowNumber = 1;

        UpdateSpotArrowLamps();
    }

    void UpdateSpotArrowLamps()
    {
        for (int i = 1; i < 5; i++)
        {
            if (_player.SpotTargetArrowNumber == i)
                pinGod.SetLampState(i + "_arrow", 2);
            else
                pinGod.SetLampState(i + "_arrow", 0);
        }
    }

    /// <summary>
    /// Adds a ball save scene to the tree and removes
    /// </summary>
    /// <param name="time">removes the scene after the time</param>
    private void DisplayBallSaveScene(float time = 2f)
    {
        var ballSaveScene = _ballSaveScene.Instance<BallSave>();
        ballSaveScene.SetRemoveAfterTime(time);
        AddChild(_ballSaveScene.Instance());
    }

    void MagnetTimer_timeout()
    {
        pinGod.SolenoidOn("magnet", 0);
        pinGod.SolenoidOn("magnet_flash", 0);
        pinGod.SolenoidOn("vpcoil", 0);

        _scoreMode.ChangeSnakeAnimation("crawl_CINEMA_4D_Main", true);
    }

    /// <summary>
    /// Saucer "kicker" active.
    /// </summary>
    private void OnBallStackPinball_SwitchActive()
    {
        if (!pinGod.IsTilted && pinGod.GameInPlay)
        {
            pinGod.AddPoints(Constant._1K);
            AdvanceBonus(1);
            pinGod.PlayVoice("meet_me", "Voice");
            if (_player.EjectHoleValue == 369)
            {
                _player.ExtraBalls++;
                _player.EjectHoleValue = Constant._15K;
                _scoreMode.ChangeSnakeAnimation("crawl to coiled_CINEMA_4D_Main", false);
            }
            else
            {
                pinGod.AddPoints(_player.EjectHoleValue);
                _scoreMode.ChangeSnakeAnimation("atk coiled_CINEMA_4D_Main", false);
            }

            pinGod.SolenoidOn("vpcoil", 1);

            UpdateLamps();
        }

        _ballStackSaucer.Start(1.5f);
    }

    private void OnBallStackPinball_timeout()
    {
        _ballStackSaucer.SolenoidPulse();
        pinGod.SolenoidOn("vpcoil", 0);
        _scoreMode.ChangeSnakeAnimation("crawl_CINEMA_4D_Main", true);
    }
}
