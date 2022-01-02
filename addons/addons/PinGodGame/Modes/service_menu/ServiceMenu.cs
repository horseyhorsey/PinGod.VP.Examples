using Godot;
using Pingod.Local.resources;

public class ServiceMenu : Node
{
	/// <summary>
	/// The default label in the scene, assigned on scene Ready
	/// </summary>
    protected Label menuNameLabel;

    protected PinGodGame pinGod;
    public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
	}

    public override void _Input(InputEvent @event)
    {
        if (pinGod.SwitchOn("enter", @event)) { OnEnter(); }
        if (pinGod.SwitchOn("up", @event)) { OnUp(); }
        if (pinGod.SwitchOn("down", @event)) { OnDown(); }
        if (pinGod.SwitchOn("exit", @event)) { OnExit(); }
    }

    public override void _Ready()
    {
		menuNameLabel = GetNode("Label") as Label;
		menuNameLabel.Text = ResourceText.service_title;
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
	/// Fired with Exit switch. Emits "ServiceMenuExit" and removes from the scene
	/// </summary>
	public virtual void OnExit() 
	{
		pinGod.PlaySfx("exit");
		pinGod.EmitSignal("ServiceMenuExit");
		this.QueueFree();
	}

	/// <summary>
	/// Fired with Up switch.
	/// </summary>
	public virtual void OnUp() { pinGod.PlaySfx("enter"); }
}
