/// <summary>
/// A base mode added to the groups named Mode. GamePlay events
/// </summary>
public abstract class PinGodGameMode : PinGodGameNode
{
    /// <summary>
    /// adds the mode to a group named Mode
    /// </summary>
    public override void _Ready()
    {
        base._Ready();

        AddToGroup("Mode");
    }

    protected virtual void OnBallDrained() { }
    protected virtual void OnBallSaved() { }
    protected virtual void OnBallStarted() { }
    protected virtual void UpdateLamps() { }

}