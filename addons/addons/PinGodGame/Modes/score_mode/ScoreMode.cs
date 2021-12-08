using Godot;

/// <summary>
/// Simple score display mode for 4 players with ball information. Used in the <see cref="Game"/> Scene
/// </summary>
public class ScoreMode : Control
{
	protected PinGodGame pinGod;
	#region Labels from Scene
	protected Label currentScoreLabel;
	protected Label playerInfoLabel;
	protected Label ballInfolabel;
	public Label[] ScoreLabels = new Label[4];
	#endregion

	/// <summary>
	/// Initialize all labels from the scene so we can update
	/// </summary>
	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
	
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
		currentScoreLabel = this.GetNode("ScoreMain") as Label;
		playerInfoLabel = this.GetNode("PlayerInfo") as Label;
		ballInfolabel = this.GetNode("BallInfo") as Label;
		GetPlayerScoreLabels();
		CallDeferred(nameof(OnScoresUpdated));
	}

	void OnBallStarted() => OnScoresUpdated();

	public virtual void GetPlayerScoreLabels()
    {
		for (int i = 0; i < ScoreLabels.Length; i++)
		{
			ScoreLabels[i] = this.GetNode($"ScoreP{i + 1}") as Label;
			ScoreLabels[i].Text = string.Empty;
		}
	}

	/// <summary>
	/// Set the labels text on signals.
	/// </summary>
	public virtual void OnScoresUpdated()
	{
		pinGod.LogDebug("scores updated");
		if (pinGod.Players?.Count > 0)
		{
			//main score display if "ScoreMain" available
			if (currentScoreLabel != null)
            {				
				if (pinGod.Player.Points > -1)
				{
					currentScoreLabel.Text = pinGod.Player.Points.ToScoreString();
				}
				else
				{
					currentScoreLabel.Text = null;
				}
			}

			if (pinGod.Players.Count > 0)
			{
				pinGod.LogDebug("Players updated");
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
			if(ballInfolabel != null)
            {
				ballInfolabel.Text = $"BALL: {pinGod.BallInPlay}";
			}
			if(playerInfoLabel != null)
            {
				playerInfoLabel.Text = $"PLAYER: {pinGod.CurrentPlayerIndex + 1}";
			}
			
		}
		else
		{
			//give default in case we have no players and ball
			if(currentScoreLabel != null)
				currentScoreLabel.Text = ((long)369000).ToScoreString();
			if(ballInfolabel!=null)
				ballInfolabel.Text = $"BALL: 369";
			if(playerInfoLabel != null)
				playerInfoLabel.Text = $"PLAYER: 369";
		}
	}
}
