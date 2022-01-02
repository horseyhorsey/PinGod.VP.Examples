using Godot;

public class GrottoScene : Control
{
	public int ScoreToDisplay { get; set; } = 25000; //set a default
	public int BonusAdvanced { get; set; } = 1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var txt = $"{ScoreToDisplay.ToScoreString()}\nADVANCED BONUS * {BonusAdvanced}";
		GetNode<BlinkingLabel>("BlinkingLabel").Text = txt;		
	}

	private void _on_Timer_timeout()
	{
		(GetNode("/root/PinGodGame") as PinGodGame)?.SolenoidPulse("saucer");
		AudioServer.SetBusEffectEnabled(1, 0, false);
		//(GetNode("/root/PinGodGame") as PinGodGame)?.PlayMusic("cook_loop_1");
		// Replace with function body.
		this.QueueFree();
	}
}
