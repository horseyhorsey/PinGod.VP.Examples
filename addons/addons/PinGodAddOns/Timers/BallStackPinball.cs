using Godot;
using System;

/// <summary>
/// Kicker / Saucer node based on timer and process switch action
/// </summary>
[Tool]
public class BallStackPinball : Timer
{

	[Export] string _sw_action = null;
	[Signal] public delegate void SwitchActive();
	[Signal] public delegate void SwitchInActive();

	#region Public Methods
	/// <summary>
	/// Creates the timer and adds to the tree
	/// </summary>
	public override void _EnterTree()
	{
		if (!Engine.EditorHint)
		{
			// Code to execute when in game.		
		}
	}

    public override void _Ready()
    {
		if (!Engine.EditorHint)
		{
			// Code to execute when in game.
			if (string.IsNullOrWhiteSpace(_sw_action))
			{
				GD.PrintErr("no _sw_action set", this.Name);
				SetProcessInput(false);
			}
		}
	}
    public override void _Input(InputEvent @event)
    {
		if (@event.IsActionPressed(_sw_action))
		{
			EmitSignal(nameof(SwitchActive));
		}
		if (@event.IsActionReleased(_sw_action))
		{
			EmitSignal(nameof(SwitchInActive));
		}
	}

    /// <summary>
    /// Stops the timer
    /// </summary>
    public override void _ExitTree()
	{
		Stop();
	}

	#endregion
}
