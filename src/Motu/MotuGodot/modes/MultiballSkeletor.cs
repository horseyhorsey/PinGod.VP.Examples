using Godot;
using System;
using System.Linq;

/// <summary>
/// Skele shot 7 times
/// </summary>
public class MultiballSkeletor : PinGodGameMode
{
    private AnimatedSprite _anims;
    private Timer _clearDisplayTimer;
    private Control _control;
    int _doubleJackpot = MotuConstant._500K;
    private MotuPinGodGame _game;
    int _jackpot = 0;
    private Label _label;
    private Label _label2;
    
    private Timer _modeEndingTimer;
    private MotuPlayer _player;

    bool[] _rotonEndTargets = new bool[3];
    int _rotonJackpot = 0;
    bool[] _rotonJackpotShots = new bool[6];
    string[] _skeleLamps = new string[] { "skele_loop_l", "skele_ramp_l", "skele_inner_spin", "skele_inner_loop", "skele_ramp_r", "skele_loop_r" };
    private int _skeletorShotCount;
    int _superJackpot = 0;
    long totalScored = 0;

    public override void _ExitTree()
    {
        base._ExitTree();
  
        _player.ResetSkeletorMultiball();

        foreach (var lamp in _skeleLamps)
        {
            pinGod.SetLampState(lamp, 0);
        }
        _game.SkellyTargetBankController();
    }

    public override void _Input(InputEvent @event)
    {
        //just accept inputs when actually running in the multiball
        if (!_player.IsSkeleMultiballEnded && _player.IsSkeleMultiballRunning && !pinGod.IsTilted)
        {
            var skeleLastChange = pinGod.GetLastSwitchChangedTime("skele_trigger");

            if (_skeletorShotCount == 0)
            {
                if (pinGod.SwitchOn("skele_l", @event) || pinGod.SwitchOn("skele_m", @event) || pinGod.SwitchOn("skele_r", @event))
                {
                    AwardDoubleJackpot();
                }
                
                if (pinGod.SwitchOn("skele_trigger", @event) && skeleLastChange > 1500f)
                {
                    IncrementSkeletorShot("skele_trigger");
                }
            }
            else
            {
                if (pinGod.SwitchOn("skele_trigger", @event) && skeleLastChange > 1500f)
                {
                    IncrementSkeletorShot("skele_trigger");
                }

                //get changed times
                var loopRightChanged = pinGod.GetLastSwitchChangedTime("loop_right");
                var loopLeftChanged = pinGod.GetLastSwitchChangedTime("loop_left");
                var spinnerChanged = pinGod.GetLastSwitchChangedTime("inner_spinner");

                if(loopRightChanged > 1500f && pinGod.GetLastSwitchChangedTime("plunger_lane") > 1500f)
                {
                    if (pinGod.SwitchOn("loop_left", @event))
                    {
                        if (!_rotonJackpotShots[0])
                        {
                            _rotonJackpotShots[0] = true;
                            IncrementSkeletorShot();
                        }
                    }

                    if (loopLeftChanged > 1500f && pinGod.SwitchOn("loop_right", @event))
                    {
                        if (!_rotonJackpotShots[5])
                        {
                            _rotonJackpotShots[5] = true;
                            IncrementSkeletorShot();
                        }
                    }
                }

                //we want the spinner to settle before giving this shot
                if(spinnerChanged > 1500f)
                {
                    if (pinGod.SwitchOn("inner_spinner", @event))
                    {
                        if (!_rotonJackpotShots[2])
                        {
                            _rotonJackpotShots[2] = true;
                            IncrementSkeletorShot();
                        }
                    }
                }

                if (pinGod.SwitchOn("leftramp_full", @event))
                {
                    if (!_rotonJackpotShots[1])
                    {
                        _rotonJackpotShots[1] = true;
                        IncrementSkeletorShot();
                    }
                }
                
                if (pinGod.SwitchOn("inner_skele_loop", @event))
                {
                    if (!_rotonJackpotShots[3])
                    {
                        _rotonJackpotShots[3] = true;
                        IncrementSkeletorShot();
                    }

                }
                if (pinGod.SwitchOn("rip_ramp_mid", @event))
                {
                    if (!_rotonJackpotShots[4])
                    {
                        _rotonJackpotShots[4] = true;
                        IncrementSkeletorShot();
                    }
                }                
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();

        _game = (pinGod as MotuPinGodGame);
        _player = _game?.CurrentPlayer() ?? new MotuPlayer(); //create a new player for testing if null

        pinGod.LogInfo("skeletor multiball ready. ejecting 3 balls");

        ResetJackpots();
        pinGod.StartMultiBall(3, 20);

        _player.SetLadder("skeletor", MotuLadder.PartComplete);        
        _player.IsSkeleMultiballRunning = true;

        _control = GetNode<Control>("Control");
        _anims = _control.GetNode<AnimatedSprite>("Anims");
        _label = _control.GetNode<Label>("Label");
        _label2 = _control.GetNode<Label>("Label2");

        _clearDisplayTimer = GetNode<Timer>("ClearDisplayTimer");
        _modeEndingTimer = GetNode<Timer>("ModeEndingTimer");

        _anims.Play();
        _clearDisplayTimer.Start();

        pinGod.StopMusic();

        //skeletor shot count for the lit shots
        _skeletorShotCount = 6 + _player.SkeletorMultiballsComplete;
        _game.PlayMusicForShotCount(_skeletorShotCount);

        pinGod.SolenoidOn("vpcoil", 8);
        UpdateLamps();
    }

    protected override void OnBallDrained()
    {
        _player.ResetSkeletorMultiball();
        this.QueueFree();
    }

    /// <summary>
    /// Updates the skeletor jackpot blue lamps
    /// </summary>
    protected override void UpdateLamps()
    {
        for (int i = 0; i < _rotonJackpotShots.Length; i++)
        {
            byte state = (byte)(_rotonJackpotShots[i] ? 1 : 2);
            pinGod.SetLampState(_skeleLamps[i], state);
        }
    }

    void AwardDoubleJackpot()
    {
        pinGod.AddPoints(_doubleJackpot);
        totalScored += _doubleJackpot;
        for (int i = 0; i < _rotonEndTargets.Length; i++)
        {
            if (!_rotonEndTargets[i])
            {
                _rotonEndTargets[i] = true;
                if (i == 2)
                {
                    pinGod.LogInfo("skele: roton end targets complete, lowering bank for super");
                    _game.SkellyTargetsDown(true);
                }
                break;
            }
        }
    }

    void ClearDisplayTimer_timeout()
    {
        _control.Visible = false;
        _anims.Stop();
    }

    private void DisplaySkeletorHit()
    {
        pinGod.PlaySfx("horsefx_1");
        PlaySequence(msg2: $"{_skeletorShotCount} SHOTS REMAIN");
    }

    /// <summary>
    /// Called by the game on the "multiball" group
    /// </summary>
    void EndMultiball()
    {        
        _player.IsSkeleMultiballEnding = true;
        _player.IsSkeleMultiballEnded = false;
        _modeEndingTimer.Start(12);
        pinGod.LogInfo("skeletor: ending multiball, 12 secs to restart");
    }

    private bool EndTargetsCompleted()
    {
        return !_rotonEndTargets.Any(x => !x);
    }

    void IncrementSkeletorShot(string name = "")
    {
        pinGod.AddPoints(2310, false);
        if (name == "skele_trigger")
        {
            SkeletorDiscHit();
            return;
        }

        var scored = pinGod.AddPoints(_rotonJackpot);
        totalScored += _rotonJackpot;

        PlaySequence("run", $"{scored.ToScoreString()}", $"LIT SHOTS INCREASE JACKPOT VALUE");

        _rotonJackpot += MotuConstant._25K;
        _jackpot += _rotonJackpot;
    }

    void ModeEndingTimer_timeout()
    {
        if (_player.IsSkeleMultiballEnding)
        {
            _player.IsSkeleMultiballEnding = false;
            _player.IsSkeleMultiballEnded = true;
            _player.IsSkeleMultiballRunning = false;
            _modeEndingTimer.Start(2);
            PlaySequence("run", msg2: $"TOTAL SCORED: {totalScored.ToScoreString()}");
            pinGod.LogInfo("skeletor mball mode ended after 10secs");
        }
        else
        {
            (GetParent().GetParent() as Game)?.StartSorceress();
            this.QueueFree();
        }
    }
    private void PlaySequence(string anim = "disappear", string msg = "SKELETOR MULTIBALL", string msg2 = "", float delay = 2f)
    {
        _clearDisplayTimer.Stop();
        _anims.Animation = anim;
        _anims.Frame = 0;
        _anims.Play();
        _label2.Text = msg;
        _label2.Text = msg2;
        _control.Visible = true;
        _clearDisplayTimer.Start(delay);
    }

    /// <summary>
    /// Reset the lit increase jackpot skele shots
    /// </summary>
    void ResetJackpots()
    {
        _jackpot = MotuConstant._250K;
        _rotonJackpot = MotuConstant._250K / 2;
        _rotonJackpotShots = new bool[6];
        UpdateLamps();
    }

    private void RestartMultiball()
    {
        _player.IsSkeleMultiballEnding = false;
        _player.IsSkeleMultiballEnded = false;
        _modeEndingTimer.Stop();
        pinGod.StartMultiBall(3, 20);

        if (_skeletorShotCount > 0 || EndTargetsCompleted())
        {
            _game.SkellyTargetsDown(true);
            pinGod.LogInfo("skeletor restarting, dropping bank");
        }
        else
        {
            _game.SkellyTargetsDown(false);
            pinGod.LogInfo("skeletor restarting, raising bank");
        }
    }

    private bool SecondStageReady()
    {
        if (_skeletorShotCount == 0)
        {
            _rotonJackpot += 25000;
            _game.SkellyTargetsDown(false);
            _game.PlayMusicForShotCount(_skeletorShotCount);
            pinGod.LogInfo("skeletor shots completed, lowered targets");
            //todo: sound Roton2ndStage
            return true;
        }

        return false;
    }

    private void SkeletorDiscHit()
    {
        if (_player.IsSkeleMultiballEnding)
        {
            RestartMultiball();
        }
        //collect a skeletor shot
        else if (_skeletorShotCount > 0)
        {
            _skeletorShotCount--;

            var scored = pinGod.AddPoints(_jackpot);
            pinGod.LogInfo("skele: collected shot, " + _skeletorShotCount + " remaining");
            totalScored += scored;
            ResetJackpots();
            _superJackpot += _rotonJackpot;

            if (SecondStageReady())
            {
                pinGod.PlaySfx("horsefx_1");
                PlaySequence(msg2: $"SHOOT TARGETS TO OPEN SUPER JACKPOT", delay: 3f);
            }
            else
            {
                DisplaySkeletorHit();
            }

            _game.PlayMusicForShotCount(_skeletorShotCount);
        }
        else if (EndTargetsCompleted())
        {
            //completes the mode adds super jackpot
            var jack = _superJackpot + (_doubleJackpot * 3);
            var scored = pinGod.AddPoints(jack);
            totalScored += scored;

            pinGod.LogInfo("skele: completed. super jackpot " + jack);
            _player.SetLadder("skeletor", MotuLadder.Complete);
            _player.SkeletorMultiballsComplete++;
            _skeletorShotCount = 6 + _player.SkeletorMultiballsComplete;

            ResetJackpots();
            _rotonEndTargets = new bool[3];

            _game.SkellyTargetsDown(false);

            PlaySequence(msg2: $"SUPER JACKPOT {scored.ToScoreString()}", delay: 3f);
        }
    }
}