using Godot;

/// <summary>
/// left ramp, inner spin then right ramp
/// </summary>
public class MultiballRipper : PinGodGameMode
{
    private AnimatedSprite _anims;
    private Timer _clearDisplayTimer;
    private Control _control;
    private int _currentShotIndex;
    private int _doubleSuperJackpot = MotuConstant._1M + MotuConstant._500K;
    private bool _doubleSuperJackpotReady;
    private int _doubleSuperJackpotsCollected;
    private int _jackpot = MotuConstant._300K + MotuConstant._50K;
    private Label _label;
    private Label _label2;
    private Timer _modeEndingTimer;
    private MotuPlayer _player;
    private int _superJackpot = MotuConstant._750K;
    private bool _superJackpotReady;
    private int _superJackpotsCollected;
    private Timer _superTimer;
    private long _totalScored;
    string[] RipperLamps = new string[] { "rip_left_loop", "rip_ramp_left", "rip_inner_spin", "rip_inner_right", "rip_right_ramp", "rip_right_loop", };
    internal bool[] RipperShots = new bool[] { true, true, true, false, false, false };

    public override void _ExitTree()
    {
        base._ExitTree();
        _player.IsRipperMultiballRunning = false;        
    }

    /// <summary>
    /// Checks the ripper shots and score jackpot if ready
    /// </summary>
    /// <param name="index"></param>
    internal void RipperShotHit(int index)
    {
        if (RipperShots[index])
        {
            AssignNewShot(index);
            ScoreAndDisplayJackpot();
        }
    }


    public override void _Ready()
    {
        base._Ready();

        _control = GetNode<Control>("Control");
        _anims = _control.GetNode<AnimatedSprite>("Anims");
        _anims.Animation = "rip"; _anims.Frame = 0;

        _label = _control.GetNode<Label>("Label");
        _label2 = _control.GetNode<Label>("Label2");
        _clearDisplayTimer = GetNode<Timer>("ClearDisplayTimer");
        _modeEndingTimer = GetNode<Timer>("ModeEndingTimer");
        _superTimer = GetNode<Timer>("SuperJackTimer");

        _control.Visible = false;

        var game = (pinGod as MotuPinGodGame);
        _player = (pinGod as MotuPinGodGame).CurrentPlayer() ?? new MotuPlayer();

        if (game != null)
        {
            var score = pinGod?.AddPoints(MotuConstant._150K) ?? 0;
            _totalScored += score;
        }

        _player.SetLadder("ripper", MotuLadder.PartComplete);
        _player.RipperSuperjackpotReady = false;
        _player.RipperDoubleSuperjackpotReady = false;
        _player.RipperDoubleSuperJackpotsCollected = 0;
        _player.IsRipperMultiballRunning = true;
        ResetJackpotDisableSuperJackpot();

        StartMultiball();

        //lift targets in this multiball
        (pinGod as MotuPinGodGame)?.SkellyTargetsDown(false);

        UpdateLamps();
    }

    /// <summary>
    /// Used to restart the mode from other modes
    /// </summary>
    public void RestartMultiball()
    {
        pinGod.LogInfo("ripper reset mball");
        _player.IsRipperModeEnded = false;
        _player.IsRipperModeEnding = false;
        _modeEndingTimer.Stop();
    }

    protected override void OnBallDrained()
    {
        base.OnBallDrained();
        this.QueueFree();
    }

    /// <summary>
    /// updates available ripper shots
    /// </summary>
    protected override void UpdateLamps()
    {
        for (int i = 0; i < RipperShots.Length; i++)
        {
            var state = (byte)(RipperShots[i] ? 1 : 2);
            pinGod.SetLampState(RipperLamps[i], state);
        }
    }

    /// <summary>
    /// cycles next shot when available
    /// </summary>
    /// <param name="lastNumber"></param>
    internal void AssignNewShot(int lastNumber)
    {
        lastNumber = lastNumber > 5 ? 0 : lastNumber;
        RipperShots[lastNumber] = false;

        switch (_currentShotIndex)
        {
            case 0:
                if (!RipperShots[1]) RipperShots[1] = true;
                else AssignNewShot(lastNumber + 1);
                break;
            case 1:
                if (!RipperShots[2]) RipperShots[2] = true;
                else AssignNewShot(lastNumber + 1);
                break;
            case 2:
                if (!RipperShots[3]) RipperShots[3] = true;
                else AssignNewShot(lastNumber + 1);
                break;
            case 3:
                if (!RipperShots[4]) RipperShots[4] = true;
                else AssignNewShot(lastNumber + 1);
                break;
            case 4:
                if (!RipperShots[5]) RipperShots[5] = true;
                else AssignNewShot(lastNumber + 1);
                break;
            case 5:
                if (!RipperShots[0]) RipperShots[0] = true;
                else AssignNewShot(lastNumber + 1);
                break;
            default:
                break;
        }

        _currentShotIndex++;
        _currentShotIndex = _currentShotIndex == 6 ? 0 : _currentShotIndex;        
    }

    void ClearDisplayTimer_timeout()
    {
        _control.Visible = false;
        _anims.Stop(); _anims.Frame = 0;
    }

    /// <summary>
    /// Invoked from signal. Start the mode end time to give player chance to complete the mode.
    /// </summary>
    void EndMultiball()
    {
        _player.IsRipperModeEnding = true;
        _modeEndingTimer.Start(10);
    }

    private void LaunchMultiball()
    {
        pinGod.StartMultiBall(2, 20);
        pinGod.LogInfo("ripper multiball started");
        pinGod.PlayMusic("the_chase");
    }

    /// <summary>
    /// Activates super jackpot at spinner
    /// </summary>
    internal void LeftRamp()
    {
        if (!_superJackpotReady)
        {
            pinGod.AddPoints(_jackpot);
            _superJackpotReady = true;
            _superTimer.Start(6);
            pinGod.LogInfo("ripper super jackpot ready, timer started");
            pinGod.PlaySfx("thunder");
        }

        UpdateLamps();
    }

    void ModeEndingTimer_timeout()
    {
        if (_player.IsRipperModeEnding)
        {
            _anims.Frame = 0;_anims.Play();
            _label.Text = "RIPPER MULTIBALL";
            _label2.Text = $"TOTAL SCORED: {_totalScored.ToScoreString()}";
            _player.IsRipperModeEnding = false;
            _player.IsRipperModeEnded = true;
            _player.IsRipperMultiballRunning = false;
            _control.Visible = true;
            _modeEndingTimer.Start(2);

            PlaySequence();
            pinGod.LogInfo("ripper mball mode ended after 10secs");

            var game = GetParent().GetParent() as Game;
            if(game != null)
            {
                if (!_player.GrayskullModeEnding)
                {
                    (GetParent().GetParent() as Game)?.StartSorceress();
                }
            }
        }
        else
        {
            pinGod.LogInfo("ripper mball mode ended after 12 secs");
            this.QueueFree();
        }
    }

    void PlaySequence(string anim = "rip", string msg = "RIPPER MULTIBALL", string msg2 = "", float delay = 2f)
    {
        _clearDisplayTimer.Stop();
        _anims.Frame = 0;
        _anims.Play();
        _label.Text = msg;
        _label2.Text = msg2;
        _control.Visible = true;
        _clearDisplayTimer.Start(delay);
    }

    private void ResetJackpotDisableSuperJackpot()
    {

        //todo: check why only gives 10k a shot?
        _jackpot = MotuConstant._300K + MotuConstant._50K;
        _superJackpotReady = false;
        _doubleSuperJackpotReady = false;
        _doubleSuperJackpot = MotuConstant._1M + MotuConstant._500K;
    }

    /// <summary>
    /// Scores double super jackpot if ready
    /// </summary>
    internal void RightRamp()
    {
        //lift the car toy
        pinGod.SolenoidPulse("ripper");

        if (_doubleSuperJackpotReady)
        {
            _doubleSuperJackpotsCollected++;
            _doubleSuperJackpotReady = false;

            _superJackpotReady = true;            
            pinGod.LogInfo("ripper: double super jackpot");
            _player.SetLadder("ripper", MotuLadder.Complete);

            var jack = pinGod.AddPoints(_doubleSuperJackpot);
            _totalScored += jack;

            PlaySequence(msg: $"{jack.ToScoreString()}", msg2: "DOUBLE SUPER JACKPOT", delay: 3f);

            ResetJackpotDisableSuperJackpot();            
            pinGod.SolenoidOn("vpcoil", 5); //screwflashers

            //todo: play sfx super jackpot
        }

        UpdateLamps();
    }

    /// <summary>
    /// Standard jackpot, super jackpot and sets double super ready
    /// </summary>
    /// <param name="doubleJackpot"></param>
    internal void ScoreAndDisplayJackpot()
    {       
        if (_superJackpotReady)
        {
            _superJackpotReady = false;
            _doubleSuperJackpotReady = true;
            pinGod.LogInfo("ripper super jackpot collected, enabling double");

            var scored = pinGod.AddPoints(_superJackpot);
            _totalScored += scored;

            PlaySequence(msg: $"SUPER JACKPOT {scored.ToScoreString()}", msg2: $"DOUBLE SUPER AT RIGHT RAMP");

            _superJackpotsCollected++;
            _superTimer.Start(6);
        }
        else
        {
            var jack = pinGod.AddPoints(_jackpot);
            _totalScored += jack;
            PlaySequence(msg: $"{jack.ToScoreString()}", msg2: "JACKPOT - TOTAL SCORED" + _totalScored.ToScoreString());
        }
    }

    private void StartMultiball()
    {
        if (_player.GrayskullModeEnding || _player.IsGrayskullMultiballRunning)
        {
            var skullMode = GetParent().GetNodeOrNull<MultiballGrayskull>("MultiballGrayskull");
            if (skullMode != null)
            {
                skullMode.ResetModeEnding();

                var balls = pinGod.BallsInPlay();
                balls = balls < 3 ? 3 : balls;

                pinGod.LogWarning("launching stacked multiball. " + balls + " balls");
                pinGod.StartMultiBall((byte)balls, 20);
            }
            else
            {
                pinGod.LogWarning("tried to reset grayskull multiball but mode wasn't found");
                LaunchMultiball();
            }
        }
        else
        {
            LaunchMultiball();
        }
    }

    void SuperJackTimer_timeout()
    {
        pinGod.LogInfo("ripper super jackpot timed out");
        _superJackpotReady = false;
        _doubleSuperJackpotReady = false;
    }
}