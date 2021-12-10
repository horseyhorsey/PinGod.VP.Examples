using Godot;

public class MsScoreMode : ScoreMode
{
    private Label multiplierLabel;
    MsPinGodGame game;
    public override void _EnterTree()
    {
        base._EnterTree();

        //get the game as MsPinGdGame to use the multiplier
        game = pinGod as MsPinGodGame;
        
        multiplierLabel = GetNode<Label>("ValueLabel");
    }

    /// <summary>
    /// Updates the multiplier text
    /// </summary>
    public override void OnScoresUpdated()
    {
        base.OnScoresUpdated();
        multiplierLabel.Text = game?.Multiplier.ToString();
    }
}
