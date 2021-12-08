public class KrScoreEntry : ScoreEntry
{
    public override void OnNewHighScore()
    {
        base.OnNewHighScore();

		pinGod.PlayMusic("boogie");
		pinGod.PlaySfx("DEVONhighscore");
	}
}
