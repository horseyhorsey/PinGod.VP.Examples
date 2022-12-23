using Godot;

public class Switch
{
    public Switch(byte num) { this.Num = num; }
    public Switch(byte num, BallSearchSignalOption ballSearch) { this.Num = num; this.BallSearch = ballSearch; }
    public byte Num { get; set; }
    /// <summary>
    /// Ball search options
    /// </summary>
    public BallSearchSignalOption BallSearch { get; set; }
    public ulong Time { get; set; }

    public Switch()
    {
        Time = OS.GetSystemTimeMsecs();
    }

    /// <summary>
    /// Sets a switch manually, pushes an InputEventAction to the queue
    /// </summary>
    /// <param name="Pressed"></param>
    /// <returns></returns>
    public void SetSwitch(bool Pressed) => Input.ParseInputEvent(new InputEventAction() { Action = this.ToString(), Pressed = Pressed });

    /// <summary>
    /// Checks the current input event. IsActionPressed(sw+num)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public bool IsOn(InputEvent input) 
    { 
        bool active = input.IsActionPressed(ToString());
        if (active)
        {
            Time = OS.GetSystemTimeMsecs();
        }
        return active;
    }
    /// <summary>
    /// Checks the current input event. IsActionReleased(sw+num)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public bool IsOff(InputEvent input) {
        bool released = input.IsActionReleased(ToString());
        if (released)
        {
            Time = OS.GetSystemTimeMsecs();
        }
        return released;
    }
    /// <summary>
    /// Checks if On/Off
    /// </summary>
    /// <returns></returns>
    public bool IsOn() => Input.IsActionPressed(ToString());

    /// <summary>
    /// Time in milliseconds since switch used
    /// </summary>
    /// <returns></returns>
    public ulong TimeSinceChange()
    {
        if(Time > 0)
        {
            return OS.GetSystemTimeMsecs() - Time;
        }

        return 0;
    }
    /// <summary>
    /// The action. sw+SwitchNum
    /// </summary>
    /// <returns></returns>
    public override string ToString() => "sw" + Num;
}
