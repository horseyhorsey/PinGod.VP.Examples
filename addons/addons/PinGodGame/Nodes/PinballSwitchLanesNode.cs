using Godot;

/// <summary>
/// Rollover lanes that are switched by flippers
/// </summary>
public abstract class PinballSwitchLanesNode : Node
{
    #region Exports
    [Export] bool _flipper_changes_lanes = true;
    /// <summary>
    /// Lamps to update
    /// </summary>
    [Export] string[] _lane_lamps = new string[0];
    /// <summary>
    /// Leds to update
    /// </summary>
    [Export] string[] _led_lamps = new string[0];
    /// <summary>
    /// Switches to handle lane events
    /// </summary>
    [Export] string[] _lane_switches = new string[0];

    [Export] protected Color _led_color = Color.Color8(255, 255, 255);

    [Export] bool _resetOnLanesCompleted = true;

    [Signal] public delegate void LanesCompleted();
    [Signal] public delegate int LaneCompleted();

    #endregion

    bool[] _lanesCompleted;
    protected PinGodGameBase pinGod;

    public override void _EnterTree()
    {
        pinGod = GetNode("/root/PinGodGame") as PinGodGameBase;

        if (_lane_switches == null)
        {
            pinGod.LogError("no rollover switches defined. removing mode");
            this.QueueFree();
        }
        else
        {
            _lanesCompleted = new bool[_lane_switches.Length];
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (_lane_switches != null)
        {
            if (_flipper_changes_lanes)
            {
                if (pinGod.SwitchOn("flipper_l", @event))
                {
                    RotateLanesLeft();
                    UpdateLamps();
                }
                if (pinGod.SwitchOn("flipper_r", @event))
                {
                    RotateLanesRight();
                    UpdateLamps();
                }
            }

            if (_lane_switches != null)
            {
                bool wasSet = false;
                for (int i = 0; i < _lane_switches.Length; i++)
                {
                    if (pinGod.SwitchOn(_lane_switches[i], @event))
                    {
                        wasSet = LaneSwitchActivated(i);
                    }
                }

                if (wasSet)
                {
                    CheckLanes();
                    UpdateLamps();                    
                }
            }
        }
    }

    /// <summary>
    /// Checks <see cref="_lanesCompleted"/>
    /// </summary>
    /// <returns></returns>
    public virtual bool CheckLanes()
    {
        bool complete = true;
        for (int i = 0; i < _lanesCompleted.Length; i++)
        {
            if (!_lanesCompleted[i])
            {
                complete = false;
                break;
            }
        }
        if (complete)
        {
            EmitSignal(nameof(LanesCompleted));
            if (_resetOnLanesCompleted) ResetLanesCompleted();
        }
        return complete;
    }

    /// <summary>
    /// Returns true if the lane was completed after this lane was activated
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public virtual bool LaneSwitchActivated(int i)
    {
        if (!_lanesCompleted[i])
        {
            _lanesCompleted[i] = true;
            pinGod.LogDebug($"lane {i} complete");
            EmitSignal(nameof(LaneCompleted), i);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Resets <see cref="_lanesCompleted"/>
    /// </summary>
    public virtual void ResetLanesCompleted() => _lanesCompleted = new bool[_lane_switches.Length];

    public virtual void RotateLanesLeft()
    {
        var firstNum = _lanesCompleted[0];
        for (int i = 0; i < _lanesCompleted.Length - 1; i++)
        {
            _lanesCompleted[i] = _lanesCompleted[i + 1];
        }
        _lanesCompleted[_lanesCompleted.Length - 1] = firstNum;

        pinGod.LogDebug("top_lanes: rot left: ", string.Join(",", _lanesCompleted));
    }

    public virtual void RotateLanesRight()
    {
        var lastNum = _lanesCompleted[_lanesCompleted.Length - 1];
        for (int i = _lanesCompleted.Length - 1; i > 0; i--)
        {
            _lanesCompleted[i] = _lanesCompleted[i - 1];
        }
        _lanesCompleted[0] = lastNum;

        pinGod.LogDebug("top_lanes: rot right: ", string.Join(",", _lanesCompleted));
    }

    public virtual void UpdateLamps()
    {
        if (_lanesCompleted != null)
        {
            if(_lane_lamps?.Length > 0)
            {
                for (int i = 0; i < _lanesCompleted.Length; i++)
                {
                    if (_lanesCompleted[i])
                    {
                        pinGod.SetLampState(_lane_lamps[i], 1);
                    }
                    else
                    {
                        pinGod.SetLampState(_lane_lamps[i], 0);
                    }
                }
            }

            if (_led_lamps?.Length > 0)
            {
                for (int i = 0; i < _lanesCompleted.Length; i++)
                {
                    if (_lanesCompleted[i])
                    {
                        pinGod.SetLedState(_led_lamps[i], 1, _led_color);
                    }
                    else
                    {
                        pinGod.SetLedState(_led_lamps[i], 0, _led_color);
                    }
                }
            }
        }
    }
}
