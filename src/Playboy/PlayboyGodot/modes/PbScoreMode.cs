/// <summary>
/// Simple score display mode for 4 players with ball information. Used in the <see cref="Game"/> Scene
/// </summary>
public class PbScoreMode : ScoreMode
{
	/// <summary>
	/// Update current player and ball
	/// </summary>
	public override void _Ready()
	{
		base._Ready();
		//GetNode<VideoPlayerPinball>("VideoPlayerPinball")?.Play();		
	}

	void OnBallStarted()
	{
		GetNode<VideoPlayerPinball>("VideoPlayerPinball")?.Play();		
	}

	void OnBallDrained() => GetNode<VideoPlayerPinball>("VideoPlayerPinball")?.Stop();

}
