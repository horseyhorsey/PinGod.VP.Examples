using Godot;

/// <summary>
/// Simple score display mode for 4 players with ball information. Used in the <see cref="Game"/> Scene
/// </summary>
public class ScoreMode : Control
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

        currentScoreLabel = this.GetNode("ScoreMain") as Label;
        playerInfoLabel = this.GetNode("PlayerInfo") as Label;
        ballInfolabel = this.GetNode("BallInfo") as Label;
        for (int i = 0; i < ScoreLabels.Length; i++)
        {
            ScoreLabels[i] = this.GetNode($"ScoreP{i + 1}") as Label;
            ScoreLabels[i].Text = string.Empty;
        }
        //signals
        pinGod.Connect("BallStarted", this, "OnScoresUpdated");
        pinGod.Connect("GameStarted", this, "OnScoresUpdated");
        pinGod.Connect("ScoresUpdated", this, "OnScoresUpdated");
        pinGod.Connect("PlayerAdded", this, "OnScoresUpdated");
    }

    /// <summary>
    /// Update current player and ball
    /// </summary>
    public override void _Ready()
    {        
        CallDeferred(nameof(OnScoresUpdated));
    }

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
            ballInfolabel.Text = $"BALL: {PinGodGame.BallInPlay}";
            playerInfoLabel.Text = $"PLAYER: {PinGodGame.CurrentPlayerIndex + 1}";
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
