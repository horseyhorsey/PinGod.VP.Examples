using Godot;
using System;

/// <summary>
/// Simple score display mode for 4 players with ball information. Used in the <see cref="Game"/> Scene
/// </summary>
public class ScoreMode : Node2D
{
	#region Labels from Scene
	private Label currentScoreLabel;
	private Label playerInfoLabel;
	private Label ballInfolabel;
	Label[] ScoreLabels = new Label[4];
	#endregion
	
	/// <summary>
	/// Initialize all labels from the scene so we can update
	/// </summary>
	public override void _Ready()
	{
		currentScoreLabel = this.GetNode("CanvasLayer/ScoreMain") as Label;
		playerInfoLabel = this.GetNode("CanvasLayer/PlayerInfo") as Label;
		ballInfolabel = this.GetNode("CanvasLayer/BallInfo") as Label;
		for (int i = 0; i < ScoreLabels.Length; i++)
		{
			ScoreLabels[i] = this.GetNode($"CanvasLayer/ScoreP{i+1}") as Label;
			ScoreLabels[i].Text = string.Empty;
		}
	}

	/// <summary>
	/// Updates labels from variables set in <see cref="GameGlobals"/> .This isCalled every frame. 'delta' is the elapsed time since the previous frame. <para/>
	/// TODO: Check if to see if more efficient when updated on score change
	/// </summary>
	/// <param name="delta"></param>
	public override void _Process(float delta)
	{
		//main score display
		if (GameGlobals.Player.Points > -1 && currentScoreLabel != null)
		{
			currentScoreLabel.Text = GameGlobals.Player.Points.ToString();
		}
		else
		{
			currentScoreLabel.Text = null;
		}

		//Update player scores
		if (GameGlobals.Players.Count > 0)
		{
			int i = 0;
			foreach (var player in GameGlobals.Players)
			{
				var lbl = ScoreLabels[i];
				if (lbl != null)
				{
					if(player.Points > -1)
					{
						lbl.Text = player.Points.ToString("N0");
					}
					else
					{
						lbl.Text = null;
					}
				}
				i++;
			}
		}

		//update current player and ball
		playerInfoLabel.Text = $"PLAYER: {GameGlobals.CurrentPlayerIndex+1}";
		ballInfolabel.Text = $"BALL: {GameGlobals.BallInPlay}";
	}
}
