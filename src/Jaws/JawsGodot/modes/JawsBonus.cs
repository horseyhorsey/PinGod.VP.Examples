using Godot;
using static Godot.GD;

/// <summary>
/// A bonus layer / mode. Display at end of ball. Bonus.tscn scene <para/>
/// Sends <see cref="PinGodGame.BonusEnded"/> signal on <see cref="_display_for_seconds"/> timeout
/// </summary>
public class JawsBonus : Control
{
	[Export] float _display_for_seconds = 5;

	private Timer timer;
	private Label bonus;
	private Label bonusBruce;
	private Label bonusBarrel;
	private Label bonusSkipper;
	private float _default_display_for_seconds;
    private TextureRect shark;
    private JawsPinGodGame pinGod;
	private JawsPlayer player;
	private long total;

	/// <summary>
	/// So the timer knows what frame to show
	/// </summary>
	int bonusIndex = 1;
	/// <summary>
	/// Awards the current player bonus and gets timer ref
	/// </summary>
	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as JawsPinGodGame;
		if (pinGod.Player != null)
		{
			pinGod.Player.Points += pinGod.Player.Bonus;
		}
		timer = GetNode("Timer") as Timer;

		bonus = GetNode("CenterContainer/VBoxContainer/bonus") as Label;
		bonusBruce = GetNode("CenterContainer/VBoxContainer/bruceybonus") as Label;
		bonusBarrel = GetNode("CenterContainer/VBoxContainer/barrelbonus") as Label;
		bonusSkipper = GetNode("CenterContainer/VBoxContainer/skipperbonus") as Label;
		_default_display_for_seconds = _display_for_seconds;

		shark = GetNode<TextureRect>("SharkFace");
	}

	public override void _Ready()
	{		
		if (!timer.IsStopped())
			timer.Stop();

		//test
		//StartBonusDisplay();
	}

	public void StartBonusDisplay()
	{
		shark.GetNodeOrNull<AnimationPlayer>(nameof(AnimationPlayer))?.Play("default_reverse");

		//test
		//player = new JawsPlayer() { Bonus = 2234, BonusBruce = 23324, BonusSkipper = 23214, BonusBarrel = 4454 };
		pinGod.DisableAllLamps();
		_display_for_seconds = _default_display_for_seconds;
		player = pinGod.GetJawsPlayer();
		pinGod.LogInfo("starting bonus display");

		total = player.Bonus + player.BonusBarrel + player.BonusBruce + player.BonusSkipper;
		pinGod.AddPoints((int)total, false);

		pinGod.AudioManager.StopMusic();
		pinGod.PlayMusic("complete");

		SetBonusText();
		timer.Start();
		Visible = true;
	}

	/// <summary>
	/// Generate a simple bonus amount string
	/// </summary>
	/// <returns></returns>
	private void SetBonusText()
	{
		pinGod.PlaySfx("bell");
		switch (bonusIndex)
		{
			case 1:		
				bonusSkipper.Text += $"\r\n{player.BonusSkipper.ToScoreString()}";
				bonusSkipper.Visible = true;
				break;
			case 2:
				bonusBarrel.Text += $"\r\n{player.BonusBarrel.ToScoreString()}";
				bonusBarrel.Visible = true;
				break;
			case 3:
				bonusBruce.Text += $"\r\n{player.BonusBruce.ToScoreString()}";
				bonusBruce.Visible = true;
				break;
			case 4:
				bonus.Text += $"\r\n{player.Bonus.ToScoreString()}";
				bonus.Text += $"\r\nTOTAL: {total.ToScoreString()}";
				bonus.Visible = true;
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// Bonus has times out. Hide the display and send <see cref="PinGodGame.BonusEnded"/>
	/// </summary>
	private void _on_Timer_timeout()
	{
		_display_for_seconds -= 1;
		bonusIndex++;
		//pinGod.LogInfo("bonus: timer ", _display_for_seconds);
		if (_display_for_seconds <= 0)
		{
			pinGod.StopMusic();
			pinGod.AddPoints(1000);
			HideAndRestLabels();
			pinGod.LogInfo("bonus: BonusEnded");
			timer.Stop();
			this.Visible = false;
			pinGod.EmitSignal("BonusEnded");
		}
		else
		{
			SetBonusText();
		}
	}

	private void HideAndRestLabels()
	{
		bonusIndex = 1;
		bonus.Visible = false; bonus.Text = "BONUS";
		bonusBruce.Visible = false; bonusBruce.Text = "BRUCEY BONUS";
		bonusBarrel.Visible = false; bonusBarrel.Text = "BARREL BONUS";
		bonusSkipper.Visible = false; bonusSkipper.Text = "SKIPPER BONUS";
	}
}
