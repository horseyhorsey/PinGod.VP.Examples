using Godot;
using static Godot.GD;

public class Attract : Node2D
{
	int _currentScene = 0;

	public override void _Input(InputEvent @event)
	{
		//Start button. See PinGod.vbs for Standard switches
		if (@event.IsActionPressed("sw" + GameGlobals.StartSwitchNum))
		{
			Print("attract: starting game");
			(GetNode("/root/GameGlobals") as GameGlobals)?.StartGame();
		}
	}

	public override void _Ready()
	{
		Print("Attract loaded");		
	}
	
	private void _on_Timer_timeout()
	{
		_currentScene++;
		if (_currentScene > 0)
		{
			(GetNode("CanvasLayer/TextLayerControl") as TextLayer).Visible = false;
			(GetNode("CanvasLayer/HighScores") as HighScores).Visible = true;
			_currentScene = -1;
		}			
		else
		{
			(GetNode("CanvasLayer/TextLayerControl") as TextLayer).Visible = true;
			(GetNode("CanvasLayer/HighScores") as HighScores).Visible = false;
		}
	}
}



