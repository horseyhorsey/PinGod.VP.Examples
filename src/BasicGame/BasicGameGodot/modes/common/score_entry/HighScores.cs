using Godot;
using System.Linq;

public class HighScores : Control
{
	/// <summary>
	/// Gets the top high scores from the <see cref="PinGodGame.GameData.HighScores"/> and sets the label
	/// </summary>
	public override void _Ready()
	{		
		var scores = string.Join("\n\r", PinGodGame.GameData.HighScores.Select(x => $"{x.Name} - {x.Scores}"));
		(GetNode("CenterContainer/VBoxContainer/Label") as Label).Text = scores;
	}
}
