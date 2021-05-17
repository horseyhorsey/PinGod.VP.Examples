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


		//signals
		GetNode("/root/GameGlobals").Connect("BallStarted", this, "OnScoresUpdated");
		GetNode("/root/GameGlobals").Connect("GameStarted", this, "OnScoresUpdated");
		GetNode("/root/GameGlobals").Connect("ScoresUpdated", this, "OnScoresUpdated");
		GetNode("/root/GameGlobals").Connect("PlayerAdded", this, "OnScoresUpdated");

		//update current player and ball
		OnScoresUpdated();
	}

	/// <summary>
	/// Set the labels text on signals.
	/// </summary>
	void OnScoresUpdated()
	{
		if (GameGlobals.Players?.Count > 0)
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

			if (GameGlobals.Players.Count > 1)
			{
				int i = 0;
				foreach (var player in GameGlobals.Players)
				{
					var lbl = ScoreLabels[i];
					if (lbl != null)
					{
						if (player.Points > -1)
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

			ballInfolabel.Text = $"BALL: {GameGlobals.BallInPlay}";
			//update current player and ball
			playerInfoLabel.Text = $"PLAYER: {GameGlobals.CurrentPlayerIndex + 1}";
		}
	}
}
