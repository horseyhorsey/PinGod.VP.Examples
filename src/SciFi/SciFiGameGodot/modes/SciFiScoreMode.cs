using Godot;

public class SciFiScoreMode : ScoreMode
{    
    public override void GetBallPlayerInfoLabels()
    {
        this.scoreLabel = GetNode<Label>("CenterContainer/VBoxContainer/ScoreMain");
        this.playerInfoLabel = GetNode<Label>("PlayerInfo");
        this.ballInfolabel = GetNode<Label>("BallInfo");
    }
}
