
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// One ball mode to complete all tasks not lit solid. "Complete all stages to reach MOTU"
/// Each solid mode is awarded 1 million. You can have Double Scoring running.
/// The Roton will always be lit by this stage, but the other shots are:
/// Beastman - shoot the scoop once
///	Stratos - Shoot the Gem shot once
///	Man - Shoot each Man at arms shot once
///	Orko - Hit each ORKO target once
/// Grayskull - Shoot the Stratos inner loop once
/// Skeletor discs - Shoot each blue disc shot once
/// Ripper - Shoot each Ripper shot once
/// Adam - shoot each of the A-D-A-M targets once
/// You get the 1 million missed from incomplete modes when doing this.
/// </summary>
public class SorceressMode : PinGodGameMode
{
    private AnimatedSprite _anims;
    private Control _control;
    private string _currentMode;
    private Label _label;
    private Label _label2;
    private List<string> _modesComplete;
    /// <summary>
    /// Collection of completed modes so not to show in rundown
    /// </summary>
    private List<string> _modesCompleted;
    private MotuPlayer _player;
    private Timer _timer;
    private int _totalAwarded;
    private bool[] adam = new bool[4];
    private string[] adamLamps = new string[] { "Adam", "aDam", "adAm", "adaM"};
    private bool[] man = new bool[4];
    private string[] manLamps = new string[] { "man_left_loop", "man_inner_spin", "man_r_loop", "man_scoop" };
    private bool[] orko = new bool[4];
    private string[] orkoLamps = new string[] { "Orko", "oRko", "orKo", "orkO" };
    private bool[] ripper = new bool[6];
    private string[] ripperLamps = new string[] { "rip_left_loop", "rip_ramp_left", "rip_inner_spin", "rip_inner_right", "rip_right_ramp", "rip_right_loop" };
    private bool[] skeletor = new bool[6];
    private string[] skeletorLamps = new string[] { "skele_loop_l", "skele_ramp_l", "skele_inner_spin", "skele_inner_loop", "skele_ramp_r", "skele_loop_r" };
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (!_player.IsSorceressRunning) return;

        //ORKO MODE SHOTS
        if (_currentMode == "orko")
        {
            if (pinGod.SwitchOn("Orko", @event))
            {
                if (!orko[0])
                {
                    orko[0] = true;
                    SetOrkoProgress();
                }
            }
            if (pinGod.SwitchOn("oRko", @event))
            {
                if (!orko[1])
                {
                    orko[1] = true;
                    SetOrkoProgress();
                }
            }
            if (pinGod.SwitchOn("orKo", @event))
            {
                if (!orko[2])
                {
                    orko[2] = true;
                    SetOrkoProgress();
                }
            }
            if (pinGod.SwitchOn("orkO", @event))
            {
                if (!orko[3])
                {
                    orko[3] = true;
                    SetOrkoProgress();
                }
            }
        }
        else if (_currentMode == "adam")
        {
            if (pinGod.SwitchOn("Adam", @event))
            {
                if (!adam[0])
                {
                    adam[0] = true;
                    SetAdamProgress();
                }
            }
            if (pinGod.SwitchOn("aDam", @event))
            {
                if (!adam[1])
                {
                    adam[1] = true;
                    SetAdamProgress();
                }
            }
            if (pinGod.SwitchOn("adAm", @event))
            {
                if (!adam[2])
                {
                    adam[2] = true;
                    SetAdamProgress();
                }
            }
            if (pinGod.SwitchOn("adaM", @event))
            {
                if (!adam[3])
                {
                    adam[3] = true;
                    SetAdamProgress();
                }
            }
        }
        else if (_currentMode == "beastman")
        {
            if (pinGod.SwitchOn("heman_scoop", @event))
            {
                _player.SetLadder("beastman", MotuLadder.Complete);
                DisplayModeRunDown();
            }
        }
        else
        {
            var loopLeftChanged = pinGod.GetLastSwitchChangedTime("loop_left");
            var loopRightChanged = pinGod.GetLastSwitchChangedTime("loop_right");
            if (pinGod.SwitchOn("loop_left", @event) && loopRightChanged > 2000f)
            {
                if (_currentMode == "manatarms")
                {
                    if (!man[0])
                    {
                        man[0] = true;
                        SetManProgress();
                    }
                }
                else if (_currentMode == "ripper")
                {
                    if (!ripper[0])
                    {
                        ripper[0] = true;
                        SetRipperProgress();
                    }
                }
                else if (_currentMode == "skeletor")
                {
                    if (!skeletor[0])
                    {
                        skeletor[0] = true;
                        SetSkeletorProgress();
                    }
                }
            }

            if (pinGod.SwitchOn("loop_right", @event) && loopLeftChanged > 2000f)
            {
                if (_currentMode == "manatarms")
                {
                    if (!man[2])
                    {
                        man[2] = true;
                        SetManProgress();
                    }
                }
                else if (_currentMode == "ripper")
                {
                    if (!ripper[5])
                    {
                        ripper[5] = true;
                        SetRipperProgress();
                    }
                }
                else if (_currentMode == "skeletor")
                {
                    if (!skeletor[5])
                    {
                        skeletor[5] = true;
                        SetSkeletorProgress();
                    }
                }
            }

            if (pinGod.SwitchOn("heman_scoop", @event))
            {
                if (_currentMode == "manatarms")
                {
                    if (!man[3])
                    {
                        man[3] = true;
                        SetManProgress();
                    }
                }
            }

            if (pinGod.SwitchOn("leftramp_full", @event))
            {
                if (_currentMode == "ripper")
                {
                    if (!ripper[1])
                    {
                        ripper[1] = true;
                        SetRipperProgress();
                    }
                }
                else if (_currentMode == "skeletor")
                {
                    if (!skeletor[1])
                    {
                        skeletor[1] = true;
                        SetSkeletorProgress();
                    }
                }
            }

            if (pinGod.SwitchOn("rip_ramp_mid", @event))
            {
                if (_currentMode == "ripper")
                {
                    if (!ripper[4])
                    {
                        ripper[4] = true;
                        SetRipperProgress();
                    }
                }
                else if (_currentMode == "skeletor")
                {
                    if (!skeletor[4])
                    {
                        skeletor[4] = true;
                        SetSkeletorProgress();
                    }
                }
            }

            if (pinGod.SwitchOn("inner_skele_loop", @event))
            {
                if (_currentMode == "ripper")
                {
                    if (!ripper[3])
                    {
                        ripper[3] = true;
                        SetRipperProgress();
                    }
                }
                else if (_currentMode == "skeletor")
                {
                    if (!skeletor[3])
                    {
                        skeletor[3] = true;
                        SetSkeletorProgress();
                    }
                }
                else if (_currentMode == "stratos")
                {
                    _player.SetLadder("stratos", MotuLadder.Complete);
                    DisplayModeRunDown();
                }
            }

            if (pinGod.SwitchOn("inner_spinner", @event) && (pinGod.GetLastSwitchChangedTime("inner_spinner") > 1500f || pinGod.GetLastSwitchChangedTime("inner_spinner") == 0))
            {
                if (_currentMode == "manatarms")
                {
                    if (!man[1])
                    {
                        man[1] = true;
                        SetManProgress();
                    }
                }
                else if (_currentMode == "grayskull")
                {
                    _player.SetLadder("grayskull", MotuLadder.Complete);
                    DisplayModeRunDown();
                }
                else if (_currentMode == "ripper")
                {
                    if (!ripper[2])
                    {
                        ripper[2] = true;
                        SetRipperProgress();
                    }
                }
                else if (_currentMode == "skeletor")
                {
                    if (!skeletor[2])
                    {
                        skeletor[2] = true;
                        SetSkeletorProgress();
                    }
                }
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();

        _player = (pinGod as MotuPinGodGame).CurrentPlayer() ?? new MotuPlayer();

        _player.HasPlayedSorceress = true;
        _player.IsSorceressRunning = true;

        _control = GetNode<Control>("Control");

        _anims = _control.GetNode<AnimatedSprite>("SorcAnim");
        _anims.Animation = "sorc";
        _anims.Frame = 0; _anims.Play();

        _label = _control.GetNode<Label>("Label");
        _label2 = _control.GetNode<Label>("Label2");

        _timer = GetNode<Timer>("ModeDisplayTimer");

        pinGod.LogInfo("sorceress mode ready. displaying modes rundown");

        pinGod.PlayMusic("funk");

        _modesCompleted = new List<string>();
        
        DisplayModeRunDown(true);
        pinGod.DisableAllLamps(); //todo: light the lamps in sequence
    }

    protected override void UpdateLamps()
    {
        base.UpdateLamps();

        pinGod.DisableAllLamps();

        switch (_currentMode)
        {
            case "beastman":
                pinGod.SetLampState("man_scoop", 2);
                break;
            case "stratos":
                pinGod.SetLampState("arrow_l_inner_loop", 2);
                break;
            case "orko":
                for (int i = 0; i < orko.Length; i++)
                {
                    pinGod.SetLampState(orkoLamps[i], (byte)(orko[i] ? 1 : 2));
                }
                break;
            case "grayskull":
                pinGod.SetLampState("gray_spin", 2);
                break;
            case "ripper":
                for (int i = 0; i < ripper.Length; i++)
                {
                    pinGod.SetLampState(ripperLamps[i], (byte)(ripper[i] ? 0 : 1));
                }
                break;
            case "skeletor":
                for (int i = 0; i < skeletor.Length; i++)
                {
                    pinGod.SetLampState(skeletorLamps[i], (byte)(skeletor[i] ? 0 : 1));
                }
                break;
            case "adam":
                for (int i = 0; i < adam.Length; i++)
                {
                    pinGod.SetLampState(adamLamps[i], (byte)(adam[i] ? 0 : 1));
                }
                break;
            default:
                break;
        }
    }

    private void DisplayModeRunDown(bool setCurrentMode = false)
    {
        var ladder = _player.MotuLadderComplete;
        _currentMode = string.Empty;

        _modesComplete = new List<string>();
        foreach (var mode in ladder)
        {
            if (!_modesCompleted.Contains(mode.Key))
            {
                if (ladder[mode.Key] != MotuLadder.Complete)
                {
                    pinGod.SetLampState("mode_"+mode.Key, 2);
                    _currentMode = mode.Key;
                    pinGod.LogInfo("current sorceress mode: " + mode.Key);
                    break;
                }
                else
                {
                    pinGod.AddPoints(MotuConstant._1M);
                    _totalAwarded += MotuConstant._1M;
                    pinGod.LogInfo(mode.Key + " - mode complete");
                    _modesComplete.Add(mode.Key);
                    _modesCompleted.Add(mode.Key);
                    pinGod.SetLampState("mode_" + mode.Key, 1);
                }
            }            
        }

        if (_modesCompleted.Count == 9)
        {
            pinGod.LogInfo("sorceress mode completed, motu ready");
            _player.IsSorceressComplete = true;
            _player.IsSorceressRunning = false;
            //todo: show MOTU ready 2 secs then remove this mode
            this.QueueFree();
        }
        else
        {
            ModeDisplayTimer_timeout(); // call timer to start rundown
        }        

        UpdateLamps();
    }

    protected override void OnBallDrained()
    {
        base.OnBallDrained();
        this.QueueFree();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        _player.IsSorceressRunning = false;
        //_player.IsWizardReady = _player.IsSorceressComplete;
    }

    void ModeDisplayTimer_timeout()
    {
        if (_modesComplete.Count > 0)
        {
            _label.Text = _modesComplete[0];
            _label2.Text = "COMPLETE 1,000,000";            
            _modesComplete.RemoveAt(0);
            _timer.Start(3f);            
            pinGod.PlaySfx("sfx7");
            _anims.Frame = 0; _anims.Play();
        }
        else if(_modesCompleted.Count < 9)
        {
            _label.Text = _currentMode;
            _label2.Text = "COMPLETE LIT SHOTS";
            pinGod.SolenoidPulse("heman_scoop");
            pinGod.PlaySfx("sfx1");
            _anims.Frame = 0; _anims.Play();
            UpdateLamps();
        }        
    }

    private void SetAdamProgress()
    {
        if (!adam.Any(x => !x))
        {
            pinGod.LogInfo("sorceress: adam completed");
            _player.SetLadder("adam", MotuLadder.Complete);
            _modesCompleted.Add("adam");
            DisplayModeRunDown();
        }
        else
        {
            UpdateLamps();
        }
    }
    
    private void SetManProgress()
    {
        if (!man.Any(x => !x))
        {
            pinGod.LogInfo("sorceress: man completed");
            _player.SetLadder("manatarms", MotuLadder.Complete);
            _modesCompleted.Add("manatarms");
            DisplayModeRunDown();
        }
        else
        {
            UpdateLamps();
        }
    }

    private void SetOrkoProgress()
    {
        if (!orko.Any(x => !x))
        {
            pinGod.LogInfo("sorceress: orko completed");
            _player.SetLadder("orko", MotuLadder.Complete);
            _modesCompleted.Add("orko");
            DisplayModeRunDown();
        }
        else
        {
            UpdateLamps();
        }
    }

    private void SetRipperProgress()
    {
        if (!ripper.Any(x => !x))
        {
            pinGod.LogInfo("sorceress: ripper completed");
            _player.SetLadder("ripper", MotuLadder.Complete);
            _modesCompleted.Add("ripper");
            DisplayModeRunDown();
        }
        else
        {
            UpdateLamps();
        }
    }

    private void SetSkeletorProgress()
    {
        if (!skeletor.Any(x => !x))
        {
            pinGod.LogInfo("sorceress: skeletor completed");
            _player.SetLadder("skeletor", MotuLadder.Complete);
            _modesCompleted.Add("skeletor");
            DisplayModeRunDown();
        }
        else
        {
            pinGod.LogInfo("sorc: skele shot");
            UpdateLamps();
        }
    }
}
