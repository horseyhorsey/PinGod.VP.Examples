using Godot;
using System;
using static Godot.GD;

/// <summary>
/// A bonus layer / mode. Display at end of ball. Bonus.tscn scene <para/>
/// Sends <see cref="GameGlobals.BonusEnded"/> signal on <see cref="_display_for_seconds"/> timeout
/// </summary>
public class Bonus : CanvasLayer
{
	private Timer timer;

	[Export] int _display_for_seconds = 5;

	public override void _Ready()
	{
		GameGlobals.Player.Points += GameGlobals.Player.Bonus;

		timer = GetNode("Timer") as Timer;
		if (!timer.IsStopped())
			timer.Stop();

		var label = GetNode("Label") as Label;
		label.Text = SetBonusText();
		timer.Start(_display_for_seconds);
	}

	/// <summary>
	/// Generate a simple bonus amount string
	/// </summary>
	/// <returns></returns>
	private string SetBonusText()
	{		
		var txt = "END OF BALL" + System.Environment.NewLine;
		txt += "BONUS" + System.Environment.NewLine;
		txt += GameGlobals.Player.Bonus.ToString("N0");		
		return txt;
	}

	private void _on_Timer_timeout()
	{
		Print("bonus: sending BonusEnded");
		GetNode("/root/GameGlobals").EmitSignal("BonusEnded");		
		this.QueueFree();
	}
}
