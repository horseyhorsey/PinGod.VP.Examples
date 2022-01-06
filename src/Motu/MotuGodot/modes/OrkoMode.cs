using Godot;

public class OrkoMode : PinGodGameMode
{
    private AnimationPlayer _animPlayer;
    private AudioStreamPlayer _audio;
    private Game _game;
    private int _modeTime;
    private MotuPlayer _player;
    private Timer _timer;
    public override void _Ready()
    {
        base._Ready();
        _timer = GetNode<Timer>("Timer");
        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _game = GetParent().GetParent() as Game;
    }

    protected override void OnBallDrained()
    {
        _timer?.Stop();
    }

    protected override void OnBallStarted()
    {
        _player = (pinGod as MotuPinGodGame).CurrentPlayer();
        _player.IsFastScoringRunning = false;        
    }
    void _on_PinballTargetsBank_OnTargetActivated(string swName, bool completed)
    {
        if (_player.IsSorceressRunning) return;
        if (_player.IsMotuMultiballRunning) return;

        if (!_player.IsFastScoringRunning)
        {
            if (completed)
            {
                pinGod.AddPoints(MotuConstant._75K);
                _animPlayer.Play("just_orko");
            }
        }
        else if (_player.IsFastScoringRunning)
        {
            pinGod.AddPoints(MotuConstant._75K);
        }
    }

    void _on_PinballTargetsBank_OnTargetsCompleted()
    {
        pinGod.LogInfo("orko: targets completed");
        CheckStartFastScoring();
    }

    void AwardRunningTargetScore()
    {
        _player.SetLadder("orko", MotuLadder.Complete);
        pinGod.LogInfo("orko: fast scoring fully completed");
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
}