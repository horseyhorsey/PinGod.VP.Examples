using Godot;
using static Godot.GD;

public class Attract : Node2D
{
	int _currentScene = 0;
	private PinGodGame pinGodGame;

	/// <summary>
	/// Checks for start button presses.
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (pinGodGame.SwitchOn("start", @event))
		{
			var started = pinGodGame.StartGame();
			if (started) { pinGodGame.SolenoidOn("disable_shows", 1); }
			Print("attract: starting game. started?", started);			
		}
	}

	public override void _Ready()
	{		
		pinGodGame = (GetNode("/root/PinGodGame") as PinGodGame);
		Print("Attract loaded");
		OscService.PulseCoilState(33, 100);
	}
	
	/// <summary>
	/// Just switches the scenes visibility on a timer
	/// </summary>
	private void _on_Timer_timeout()
	{
		_currentScene++;
		if (_currentScene > 0)
		{
			(GetNode("CanvasLayer/TextLayerControl") as TextLayer).Visible = false;
			(GetNode("CanvasLayer/HighScores") as HighScores).Visible = true;
			_currentScene = -1;

			//Play Lampshow in Visual pinball
			OscService.PulseCoilState(34, 100);
		}			
		else
		{
			(GetNode("CanvasLayer/TextLayerControl") as TextLayer).Visible = true;
			(GetNode("CanvasLayer/HighScores") as HighScores).Visible = false;

			//Play Lampshow in Visual pinball
			OscService.PulseCoilState(35, 100);
		}
	}
}
