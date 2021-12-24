using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class MotuWizardLane
{
    public bool[] Completed { get; set; }
    public bool IsCompleted { get; set; }
    public Label Label { get; set; }
    public string[] Lamps { get; set; }
    public string Name { get; internal set; }
    public int Shots { get; set; }
    /// <summary>
    /// Returns true if fully completed
    /// </summary>
    /// <param name="shots"></param>
    /// <returns></returns>
    public bool CheckShotStatus()
    {
        for (int i = 0; i < Completed.Length; i++)
        {
            if (!Completed[i])
            {
                Completed[i] = true;
                return Completed.Length == i + 1 ? true : false;
            }
        }

        return true;
    }

    public override string ToString() => $"{Name} = {Shots}";

    internal void UpdateLabel() => Label.Text = ToString();
}

public class MultiballMotu : PinGodGameMode
{
    int _jackpot = MotuConstant._500K;
    int _jackpotIncrementAmt = MotuConstant._50K;
    private List<MotuWizardLane> _lanes;
    private bool[] _lanesComplete = new bool[6];
    private Timer _modeEndTimer;
    private MotuPlayer _player;
    int _superJackpot = MotuConstant._1M;
    private Timer _timer;
    bool introEnding = false;
    bool AllLanesComplete => !_lanesComplete.Any(x => !x);
    long totalScored = 0;

    public override void _ExitTree()
    {
        base._ExitTree();
        _player.IsMotuMultiballRunning = false;
        _player.ResetAfterCompletion();
        pinGod.LogInfo("todo: reset player, update lamps");
    }

    public override void _Input(InputEvent @event)
    {

        var loopLChange = pinGod.GetLastSwitchChangedTime("loop_left");
        var loopRChange = pinGod.GetLastSwitchChangedTime("loop_right");
        if (pinGod.SwitchOn("loop_left", @event) && loopRChange > 1500f)
        {
            LeftLoop();
        }

        var lastChange = pinGod.GetLastSwitchChangedTime("inner_spinner");
        if (pinGod.SwitchOn("inner_spinner", @event) && lastChange > 1500f && lastChange > 0f)
        {
            pinGod.LogInfo("spin");
            InnerSpinner();
        }

        if (pinGod.SwitchOn("leftramp_full", @event))
        {
            LeftRamp();
        }

        if (pinGod.SwitchOn("rip_ramp_mid", @event))
        {
            RightRamp();
        }

        if (pinGod.SwitchOn("inner_skele_loop", @event))
        {
            InnerLoop();
        }

        if (pinGod.SwitchOn("loop_right", @event) && loopLChange > 1500f)
        {
            RightLoop();
        }

        if (pinGod.GetLastSwitchChangedTime("skele_trigger") > 1500f && pinGod.SwitchOn("skele_trigger", @event))
        {
            if (AllLanesComplete)
            {
                pinGod.AddPoints(_superJackpot);
                pinGod.LogInfo("motu super jackpot " + _superJackpot);                
                pinGod.PlaySfx("spin_sword");
                pinGod.SolenoidOn("vp_coil", 5);
                ResetShots();
                (pinGod as MotuPinGodGame).SkellyTargetsDown(false);
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();

        _player = (pinGod as MotuPinGodGame).CurrentPlayer() ?? new MotuPlayer();
        _player.IsMotuMultiballRunning = true;
        pinGod.DisableAllLamps();

        _lanes = GenerateLanes();

        //create arrays for shots to make
        ResetShots();
        
        if (!_player.HasPlayedSorceress)
        {
            pinGod.AddPoints(MotuConstant._1M * 10); //4mil
            //todo: big sound 10 mill
        }

        _timer = new Timer() { WaitTime = 12, Autostart = true, OneShot = true };
        _timer.Connect("timeout", this, "ModeIntroTimeout");
        this.AddChild(_timer);

        _modeEndTimer = GetNode<Timer>("ModeEndingTimer");

        pinGod.AddPoints(MotuConstant._1M * 4); //4mil        

        pinGod.PlayMusic("theme");
        pinGod.LogInfo("motu ready");

        pinGod.SolenoidOn("vpcoil", 10); //todo: vpcoil 10

        pinGod.SetBallSearchStop();
    }

    protected override void OnBallDrained()
    {
        base.OnBallDrained();

        _player.IsMotuMultiballRunning = false;

        this.QueueFree();
    }

    protected override void UpdateLamps()
    {
        base.UpdateLamps();

        for (int i = 0; i < _lanes[0].Lamps.Length; i++)
        {
            pinGod.SetLampState(_lanes[0].Lamps[i], (byte)(_lanes[0].Completed[i] ? 0 : 1));
        }

        for (int i = 0; i < _lanes[1].Lamps.Length; i++)
        {
            pinGod.SetLampState(_lanes[1].Lamps[i], (byte)(_lanes[1].Completed[i] ? 0 : 1));
        }

        for (int i = 0; i < _lanes[2].Lamps.Length; i++)
        {
            pinGod.SetLampState(_lanes[2].Lamps[i], (byte)(_lanes[2].Completed[i] ? 0 : 1));
        }

        for (int i = 0; i < _lanes[3].Lamps.Length; i++)
        {
            pinGod.SetLampState(_lanes[3].Lamps[i], (byte)(_lanes[3].Completed[i] ? 0 : 1));
        }

        for (int i = 0; i < _lanes[4].Lamps.Length; i++)
        {
            pinGod.SetLampState(_lanes[4].Lamps[i], (byte)(_lanes[4].Completed[i] ? 0 : 1));
        }

        for (int i = 0; i < _lanes[5].Lamps.Length; i++)
        {
            pinGod.SetLampState(_lanes[5].Lamps[i], (byte)(_lanes[5].Completed[i] ? 0 : 1));
        }
    }

    void AddToJackpot()
    {
        if (_jackpot < (MotuConstant._1M + MotuConstant._500K))
        {
            _jackpot += _jackpotIncrementAmt;
        }
        else { _jackpot = MotuConstant._1M + MotuConstant._500K; }
    }

    /// <summary>
    /// Invoked from signal. Start the mode end time to give player chance to complete the mode.
    /// </summary>
    void EndMultiball()
    {
        _modeEndTimer.Start(10); //todo: show some stuff, totals

        if (!_player.IsMotuMultiballRunning)
            pinGod.PlayMusic("theme_alt");

        GetNode<VBoxContainer>(nameof(VBoxContainer)).Visible = false;

        GetNode<Label>("Control/Label").Text = $"{totalScored.ToScoreString()}";
        GetNode<Label>("Control/Label2").Text = $"MOTU MULTIBALL TOTAL SCORED";
    }

    private List<MotuWizardLane> GenerateLanes()
    {
        return new List<MotuWizardLane>()
        {
            new MotuWizardLane()
            {
                Name = "LEFT LOOP",
                Lamps = new string[] { "man_left_loop", "rip_left_loop", "skele_loop_l", "arrow_l_left_loop" },
                Shots = 4,
                Label = GetNode<Label>("VBoxContainer/LeftLoop"),
                Completed = new bool[4]
            },
            new MotuWizardLane()
            {
                Name = "GRAYSKULL RAMP",
                Lamps = new string[] { "rip_ramp_left", "skele_ramp_l", "arrow_l_left_ramp" },
                Shots = 3,
                Label = GetNode<Label>("VBoxContainer/Grayskull"),
                Completed = new bool[3]
            },
            new MotuWizardLane()
            {
                Name = "GRAYSKULL SPINNER",
                Lamps = new string[] { "gray_spin", "man_inner_spin", "rip_inner_spin", "skele_inner_spin", "arrow_l_inner_spin" },
                Shots = 5,
                Label = GetNode<Label>("VBoxContainer/Spinner"),
                Completed = new bool[5]
            },
            new MotuWizardLane()
            {
                Name = "INNER LOOP",
                Lamps = new string[] { "rip_inner_right", "skele_inner_loop", "arrow_l_inner_loop" },
                Shots = 3,
                Label = GetNode<Label>("VBoxContainer/InnerLoop"),
                Completed = new bool[3]
            },
            new MotuWizardLane()
            {
                Name = "RIPPER RAMP",
                Lamps = new string[] { "rip_right_ramp", "skele_ramp_r", "arrow_l_right_loop" },
                Shots = 3,
                Label = GetNode<Label>("VBoxContainer/Ripper"),
                Completed = new bool[3]
            },
            new MotuWizardLane()
            {
                Name = "RIGHT LOOP",
                Lamps = new string[] { "man_r_loop", "rip_right_loop", "skele_loop_r", "arrow_l_right_loop" },
                Shots = 4,
                Label = GetNode<Label>("VBoxContainer/RightLoop"),
                Completed = new bool[4]
            }
        };
    }
    void InnerLoop()
    {
        var status = _lanes[3].CheckShotStatus();
        if (status)
        {
            pinGod.LogInfo("motu: inner loop complete");
            SetShotCompleted(3);
        }
        else
        {
            pinGod.LogInfo("motu: inner loop advanced");
            _lanes[3].Shots--;
            _lanes[3].UpdateLabel();
            totalScored += pinGod.AddPoints(_jackpot);
            AddToJackpot();
        }

        UpdateLamps();
    }

    void InnerSpinner()
    {
        if (_lanes[2].CheckShotStatus())
        {
            pinGod.LogInfo("motu: spinner shots complete");
            SetShotCompleted(2);
        }
        else
        {
            pinGod.LogInfo("motu: spinner lane advanced");
            _lanes[2].Shots--;
            _lanes[2].UpdateLabel();

            totalScored += pinGod.AddPoints(_jackpot);
            AddToJackpot();
        }

        UpdateLamps();
    }

    void LeftLoop()
    {
        if (_lanes[0].CheckShotStatus())
        {
            pinGod.LogInfo("motu: left loop complete");
            SetShotCompleted(0);
        }
        else
        {
            pinGod.LogInfo("motu: left loop advanced");
            _lanes[0].Shots--;
            _lanes[0].UpdateLabel();

            totalScored += pinGod.AddPoints(_jackpot);
            AddToJackpot();
        }

        UpdateLamps();
    }

    void LeftRamp()
    {
        if (_lanes[1].CheckShotStatus())
        {
            SetShotCompleted(1);
        }
        else
        {
            pinGod.LogInfo("motu: left ramp");
            _lanes[1].Shots--;
            _lanes[1].UpdateLabel();

            totalScored += pinGod.AddPoints(_jackpot);
            AddToJackpot();
        }

        UpdateLamps();
    }

    void ModeEndingTimer_timeout()
    {
        this.QueueFree();
    }

    void ModeIntroTimeout()
    {
        if (!introEnding)
        {
            pinGod.SetBallSearchReset();
            introEnding = true;
            pinGod.SolenoidOn("vpcoil", 11); //todo: vpcoil 11
            _timer.Start(2);
        }
        else
        {
            pinGod.SetBallSearchReset();

            _timer.Stop();
            pinGod.SolenoidPulse("heman_scoop");

            //8-ball multiball - standard
            pinGod.StartMultiBall(8, 30);

            UpdateLamps();
        }
    }

    void OpenJackpot()
    {
        if (AllLanesComplete)
        {
            pinGod.LogInfo("motu: all lanes complete");
            (pinGod as MotuPinGodGame)?.SkellyTargetsDown(true);
            pinGod.PlayMusic("creeping");
        }
    }

    void ResetShots()
    {
        _lanes = GenerateLanes();
        _lanesComplete = new bool[6];
        UpdateLamps();
    }

    void RightLoop()
    {
        if (_lanes[5].CheckShotStatus())
        {
            pinGod.LogInfo("motu: right loop complete");
            SetShotCompleted(5);
        }
        else
        {
            pinGod.LogInfo("motu: left ramp");
            _lanes[5].Shots--;
            _lanes[5].UpdateLabel();

            totalScored += pinGod.AddPoints(_jackpot);
            AddToJackpot();
        }

        UpdateLamps();
    }

    void RightRamp()
    {
        if (_lanes[4].CheckShotStatus())
        {
            pinGod.LogInfo("motu: right ramp complete");
            SetShotCompleted(4);
        }
        else
        {
            pinGod.LogInfo("motu: right ramp");
            _lanes[4].Shots--;
            _lanes[4].UpdateLabel();

            totalScored += pinGod.AddPoints(_jackpot);
            AddToJackpot();
        }

        UpdateLamps();
    }

    private void SetShotCompleted(int laneNum)
    {
        _lanesComplete[laneNum] = true;
        OpenJackpot();
    }
}
