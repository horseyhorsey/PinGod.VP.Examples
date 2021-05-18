using Godot;
using static Godot.GD;

public class ServiceMenu : CanvasLayer
{
	private GameGlobals globals;
	private bool isEnabled;

	public override void _Ready()
	{
		
	}

	public override void _EnterTree()
	{
		globals = GetNode("/root/GameGlobals") as GameGlobals;
	}

	public override void _Input(InputEvent @event)
	{		
		if (@event.IsActionPressed("sw7"))
		{
			Print("sw:Enter");			
		}
		if (@event.IsActionPressed("sw6"))
		{
			Print("sw:Up");
		}
		if (@event.IsActionPressed("sw5"))
		{
			Print("sw:Down");
		}
		if (@event.IsActionPressed("sw4"))
		{
			Print("sw:Exit");
			globals.EmitSignal("ServiceMenuExit");
			this.QueueFree();			
		}
	}
}
