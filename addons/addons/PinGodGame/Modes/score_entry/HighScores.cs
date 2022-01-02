using Godot;
using Pingod.Local.resources;
using System.Linq;

public class HighScores : Control
{
	private PinGodGame pinGod;
	private Label Label;

    public Label Label2 { get; private set; }

    public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		Label = GetNode("CenterContainer/VBoxContainer/Label") as Label;
		Label2 = GetNode("CenterContainer/VBoxContainer/Label2") as Label;
	}

	/// <summary>
	/// Gets the top high scores from the <see cref="PinGodGame.GameData.HighScores"/> and sets the label
	/// </summary>
	public override void _Ready()
	{		
		var scores = string.Join("\n\r", pinGod.GameData.HighScores.Select(x => $"{x.Scores.ToScoreString()}    {x.Name}"));
		Label.Text = scores;
		Label2.Text = ResourceText.high_score_title;
	}
}
