using Godot;

/// <summary>
/// Basic credit Label to listen for Coin events to update the scene <para/>
/// CreditsLayer.tscn
/// </summary>
public class CreditsLayer : Label
{
	private PinGodGame pingod;

	public override void _Ready()
	{
		//update when the credit changes, when added and players added
		pingod = GetNode("/root/PinGodGame") as PinGodGame;
		GetNode("/root/PinGodGame").Connect("CreditAdded", this, "OnCreditsUpdated");
		GetNode("/root/PinGodGame").Connect("PlayerAdded", this, "OnCreditsUpdated");
		OnCreditsUpdated();		
	}

	private void OnCreditsUpdated()
	{
		this.Text = $"{pingod.GameData.Credits} CREDITS";
	}
}
