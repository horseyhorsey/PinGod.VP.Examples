using Godot;
using System.Collections.Generic;
using System.Linq;

public class NmBonus : Bonus
{
    private IEnumerable<string> _frames;
    private int? _frameCount;
    private int _frameCountMultiplier = 1;
    private int _currentFrame = 0;
    private int _currentFrameCountMultiplier = 1;
    [Export] float _countDownDelay = 0.2f;
    private NightmarePlayer _player;

    public override void _Ready()
    {
        base._Ready(); //call to stop timer
        _player = pinGod?.Player as NightmarePlayer;
    }

    public override string SetBonusText(string text = "")
    {
        return text;
    }

    /// <summary>
    /// This normally invoked by the projects Game
    /// </summary>
    public override void StartBonusDisplay()
    {
        //base.StartBonusDisplay();
        pinGod.LogInfo("bonus: starting display. ball is started? " + pinGod.IsBallStarted);
        Visible = true;        

        var bonus = _player.Bonus;

        //don't apply multiplier if ball is running
        if (pinGod.IsBallStarted)
        {
            pinGod.AddPoints(bonus);
            pinGod.LogInfo("bonus awarded: " + bonus);
            timer.Start(_countDownDelay);
        }
        else
        {
            bonus = bonus * _player.Multiplier;
            pinGod.AddPoints(bonus, true);
            pinGod.LogInfo("bonus multi awarded: " + bonus);
            timer.Start(0.5f);
            //todo:extra ball, display REINCARNATE
        }

        //get count down frames
        _frames = PinGodHelper.CreateBonusZeroCountdown(bonus);
        _frameCount = _frames?.Count() ?? 0;
        _currentFrame = 0;
    }

    void SetMultiplierLabel(int mPlier, long bonus)
    {
        label.Text = $"BONUS {mPlier}X\n{(bonus * mPlier).ToScoreString()}";
    }

    public override void OnTimedOut()
    {
        if (!pinGod.IsBallStarted && _currentFrameCountMultiplier < _player.Multiplier)
        {            
            pinGod.PlaySfx("snd_lowsquirk");
            SetMultiplierLabel(_currentFrameCountMultiplier, _player.Bonus);
            _currentFrameCountMultiplier++;            
            return;
        }

        timer.Stop();
        timer.Start(0.1f);
        if (_frameCount > 0)
        {
            label.Text = $"SCORE BONUS\n{_frames.ElementAt(_currentFrame)}";
            _currentFrame++;
            pinGod.PlaySfx("snd_kicker");
        }

        if (_currentFrame == _frameCount)
        {
            timer.Stop();
            this.Visible = false;

            //don't want to send this signal if we're in a ball
            if (!pinGod.IsBallStarted)
                pinGod.EmitSignal("BonusEnded");
        }
    }
}
