public class DropTargetsMode : PinballTargetsControl
{
    public override void _Ready()
    {
        pinGod.LogDebug($"DropTargetsMode are ready");        
    }

    public override bool SetTargetComplete(int index)
    {
        bool result = base.SetTargetComplete(index);
        UpdateLamps();
        pinGod.LogDebug($"drop index {index} target hit ", result);        
        return result;
    }

    public override bool CheckTargetsCompleted(int index)
    {
        bool result = base.CheckTargetsCompleted(index);
        pinGod.LogDebug($"drop targets are all completed? ", result);
        return result;
    }

    public override void TargetsCompleted(bool reset = true)
    {
        pinGod.LogDebug($"reset targets? ", reset);
        if (reset)
        {
            pinGod.SolenoidPulse("dt_reset");
        }
        base.TargetsCompleted(reset);

        UpdateLamps();
    }

    public override void UpdateLamps()
    {
        base.UpdateLamps();
    }
}
