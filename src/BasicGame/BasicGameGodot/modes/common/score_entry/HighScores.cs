using Godot;
using System;
using System.Linq;

public class HighScores : Control
{
	public override void _Ready()
	{
		//Print("attract: starting game");
		//(GetNode("/root/GameGlobals") as GameGlobals);
		var scores = string.Join("\n\r", GameGlobals.GameData.HighScores.Select(x => $"{x.Name} - {x.Scores}"));
		(GetNode("VBoxContainer/Label") as Label).Text = scores;
	}

}
