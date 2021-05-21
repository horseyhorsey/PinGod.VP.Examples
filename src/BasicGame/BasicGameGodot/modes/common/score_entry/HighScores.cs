using Godot;
using System.Linq;

public class HighScores : Control
{
    private PinGodGame pinGod;
    private Label Label;

    public override void _EnterTree()
    {
        pinGod = GetNode("/root/PinGodGame") as PinGodGame;
        Label = GetNode("CenterContainer/VBoxContainer/Label") as Label;
    }

    /// <summary>
    /// Gets the top high scores from the <see cref="PinGodGame.GameData.HighScores"/> and sets the label
    /// </summary>
    public override void _Ready()
	{		
		var scores = string.Join("\n\r", pinGod.GameData.HighScores.Select(x => $"{x.Scores}    {x.Name}"));
        Label.Text = scores;
	}
}
