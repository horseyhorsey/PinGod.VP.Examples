using Godot;

/// <summary>
/// Simple score display mode for 4 players with ball information. Used in the <see cref="Game"/> Scene
/// </summary>
public class PbScoreMode : Control
{
	private PinGodGame pinGod;
	#region Labels from Scene
	private Label currentScoreLabel;
	private Label playerInfoLabel;
	private Label ballInfolabel;
	Label[] ScoreLabels = new Label[4];
	#endregion

	/// <summary>
	/// Initialize all labels from the scene so we can update
	/// </summary>
	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;

		currentScoreLabel = this.GetNode("CenterContainer/ScoreMain") as Label;
		playerInfoLabel = this.GetNode("GameInfo/PlayerInfo") as Label;
		ballInfolabel = this.GetNode("GameInfo/BallInfo") as Label;
		for (int i = 0; i < ScoreLabels.Length; i++)
		{
			ScoreLabels[i] = this.GetNode($"Scores/ScoreP{i + 1}") as Label;
			ScoreLabels[i].Text = string.Empty;
		}
		//signals
		pinGod.Connect("GameStarted", this, "OnScoresUpdated");
		pinGod.Connect("ScoresUpdated", this, "OnScoresUpdated");
		pinGod.Connect("PlayerAdded", this, "OnScoresUpdated");
	}

	/// <summary>
	/// Update current player and ball
	/// </summary>
	public override void _Ready()
	{
		GetNode<VideoPlayerPinball>("VideoPlayerPinball")?.Play();
		CallDeferred(nameof(OnScoresUpdated));
	}

	void OnBallStarted()
	{
		GetNode<VideoPlayerPinball>("VideoPlayerPinball")?.Play();
		OnScoresUpdated();
	}

	void OnBallDrained() => GetNode<VideoPlayerPinball>("VideoPlayerPinball")?.Stop();

	/// <summary>
	/// Set the labels text on signals.
	/// </summary>
	void OnScoresUpdated()
	{
		if (pinGod.Players?.Count > 0)
		{
			//main score display
			if (pinGod.Player.Points > -1 && currentScoreLabel != null)
			{
				currentScoreLabel.Text = pinGod.Player.Points.ToScoreString();
			}
			else
			{
				currentScoreLabel.Text = null;
			}

			if (pinGod.Players.Count > 1)
			{
				int i = 0;
				foreach (var player in pinGod.Players)
				{
					var lbl = ScoreLabels[i];
					if (lbl != null)
					{
						if (player.Points > -1)
						{
							lbl.Text = player.Points.ToScoreString();
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
			ballInfolabel.Text = $"BALL: {pinGod.BallInPlay}";
			playerInfoLabel.Text = $"PLAYER: {pinGod.CurrentPlayerIndex + 1}";
		}
		else
		{
			//give default in case we have no players and ball
			currentScoreLabel.Text = ((long)369000).ToScoreString();
			ballInfolabel.Text = $"BALL: 369";
			playerInfoLabel.Text = $"PLAYER: 369";
		}
	}
}
