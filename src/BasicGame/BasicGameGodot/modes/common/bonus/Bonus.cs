using Godot;
using System;
using static Godot.GD;

/// <summary>
/// A bonus layer / mode. Display at end of ball. Bonus.tscn scene <para/>
/// Sends <see cref="PinGodGame.BonusEnded"/> signal on <see cref="_display_for_seconds"/> timeout
/// </summary>
public class Bonus : Control
{
	private Timer timer;

	[Export] int _display_for_seconds = 5;

	public override void _Ready()
	{
		if(PinGodGame.Player != null)
		{
			PinGodGame.Player.Points += PinGodGame.Player.Bonus;
		}		

		timer = GetNode("Timer") as Timer;
		if (!timer.IsStopped())
			timer.Stop();		
	}

	public void StartBonusDisplay()
	{
		var label = GetNode("Label") as Label;
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
		txt += PinGodGame.Player?.Bonus.ToString("N0");		
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
		GetNode("/root/PinGodGame").EmitSignal("BonusEnded");	
	}
}
