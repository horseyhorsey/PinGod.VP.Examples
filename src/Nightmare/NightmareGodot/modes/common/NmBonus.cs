using Godot;
using System.Collections.Generic;
using System.Linq;

public class NmBonus : Bonus
{
    private IEnumerable<string> _frames;
    private int? _frameCount;
    private int _currentFrame = 0;
    [Export] float _countDownDelay = 0.2f;

    public override void _Ready()
    {
        base._Ready(); //call to stop timer
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

        var bonus = pinGod.Player.Bonus;
        label.Text = $"BONUS 1X {bonus.ToScoreString()}";

        //don't apply multiplier if ball is running
        if (pinGod.IsBallStarted)
        {
            pinGod.AddPoints(pinGod.Player.Bonus);
        }
        else
        {
            //todo: show multiplier and bonus , then cycle the multipliers if higher than 1X
        }

        _frames = PinGodHelper.CreateBonusZeroCountdown(pinGod?.Player?.Bonus ?? 0);
        _frameCount = _frames?.Count() ?? 0;
        _currentFrame = 0;
        timer.Start(_countDownDelay);        

        
    }

    public override void OnTimedOut()
    {
        if(_frameCount > 0)
        {
            label.Text = $"SCORE BONUS\n{_frames.ElementAt(_currentFrame)}";
            _currentFrame++;
            pinGod.PlaySfx("snd_kicker");
        }        

        if (_currentFrame == _frameCount)
        {
            timer.Stop();
            this.Visible = false;
            pinGod.EmitSignal("BonusEnded");
        }
    }
}
