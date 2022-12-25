using Godot;
using System.Linq;

/// <summary>
/// Display high scores from the saved <see cref="GameData.HighScores"/>
/// </summary>
public class HighScores : Control
{
	internal PinGodGame pinGod;
	private Label Label;

	/// <summary>
	/// Gets objects from scene
	/// </summary>
    public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		Label = GetNode("CenterContainer/VBoxContainer/Label") as Label;
	}

	/// <summary>
	/// Gets the top high scores from <see cref="pinGod"/> <see cref="GameData.HighScores"/> and sets the label with scores
	/// </summary>
	public override void _Ready()
	{		
		var scores = string.Join("\n\r", pinGod.GameData.HighScores.Select(x => $"{x.Scores.ToScoreString()}    {x.Name}"));
		Label.Text = scores;
	}
}
