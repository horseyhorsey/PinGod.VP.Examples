using Godot;
using System;

/// <summary>
/// Tool: Kicker / Saucer node based on timer and to process switch actions.
/// </summary>
[Tool]
public class BallStackPinball : Timer
{
    /// <summary>
    /// Coil name
    /// </summary>
    [Export] public string _coil = null;

    /// <summary>
    /// Switch name to listen for active/inactive
    /// </summary>
    [Export] string _switch = null;

    private PinGodGame pingod;

    /// <summary>
    /// Emitted when switch is on
    /// </summary>
    [Signal] public delegate void SwitchActive();

    /// <summary>
    /// Emitted when switch is off
    /// </summary>
    [Signal] public delegate void SwitchInActive();

	#region Public Methods
	/// <summary>
	/// Creates the timer and adds to the tree
	/// </summary>
	public override void _EnterTree()
	{
		if (!Engine.EditorHint)
		{
			pingod = GetNode("/root/PinGodGame") as PinGodGame;
			// Code to execute when in game.
		}
	}

    /// <summary>
    /// Stops the timer
    /// </summary>
    public override void _ExitTree()
    {
        Stop();
    }

    /// <summary>
    /// Emits <see cref="SwitchActive"/> or <see cref="SwitchInActive"/>
    /// </summary>
    /// <param name="event"></param>
    public override void _Input(InputEvent @event)
    {
        if (pingod.SwitchOn(_switch, @event))
        {
            EmitSignal(nameof(SwitchActive));
        }
        else if (pingod.SwitchOff(_switch, @event))
        {
            EmitSignal(nameof(SwitchInActive));
        }
    }

    /// <summary>
    /// Turns off SetProcessInput to process no switches if they haven't been set
    /// </summary>
    public override void _Ready()
    {
		if (!Engine.EditorHint)
		{
			// Code to execute when in game.
			if (string.IsNullOrWhiteSpace(_switch))
			{
				pingod.LogError("no _sw_action set", this.Name);
				SetProcessInput(false);
			}
		}
	}

    /// <summary>
    /// Uses Pingod <see cref="PinGodGame.SolenoidPulseTimer"/>
    /// </summary>
    public void SolenoidPulse() => pingod.SolenoidPulseTimer(_coil);

    #endregion
}
