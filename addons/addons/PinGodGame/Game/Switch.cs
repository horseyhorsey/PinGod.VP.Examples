using Godot;

public class Switch
{
    public Switch(byte num) { this.Num = num; }
    public Switch(byte num, BallSearchSignalOption ballSearch) { this.Num = num; this.BallSearch = ballSearch; }
    public byte Num { get; set; }
    public BallSearchSignalOption BallSearch { get; set; }
    /// <summary>
    /// Checks the current input event. IsActionPressed(sw+num)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public bool IsOn(InputEvent input) => input.IsActionPressed(ToString());
    /// <summary>
    /// Checks the current input event. IsActionReleased(sw+num)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public bool IsOff(InputEvent input) => input.IsActionReleased(ToString());
    /// <summary>
    /// Checks if On/Off
    /// </summary>
    /// <returns></returns>
    public bool IsOn() => Input.IsActionPressed(ToString());
    /// <summary>
    /// The action. sw+SwitchNum
    /// </summary>
    /// <returns></returns>
    public override string ToString() => "sw" + Num;
}
