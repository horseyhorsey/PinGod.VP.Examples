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
    /// Switches to handle lane events
    /// </summary>
    [Export] string[] _lane_switches = new string[0];
    [Export] bool _logging_enabled = false;
    [Export] bool _reset_on_ball_started = true;
    #endregion

    bool[] _lanesCompleted;
    protected PinGodGameBase pinGod;

    public override void _EnterTree()
    {
        pinGod = GetNode("/root/PinGodGame") as PinGodGameBase;
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
    /// Sets up the <see cref="_lane_switches"/> with <see cref="_lanesCompleted"/>
    /// </summary>
    public override void _Ready()
    {
        if (_lane_switches == null)
        {
            GD.PushError("no rollover switches defined. removing mode");
            this.QueueFree();
        }
        else
        {
            _lanesCompleted = new bool[_lane_switches.Length];
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
        return complete;
    }

    public virtual bool LaneSwitchActivated(int i)
    {
        if (!_lanesCompleted[i])
        {
            _lanesCompleted[i] = true;
            if (_logging_enabled)
                GD.Print($"lane {i} complete");
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

        if (_logging_enabled)
            GD.Print("top_lanes: rot left: ", string.Join(",", _lanesCompleted));
    }

    public virtual void RotateLanesRight()
    {
        var lastNum = _lanesCompleted[_lanesCompleted.Length - 1];
        for (int i = _lanesCompleted.Length - 1; i > 0; i--)
        {
            _lanesCompleted[i] = _lanesCompleted[i - 1];
        }
        _lanesCompleted[0] = lastNum;

        if (_logging_enabled)
            GD.Print("top_lanes: rot right: ", string.Join(",", _lanesCompleted));
    }

    public virtual void UpdateLamps()
    {
        if (_lanesCompleted != null)
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
    }
}
