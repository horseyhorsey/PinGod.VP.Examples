using Godot;
using static Godot.GD;

/// <summary>
/// Simple score entry: Sends <see cref="GameGlobals.ScoreEntryEnded"/> TODO entries, placeholder
/// </summary>
public class ScoreEntry : CanvasLayer
{
	/// <summary>
	/// Needs a timer to clear the screen just to go back to attract if players are idle
	/// </summary>
	[Export] int _timeout_seconds = 30;

	int currentTime;
	private Label timeoutLabel;

	public override void _Ready()
	{
		Print("score entry timeout:", _timeout_seconds);
		timeoutLabel = GetNode("TimeLabel") as Label;
		currentTime = _timeout_seconds;		
		timeoutLabel.Text = currentTime.ToString();
	}

	private void _on_TimeOut_timeout()
	{
		// Replace with function body.
		currentTime--;
		timeoutLabel.Text = currentTime.ToString();
		if (currentTime <= 0)
		{
			GetNode("/root/GameGlobals").EmitSignal("ScoreEntryEnded");
			this.QueueFree();
		}			
	}
}
