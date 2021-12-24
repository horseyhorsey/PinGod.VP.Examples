using Godot;

/// <summary>
/// Spinners add to time
/// </summary>
public class StratosMode : PinGodGameMode
{
    private AudioStreamPlayer _audioPlayer;
    [Export] AudioStream _completeSound;
    private Control _control;
    [Export] int _modeTime = 25;
    private Timer _modeTimer;
    private MotuPlayer _player;
    private AnimatedSprite _stratosAnim;
    [Export] int _stratosAward = MotuConstant._750K;
    private bool _stratosComplete;
    private AnimationPlayer _stratosExtendedAnim;
    [Export] int _stratosMax = 2500000;
    AudioStream _swoopSound;

    public override void _ExitTree()
    {
        base._ExitTree();
        _player.IsStratosRunning = false;
        (GetParent().GetParent() as Game)?.StartSorceress();
    }

    public override void _Ready()
    {
        base._Ready();

        //get the player and setup beastman round
        _player = ((MotuPinGodGame)pinGod)?.CurrentPlayer() ?? new MotuPlayer();

        _control = GetNode<Control>("Control");
        _stratosAnim = _control.GetNode("StratosAnim") as AnimatedSprite;

        _player.SetLadder("stratos", MotuLadder.PartComplete);

        _stratosExtendedAnim = GetNode<AnimationPlayer>(nameof(AnimationPlayer));

        _modeTimer = GetNode<Timer>("ModeTimer");

        //get the player and save the loaded sound
        _audioPlayer = GetNode<AudioStreamPlayer>(nameof(AudioStreamPlayer));
        _swoopSound = _audioPlayer.Stream;

        _stratosAnim.Frame = 0;
        _stratosAnim.Play();

        _modeTimer.Start();
    }

    protected override void OnBallDrained()
    {
        base.OnBallDrained();
        this.QueueFree();
    }

    internal void AddTime()
    {
        if (_modeTime < 40)
        {
            if (_modeTime + 10 >= 40)
                _modeTime = 40;
            else
            {
                _modeTime += 10;
            }

            ShowTimeExtended();
            pinGod.LogInfo("stratos: time extended");
        }
    }

    void ModeTimer_timeout()
    {
        if (_modeTime <= 0)
        {
            _modeTimer.Stop();
            pinGod.LogInfo("stratos: timed out");
            this.QueueFree();
        }
        else
        {
            _modeTime--;
        }
    }

    internal void ProcessStratosShot()
    {
        _stratosAward += MotuConstant._250K;
        if (_stratosAward >= _stratosMax)
        {
            _stratosAward = _stratosMax;
        }

        pinGod.AddPoints(_stratosAward);
        if (!_stratosComplete)
        {
            _stratosComplete = true;
            _audioPlayer.Stream = _completeSound;
            _audioPlayer.Play();
            _player.SetLadder("stratos", MotuLadder.Complete);
            pinGod.LogInfo("stratos: mode completed");
        }
        else
        {
            _audioPlayer.Stream = _swoopSound;
            _audioPlayer.Play();
        }
    }
    private void ShowTimeExtended()
    {
        _stratosExtendedAnim.Play("StratosExtendedAnim");
        _audioPlayer.Stream = _swoopSound;
        _audioPlayer.Play();
    }

    void StratosAnim_animation_finished()
    {
        _control.Visible = false;
    }
}
