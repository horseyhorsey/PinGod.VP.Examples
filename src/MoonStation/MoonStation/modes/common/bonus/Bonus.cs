using Godot;
using static Godot.GD;

/// <summary>
/// A bonus layer / mode. Display at end of ball. Bonus.tscn scene <para/>
/// Sends <see cref="PinGodGame.BonusEnded"/> signal on <see cref="_display_for_seconds"/> timeout
/// </summary>
public class Bonus : Control
{
	[Export] int _display_for_seconds = 5;

	private Timer timer;
	private Label label;
	private PinGodGame pinGod;

	/// <summary>
	/// Awards the current player bonus and gets timer ref
	/// </summary>
	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		timer = GetNode("Timer") as Timer;
		label = GetNode("Label") as Label;
	}

	public override void _Ready()
	{		
		if (!timer.IsStopped())
			timer.Stop();		
	}

	public void StartBonusDisplay()
	{
		if (pinGod.Player != null)
		{
			pinGod.Player.Points += pinGod.Player.Bonus;
		}
		label.Text = SetBonusText();
		timer.Start(_display_for_seconds);
		Visible = true;
	}

	/// <summary>
	/// Generate a simple bonus amount string
	/// </summary>
	/// <returns></returns>
	private string SetBonusText()
	{		
		var txt = "END OF BALL" + System.Environment.NewLine;
		txt += "BONUS" + System.Environment.NewLine;
		txt += pinGod.Player?.Bonus.ToString("N0");		
		return txt;
	}

	/// <summary>
	/// Bonus has times out. Hide the display and send <see cref="PinGodGame.BonusEnded"/>
	/// </summary>
	private void _on_Timer_timeout()
	{
		Print("bonus: BonusEnded");
		timer.Stop();
		this.Visible = false;
		pinGod.EmitSignal("BonusEnded");	
	}
}
