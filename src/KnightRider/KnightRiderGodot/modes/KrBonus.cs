/// <summary>
/// A bonus layer / mode. Display at end of ball. Bonus.tscn scene <para/>
/// Sends <see cref="PinGodGame.BonusEnded"/> signal on <see cref="_display_for_seconds"/> timeout
/// </summary>
public class KrBonus : Bonus
{
	public override void StartBonusDisplay()
	{
		base.StartBonusDisplay();
		pinGod.DisableAllLamps();
		pinGod.PlayMusic("kr_theme_end");
		pinGod.PlayTvScene("kitt_birds", "END OF BALL\nBONUS", _display_for_seconds, loop: false);		
	}

    public override void OnTimedOut()
    {
		pinGod.StopMusic();
		GetNode<VideoPlayerPinball>("VideoPlayerPinball")?.Stop();
		base.OnTimedOut();
	}
}
