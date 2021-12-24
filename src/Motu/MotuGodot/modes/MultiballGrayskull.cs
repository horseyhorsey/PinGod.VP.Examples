using Godot;
using System.Linq;

/// <summary>
/// Multiball with add a ball
/// </summary>
public class MultiballGrayskull : PinGodGameMode
{
    private int _addABalls = 3;
    private bool[] _addABallTargets = new bool[3];
    private AnimatedSprite _anims;
    private AudioStreamPlayer _audio;
    private AudioStream _defaultAudioStream;
    private Timer _clearDisplayTimer;
    private Control _control;
    private int _jackpot = 350000;
    private Label _label;
    private Label _label2;
    private Timer _modeEndingTimer;
    private MotuPlayer _player;
    private int _superExtraScore;
    private int _superJackpot = MotuConstant._1M;
    private int _supersCollected;
    private int _totalBallsForMode;
    private long _totalScored;

    public override void _ExitTree()
    {
        base._ExitTree();
        _player.IsGrayskullMultiballRunning = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (pinGod.GameInPlay && !pinGod.IsTilted && _player.IsGrayskullMultiballRunning)
        {
            //Skeletor switches
            if (pinGod.SwitchOn("skele_l", @event))
            {
                OnAddABallHit();
            }
            if (pinGod.SwitchOn("skele_m", @event))
            {
                OnAddABallHit();
            }
            if (pinGod.SwitchOn("skele_r", @event))
            {
                OnAddABallHit();
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();

        _player = (pinGod as MotuPinGodGame).CurrentPlayer();
        _anims = GetNode<AnimatedSprite>("Control/GrayskullAnim");
        _control = GetNode<Control>("Control");
        _control.Visible = false;
        _label = _control.GetNode<Label>("Label");
        _label2 = _control.GetNode<Label>("Label2");

        _clearDisplayTimer = GetNode<Timer>("ClearDisplayTimer");
        _modeEndingTimer = GetNode<Timer>("ModeEndingTimer");

        _player.SetLadder("grayskull", MotuLadder.PartComplete);
        _player.IsGrayskullMultiballRunning = true;
        _player.IsGraySkullReady = false;
        _player.GrayskullProgress = new bool[6];

        _audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _defaultAudioStream = _audio.Stream;

        //lift targets in this multiball
        (pinGod as MotuPinGodGame)?.SkellyTargetsDown(false);

        pinGod.PlayMusic("base-music-bgm");

        //todo: sound `Sorceress_PowerOfEternia`

        if (_player.IsRipperModeEnding || _player.IsRipperMultiballRunning)
        {
            var ripMode = GetParent().GetNodeOrNull<MultiballRipper>("MultiballRipper");
            if (ripMode != null)
            {
                ripMode.RestartMultiball();
            }
            else
            {
                pinGod.LogWarning("tried to reset ripper multiball but it wasn't found");
            }

            pinGod.StartMultiBall(3, 20);
            pinGod.LogInfo("grayskull: restarting rippermode");
        }
        else
        {
            pinGod.StartMultiBall(2, 20);
            pinGod.LogInfo("grayskull: multiball running 2 balls");
        }
    }

    /// <summary>
    /// Reset the mode if it's ending after adding new ball
    /// </summary>
    public void ResetModeEnding()
    {
        _modeEndingTimer.Stop();
        _player.GrayskullModeEnded = false;
        _player.GrayskullModeEnding = false;
    }

    internal void CollectJackpot()
    {
        //score jackpot
        _audio.Stream = _defaultAudioStream;
        _audio.Play();
        var scored = pinGod.AddPoints(_jackpot);
        _totalScored += scored;
        pinGod.LogInfo("grayskull jackpot scored " + scored);
        pinGod.SolenoidOn("vpcoil", 5); //screwflashers

        //increment jackpot, max 500k
        if (_jackpot < MotuConstant._500K)
            _jackpot += 25000;
        else
            _jackpot = MotuConstant._500K;

        //add to super jackpot
        _superJackpot += _jackpot;

        DisplayJackpot(scored);
    }

    internal void CollectSuperJackpot()
    {
        var scored = pinGod.AddPoints(_superJackpot);
        _totalScored += scored;
        _supersCollected++;
        pinGod.PlaySfx("spin_sword");
        DisplayJackpot(scored, true);
        _superExtraScore = MotuConstant._250K * _supersCollected;
        _superJackpot = MotuConstant._1M + _superExtraScore;

        _player.SetLadder("grayskull", MotuLadder.Complete);
        pinGod.LogInfo("grayskull super jackpot scored " + scored);

        pinGod.SolenoidOn("vpcoil", 5); //screwflashers
    }
    protected override void OnBallDrained()
    {
        base.OnBallDrained();
    }

    void ClearDisplayTimer_timeout()
    {
        _control.Visible = false;
        _anims.Stop();
        pinGod.SolenoidOn("vpcoil", 0); //reset show
    }

    void DisplayJackpot(long score, bool isSuper = false)
    {
        var msg = isSuper ? "SUPER JACKPOT" : "JACKPOT ";
        PlaySequence(msg: msg, msg2: score.ToScoreString());
    }

    void EndMultiball()
    {
        _player.GrayskullModeEnding = true;
        _player.GrayskullModeEnded = false;
        _modeEndingTimer.Start(10);
        pinGod.LogInfo("grayskull: mode ending 10 seconds");
    }

    /// <summary>
    /// Turn off the multiball when time is up
    /// </summary>
    void ModeEndingTimer_timeout()
    {
        var game = GetParent().GetParent() as Game;
        if (_player.GrayskullModeEnding)
        {
            _player.IsGrayskullMultiballRunning = false;
            _player.GrayskullModeEnding = false;
            _player.GrayskullModeEnded= true;
            _modeEndingTimer.Start(2);
            PlaySequence(msg2: $"TOTAL SCORED {_totalScored.ToScoreString()}", delay: 3f);
            pinGod.LogInfo("grayskull: mode ending timed out");
            game?.PlayBackgroundMusic();
            game?.StartSorceress();
        }
        else
        {
            game.StartSorceress();     
            this.QueueFree();
        }
    }

    void OnAddABallHit()
    {
        if (_player.GrayskullModeEnded) return;

        if (_addABalls > 0)
        {
            //assign a shot target
            for (int i = 0; i < _addABallTargets.Length; i++)
            {
                if (!_addABallTargets[i])
                {
                    _addABallTargets[i] = true;
                    break;
                }
            }

            //are all shot targets complete?
            if (!_addABallTargets.Any(x => !x))
            {
                _addABallTargets = new bool[3];
                if (_addABalls >= 1)
                {
                    _addABalls--;
                    _totalBallsForMode++;
                    var ballsInPlay = pinGod.BallsInPlay();
                    pinGod.LogInfo("grayskullmball: ball added, ballsinplay: " + ballsInPlay);

                    //set limit to 4 ball multiball
                    ballsInPlay++;
                    if (ballsInPlay > 4)
                        ballsInPlay = 4;
                    pinGod.StartMultiBall((byte)(ballsInPlay), 14);

                    ResetModeEnding();
                    PlaySequence("give_ball", msg2: "BALL ADDED - " + _addABalls + " ADD-A-BALLS LEFT");
                    _audio.Stream = pinGod.AudioManager.Sfx["skele_always_heman"];
                    _audio.Play();
                }
            }
            else
            {
                if (_clearDisplayTimer.IsStopped())
                {
                    PlaySequence("add_ball", msg2: "SKELETOR TARGETS ADD-A-BALL");
                    _audio.Stream = pinGod.AudioManager.Sfx["skele_heman"];
                    _audio.Play();
                }
            }
        }
    }

    private void PlaySequence(string anim = "walk", string msg = "GRAYSKULL MULTIBALL", string msg2 = "", float delay = 2f)
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
}
