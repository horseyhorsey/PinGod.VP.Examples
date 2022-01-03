using Godot;

/// <summary>
/// Simple score display mode for 4 players with ball information. Used in the <see cref="Game"/> Scene
/// </summary>
public class ScoreMode : Node
{
    /// <summary>
    /// Show Player ones ScoreP1 label if set to true. Normally in a pinball the scorep1 would not display with main score unless multi-player
    /// </summary>
    [Export] bool _single_player_p1_visible = false;

    [Export] bool _show_main_score_multiplayer = true;

    #region Node paths to select in scene
    [Export] NodePath _ballInfoLabel = null;
    [Export] NodePath _playerInfoLabel = null;
    [Export] NodePath _scoreLabel = null;
    [Export] NodePath _scoreLabelP1 = null;
    [Export] NodePath _scoreLabelP2 = null;
    [Export] NodePath _scoreLabelP3 = null;
    [Export] NodePath _scoreLabelP4 = null; 
    #endregion

    protected PinGodGame pinGod;

    #region Labels from Scene
    protected Label ballInfolabel;
    protected Label scoreLabel;
    protected Label playerInfoLabel;
    protected Label[] ScoreLabels = new Label[4];
    #endregion

    /// <summary>
    /// Connects to signals to update scores
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
        GetBallPlayerInfoLabels();
        GetPlayerScoreLabels();

        CallDeferred(nameof(OnScoresUpdated));
    }

    /// <summary>
    /// Assigns the given node paths to Labels. Ball, Player, Main Score
    /// </summary>
    public virtual void GetBallPlayerInfoLabels()
    {
        ballInfolabel = _ballInfoLabel == null ? null : GetNode<Label>(_ballInfoLabel);
        playerInfoLabel = _playerInfoLabel == null ? null : GetNode<Label>(_playerInfoLabel);
        scoreLabel = _scoreLabel == null ? null : GetNode<Label>(_scoreLabel);
    }

    /// <summary>
    /// Assigns the player score labels and resets the initial text of the label. 
    /// </summary>
    public virtual void GetPlayerScoreLabels()
    {
        ScoreLabels[0] = _scoreLabelP1 != null ? GetNode<Label>(_scoreLabelP1) : null;
        if (ScoreLabels[0] != null) ScoreLabels[0].Text = string.Empty;
        ScoreLabels[1] = _scoreLabelP2 != null ? GetNode<Label>(_scoreLabelP2) : null;
        if (ScoreLabels[1] != null) ScoreLabels[1].Text = string.Empty;
        ScoreLabels[2] = _scoreLabelP3 != null ? GetNode<Label>(_scoreLabelP3) : null;
        if (ScoreLabels[2] != null) ScoreLabels[2].Text = string.Empty;
        ScoreLabels[3] = _scoreLabelP4 != null ? GetNode<Label>(_scoreLabelP4) : null;
        if (ScoreLabels[3] != null) ScoreLabels[3].Text = string.Empty;
    }

    /// <summary>
    /// Updates all the labels text
    /// </summary>
    public virtual void OnScoresUpdated()
    {
        if (pinGod.Players?.Count > 0)
        {
            //main score display if "ScoreMain" available
            if (scoreLabel != null)
            {
                if (pinGod.Player.Points > -1)
                {
                    if(pinGod.Players.Count > 1 && !_show_main_score_multiplayer) 
                    {
                        scoreLabel.Text = null;
                    }
                    else
                    {
                        scoreLabel.Text = pinGod.Player.Points.ToScoreString();                        
                    }
                }
                else
                {
                    scoreLabel.Text = null;
                }
            }

            int i = 0;
            foreach (var player in pinGod.Players)
            {
                if (pinGod.Players.Count == 1 && i == 0 && !_single_player_p1_visible)
                {
                    i++;
                    continue;
                }
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

            //update current player and ball
            if (ballInfolabel != null)
            {                
                ballInfolabel.Text = Tr("BALL") + " " + pinGod.BallInPlay.ToString();
            }
            if (playerInfoLabel != null)
            {
                playerInfoLabel.Text = $"{Tr("PLAYER")}: {pinGod.CurrentPlayerIndex + 1}";
                ballInfolabel.Text = Tr("PLAYER") + " " + (pinGod.CurrentPlayerIndex + 1).ToString();
            }

        }
        else
        {
            //give default in case we have no players and ball
            if (scoreLabel != null)
                scoreLabel.Text = ((long)369000).ToScoreString();
            if (ballInfolabel != null)
                ballInfolabel.Text = $"BALL: 369";
            if (playerInfoLabel != null)
                playerInfoLabel.Text = $"PLAYER: 369";
        }
    }

    void OnBallStarted() => OnScoresUpdated();
}
