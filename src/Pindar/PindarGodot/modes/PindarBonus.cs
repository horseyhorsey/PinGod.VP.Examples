using Godot;

public class PindarBonus : Bonus
{

    private PindarPlayer _player;
    private Timer _timer;

    public override void _Ready()
    {        
        base._Ready();
    }

    long totalScored = 0;
    long bonusCount = 0;

    public override void StartBonusDisplay(bool visible = true)
    {
        pinGod.StopMusic();
        _timer = GetNode<Timer>("BonusTimer");
        _timer.Start(0.2f);

        totalScored = 0;

        pinGod.LogInfo("bonus pindar ready");
        _player = (pinGod as CustomPinGodGame).PindarPlayer;
        pinGod.LogInfo("player bonus count " + _player.BonusCount);
        _display_for_seconds = (0.2f) * (_player.BonusCount * _player.BonusMultiplier);

        bonusCount = _player.BonusCount;

        base.StartBonusDisplay(visible);
    }

    private void UpdateBonusLamps()
    {
        for (int i = 0; i < 11; i++)
        {
            pinGod.SetLampState($"b_{i + 1}", 0);
        }

        if (_player.BonusCount >= 20)
        {
            pinGod.SetLampState($"b_11", 1);
            var cnt = _player.BonusCount;
            while (cnt % 10 != 0)
            {
                pinGod.SetLampState($"b_{cnt % 10}", 1);
                cnt--;
            }
        }
        else
        {
            if (_player.BonusCount >= 10)
                pinGod.SetLampState($"b_10", 1);

            var cnt = _player.BonusCount;
            while (cnt % 10 != 0)
            {
                pinGod.SetLampState($"b_{cnt % 10}", 1);
                cnt--;
            }
        }
    }

    void BonusTimer_timeout()
    {
        pinGod.PlaySfx("bonus_stab", "Voice");
        totalScored += pinGod.AddPoints(Constant._1K, false);
        _player.BonusCount--;

        label.Text = $"PINDAR\nBONUS\n{totalScored.ToScoreString()}";
        UpdateBonusLamps();

        if(_player.BonusMultiplier > 1 && _player.BonusCount == 0)
        {
            _player.BonusMultiplier--;
            _player.BonusCount = (int)bonusCount;
        }

        if (_player.BonusCount == 0)
        {
            pinGod.AddPoints(0); //to update
            _timer.Stop();
        }            
    }
}