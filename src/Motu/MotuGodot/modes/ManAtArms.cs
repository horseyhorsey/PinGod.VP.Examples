using Godot;
using System.Linq;

public class ManAtArms : PinGodGameMode
{
    private AnimatedSprite _anims;
    private AudioStreamPlayer _audio;
    private Game _game;
    private Timer _clearDisplyTimer;
    private Control _control;
    private Label _label;
    private PinballSwitchLanesNode _laneChangeScene;
    [Export] AudioStream _laneSfx;
    [Export] Godot.Collections.Array<AudioStream> _manCallouts;
    private AnimatedSprite _manSprite;
    int _modeTime = 25;
    internal Timer _modeTimer;
    private MotuPlayer _player;

    public override void _Ready()
    {
        base._Ready();
        pinGod.LogInfo("manatarms ready");
        _laneChangeScene = GetNode<PinballSwitchLanesNode>("LaneChange");

        _modeTimer = GetNode<Timer>("ModeTimer");
        _manSprite = GetNode<AnimatedSprite>("ManSprite");
        _manSprite.Visible = false;

        _control = GetNode<Control>("Control");
        _control.Visible = false;
        _anims = _control.GetNode<AnimatedSprite>("Anims");
        _label = _control.GetNode<Label>("Label");
        _clearDisplyTimer = GetNode<Timer>("ClearDisplayTimer");

        _audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

        _game = GetParent().GetParent() as Game;
    }

    protected override void OnBallDrained()
    {
        if (_player.IsManAtArmsRunning)
        {
            _player.IsManAtArmsRunning = false;
            _player.IsManAtArmsReady = false;
            _modeTime = 25;
        }
    }

    protected override void OnBallStarted()
    {
        //get the player
        _player = ((MotuPinGodGame)pinGod)?.CurrentPlayer() ?? new MotuPlayer();
    }

    protected override void UpdateLamps()
    {
        if (_player.IsManAtArmsRunning)
        {
            if (!_player.ManAtArmsProgress[0]) pinGod.SetLampState("man_left_loop", 2);
            else pinGod.SetLampState("man_left_loop", 1);
            if (!_player.ManAtArmsProgress[1]) pinGod.SetLampState("man_inner_spin", 2);
            else pinGod.SetLampState("man_inner_spin", 1);
            if (!_player.ManAtArmsProgress[2]) pinGod.SetLampState("man_r_loop", 2);
            else pinGod.SetLampState("man_r_loop", 1);
            if (!_player.ManAtArmsProgress[3]) pinGod.SetLampState("man_scoop", 2);
            else pinGod.SetLampState("man_scoop", 1);
        }
        else
        {
            if (_player.IsManAtArmsReady) pinGod.SetLampState("man_scoop", 2);
            else pinGod.SetLampState("man_scoop", 0);

            pinGod.SetLampState("man_left_loop", 0);
            pinGod.SetLampState("man_inner_spin", 0);
            pinGod.SetLampState("man_r_loop", 0);
        }        
    }
    public long AwardCurrentLitScore()
    {
        var scored = pinGod.AddPoints(_player.ManAtArmsRunningScore);
        _player.ManAtArmsRunningScore += MotuConstant._75K;
        return scored;
    }

    private void AwardManAtArmsMode()
    {
        _player.ResetRunningManAtArms();
        _player.SetLadder("manatarms", MotuLadder.Complete);
        pinGod.LogInfo("manatarms: ladder complete");

        _anims.Animation = "complete"; _anims.Frame = 0;
        _anims.Play();
        _label.Text = "MAN AT ARMS COMPLETE";
        _clearDisplyTimer.Start(2);
        _control.Visible = true;

        //play justintime
        _audio.Stream = _manCallouts[2];
        _audio.Play();

        _game?.StartSorceress();
    }

    public void CheckRunningProgressComplete(long score)
    {
        if (!_player.ManAtArmsProgress.Any(x => !x))
        {
            AwardManAtArmsMode();
        }
        else
        {
            DisplayManRunningCollected(score);
        }

        UpdateLamps();
    }

    void ClearDisplayTimer_timeout()
    {
        _control.Visible = false;
        _anims.Stop();
    }
    void DisplayManCollected(char letter)
    {
        _manSprite.Frame = 0;
        _manSprite.Play();
        _manSprite.Visible = true;
    }

    private void DisplayManRunningCollected(long score)
    {
        pinGod.LogInfo("manatarms running: progressed");
        _anims.Animation = "more_work"; _anims.Frame = 0;
        _anims.Play();

        //random man audio sound
        var num = new Godot.RandomNumberGenerator().RandiRange(0, 1);
        _audio.Stream = _manCallouts[num];
        _audio.Play();

        _label.Text = "MAN-AT-ARMS " + score.ToScoreString();
        _clearDisplyTimer.Start(2);
        _control.Visible = true;
    }

    void LaneChange_LaneCompleted(int index)
    {
        //if (_player.IsMotuMultiballRunning) return;

        pinGod.LogInfo("man at arms lane complete: " + index);
        pinGod.AddPoints(MotuConstant._10K);

        //play sfx on the lane when no multiball running
        if (!pinGod.IsMultiballRunning)
        {
            _audio.Stream = _laneSfx;
            _audio.Play();
        }

        //pop up manatarms
        if (!pinGod.IsMultiballRunning)
        {
            switch (index)
            {
                case 0:
                    DisplayManCollected('M');
                    break;
                case 1:
                    DisplayManCollected('A');
                    break;
                case 2:
                    DisplayManCollected('N');
                    break;
                default:
                    break;
            }
        }
    }

    void LaneChange_LanesCompleted()
    {
        //if (_player.IsMotuMultiballRunning) return;

        pinGod.LogInfo("man at arms lanes completed");
        _laneChangeScene.ResetLanesCompleted();
        _player.ManAtArmsBonusLevel += MotuConstant._5K;

        if (!_player.IsManAtArmsRunning && !_player.IsManAtArmsReady)
        {
            //open the scoop start shot if level 0
            if (_player.ManAtArmsLevel == 0)
            {
                _player.IsManAtArmsReady = true;
                ScoreManBonus();
                _player.ManAtArmsLevel = 3;
                pinGod.LogInfo("manatarms set scoop shot ready, next level set to 3 to open");
            }
            else if (_player.ManAtArmsLevel > 0)
            {
                _player.ManAtArmsLevel--;
                pinGod.LogInfo("lowering ManAtArmsLevel " + _player.ManAtArmsLevel);
            }
        }
    }

    void ManSprite_animation_finished()
    {
        _manSprite.Stop();
        _manSprite.Visible = false;
    }

    void ModeTimer_timeout()
    {
        if (_modeTime <= 0)
        {
            _modeTimer.Stop();
            _player.IsManAtArmsRunning = false;
            _player.IsManAtArmsReady = false;
            _modeTime = 25;
            pinGod.LogInfo("manatarms: mode timed out");
            _game?.StartSorceress();
        }
        else
        {
            //todo: display label countdown
            _modeTime--;
        }
    }

    private void ScoreManBonus()
    {
        pinGod.AddPoints(_player.ManAtArmsBonusLevel);
    }
}
