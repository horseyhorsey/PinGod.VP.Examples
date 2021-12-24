using Godot;
using System.Linq;

public class OrkoMode : PinGodGameMode
{
    private MotuPlayer _player;
    private int _modeTime;
    private long _orkoTargetAward;
    private Timer _timer;
    private AnimationPlayer _animPlayer;
    private AudioStreamPlayer _audio;
    private Game _game;

    protected override void OnBallStarted()
    {
        _player = (pinGod as MotuPinGodGame).CurrentPlayer();
        _player.IsFastScoringRunning = false;        
    }

    public override void _Ready()
    {
        base._Ready();
        _timer = GetNode<Timer>("Timer");
        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _game = GetParent().GetParent() as Game;
    }

    /// <summary>
    /// Score targets completed or starts fast scoring
    /// </summary>
    void CheckStartFastScoring()
    {
        if (_player.MotuLadderComplete["orko"] > MotuLadder.None && !_player.IsFastScoringRunning)
        {
            pinGod.LogInfo("orko already completed, checking");

            if(_player.OrkoTimesToComplete >= 0)
            {
                _player.OrkoTimesToComplete--;
                if(_player.OrkoTimesToComplete <= 0)
                {
                    _player.OrkoTimesToComplete = 4;
                    StartFastScoring();
                }

                _player.OrkoShots = new bool[4];
            }
        }
        else if(!_player.IsFastScoringRunning)
        {
            _player.SetLadder("orko", MotuLadder.PartComplete);
            StartFastScoring();
        }
        else if(_player.IsFastScoringRunning)
        {
            AwardRunningTargetScore();
        }
        else
        {
            pinGod.LogWarning("orko: shouldn't get to this spot");
        }
    }

    void AwardRunningTargetScore()
    {
        _player.SetLadder("orko", MotuLadder.Complete);
        pinGod.LogInfo("orko: fast scoring fully completed");
    }

    void Target_TargetComplete(int index)
    {
        _animPlayer.Play("just_orko");
    }

    void StartFastScoring() 
    {
        pinGod.LogInfo("orko: fast scoring started");
        _player.IsFastScoringRunning = true;
        _modeTime = 25;
        _timer?.Start();
        _animPlayer.Play("left_to_right");
        _audio.Play();
    }

    void TimerTimeout()
    {
        if(_modeTime >= 0)
        {
            _modeTime--;
        }
        else
        {
            pinGod.LogInfo("orko: mode timed out, resetting orko");
            _timer?.Stop();
            _player.IsFastScoringRunning = false;
            _player.ResetOrkoShots();
            _game.StartSorceress();
            
        }
    }

    protected override void OnBallDrained()
    {
        _timer?.Stop();
    }

    void OrkoTargets_OnTargetsCompleted()
    {
        pinGod.LogInfo("orko: targets completed");
        CheckStartFastScoring();
    }
}