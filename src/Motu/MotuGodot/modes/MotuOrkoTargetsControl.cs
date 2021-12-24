using Godot;

public class MotuOrkoTargetsControl : PinballTargetsControl
{
    private MotuPlayer _player;

    [Signal] public delegate int TargetComplete();

    void OnBallStarted()
    {
        _player = (pinGod as MotuPinGodGame).CurrentPlayer();
    }

    public override bool SetTargetComplete(int index)
    {
        if (_player.IsSorceressRunning) return false;
        if (_player.IsMotuMultiballRunning) return false;

        if (!_player.IsFastScoringRunning)
        {            
            var result = base.SetTargetComplete(index);
            if (result)
            {
                pinGod.AddPoints(MotuConstant._75K);
                EmitSignal(nameof(TargetComplete), index);
            }
            return result;
        }
        else if (_player.IsFastScoringRunning)
        {
            pinGod.AddPoints(MotuConstant._75K);
            return base.SetTargetComplete(index);
        }

        return false;
    }
}