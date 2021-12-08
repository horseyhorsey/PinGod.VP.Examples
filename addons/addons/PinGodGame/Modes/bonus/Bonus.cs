using Godot;

/// <summary>
/// A bonus layer / mode. Display at end of ball. Bonus.tscn scene <para/>
/// Sends <see cref="PinGodGame.BonusEnded"/> signal on <see cref="_display_for_seconds"/> timeout
/// </summary>
public class Bonus : Control
{
	[Export] protected int _display_for_seconds = 5;

	protected Label label;
	protected PinGodGame pinGod;
	protected Timer timer;
	/// <summary>
	/// Awards the current player bonus and gets timer ref
	/// </summary>
	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		if (pinGod.Player != null)
		{
			pinGod.Player.Points += pinGod.Player.Bonus;
		}
		timer = GetNode("Timer") as Timer;
		label = GetNode("Label") as Label;
	}

	public override void _Ready()
	{
		if (!timer.IsStopped())
			timer.Stop();
	}

	/// <summary>
	/// Starts display for the amount of seconds set
	/// </summary>
	public virtual void StartBonusDisplay()
	{
		label.Text = SetBonusText();
		pinGod.LogDebug("bonus: set label text to:", label.Text);
		timer.Start(_display_for_seconds);
		Visible = true;
	}

	public virtual void OnTimedOut()
    {
		pinGod.LogDebug("bonus: BonusEnded");
		timer.Stop();
		this.Visible = false;
		pinGod.EmitSignal("BonusEnded");
	}

	/// <summary>
	/// Bonus has times out. Hide the display and send <see cref="PinGodGame.BonusEnded"/>
	/// </summary>
	private void _on_Timer_timeout()
	{
		OnTimedOut();		
	}

	/// <summary>
	/// Generate a simple bonus amount string
	/// </summary>
	/// <returns></returns>
	public virtual string SetBonusText()
	{
		var txt = "END OF BALL" + System.Environment.NewLine;
		txt += "BONUS" + System.Environment.NewLine;
		txt += pinGod.Player?.Bonus.ToString("N0");
		return txt;
	}
}
