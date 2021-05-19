using Godot;
using static Godot.GD;

public class ServiceMenu : CanvasLayer
{
	private PinGodGame pinGodGame;

	public override void _Ready()
	{
		
	}

	public override void _EnterTree()
	{
		pinGodGame = GetNode("/root/PinGodGame") as PinGodGame;
	}

	public override void _Input(InputEvent @event)
	{		
		if (pinGodGame.SwitchOn("enter", @event))
		{	
		}
		if (pinGodGame.SwitchOn("up", @event))
		{
		}
		if (pinGodGame.SwitchOn("down", @event))
		{
		}
		if (pinGodGame.SwitchOn("exit", @event))
		{
			pinGodGame.EmitSignal("ServiceMenuExit");
			this.QueueFree();			
		}
	}
}
