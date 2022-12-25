using Godot;

/// <summary>
/// Inherits from Node, is a mode
/// </summary>
public class ServiceMenu : Node
{
	/// <summary>
	/// The default label in the scene, assigned on scene Ready
	/// </summary>
    protected Label menuNameLabel;

	/// <summary>
	/// Pingod game reference
	/// </summary>
    protected PinGodGame pinGod;

	/// <summary>
	/// Just gets a reference to <see cref="pinGod"/>, this should be invoked if overriding the method
	/// </summary>
    public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
	}

	/// <summary>
	/// Checks for coin door switches, menus. enter, up , down and exit
	/// </summary>
	/// <param name="event"></param>
    public override void _Input(InputEvent @event)
    {
        if (pinGod.SwitchOn("enter", @event)) { OnEnter(); }
        if (pinGod.SwitchOn("up", @event)) { OnUp(); }
        if (pinGod.SwitchOn("down", @event)) { OnDown(); }
        if (pinGod.SwitchOn("exit", @event)) { OnExit(); }
    }

    /// <summary>
    /// Gets the label in the center of screen. <see cref="menuNameLabel"/>
    /// </summary>
    public override void _Ready()
    {
		menuNameLabel = GetNode("CenterContainer/Label") as Label;
	}
	/// <summary>
	/// Fired with Down switch.
	/// </summary>
	public virtual void OnDown() { pinGod.PlaySfx("enter"); }

	/// <summary>
	/// Fired with Enter switch.
	/// </summary>
	public virtual void OnEnter() { pinGod.PlaySfx("enter"); }

    /// <summary>
    /// Fired with Exit switch. Emits "ServiceMenuExit" and removes from the scene, plays "exit" sfx
    /// </summary>
    public virtual void OnExit() 
	{
		pinGod.PlaySfx("exit");
		pinGod.EmitSignal("ServiceMenuExit");
		this.QueueFree();
	}

	/// <summary>
	/// Fired with Up switch, plays "enter" sfx
	/// </summary>
	public virtual void OnUp() { pinGod.PlaySfx("enter"); }
}
