using Godot;

/// <summary>
/// A bonus layer / mode. Display at end of ball. Bonus.tscn scene <para/>
/// Sends <see cref="PinGodGame.BonusEnded"/> signal on <see cref="_display_for_seconds"/> timeout
/// </summary>
public class SciFiBonus : Bonus
{
	[Export] float _countdown_interval = 0.6f;

	private SciFiPlayer player;
	int _totalBonus = 1000;

    public override void _EnterTree()
    {
        base._EnterTree();

    }

    public override void StartBonusDisplay()
	{
		_totalBonus = 1000;
		Visible = true;
		player = ((SciFiPinGodGame)pinGod).GetSciFiPlayer();
		pinGod.AddPoints(1000);
		_countdown_interval = 0.6f;
		StartTimerCountdown();
	}

	void StartTimerCountdown() => timer.Start(_countdown_interval);

    public override void OnTimedOut()
    {
		//finished counting bonus
		if (player.AlienBonus <= 0 && player.DefenderBonus <= 0)
		{
			pinGod.LogInfo("bonus: BonusEnded");
			timer.Stop();
			pinGod.EmitSignal("BonusEnded");
			this.Visible = false;
		}
		else
		{
			if (player.DefenderBonus > 0)
			{
				player.AddDefenderBonus(-1);
				((SciFiPinGodGame)pinGod).UpdateDefenderLamps();
			}
			else
			{
				player.AddAlienBonus(-1);
				((SciFiPinGodGame)pinGod).UpdateAlienLamps();
			}

			var points = 1000 * player.BonusMultiplier;
			_totalBonus += points;
			pinGod.AddPoints(points);
			label.Text = SetBonusText();

			pinGod.PlaySfx("bonus_counter");

			if (_countdown_interval > 0.050f)
			{
				_countdown_interval -= 0.050f;
			}
			StartTimerCountdown();
		}
	}

	/// <summary>
	/// Generate a simple bonus amount string
	/// </summary>
	/// <returns></returns>
	public override string SetBonusText()
	{
		var txt = "END OF BALL" + System.Environment.NewLine;
		txt += "DEFENDER AND ALIEN BONUS" + System.Environment.NewLine;
		txt += _totalBonus.ToString("N0");		
		return txt;
	}
}
