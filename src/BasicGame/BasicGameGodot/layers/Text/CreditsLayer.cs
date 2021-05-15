using Godot;

/// <summary>
/// Basic credit Label to listen for Coin events to update the scene <para/>
/// CreditsLayer.tscn
/// </summary>
public class CreditsLayer : Label
{
	public override void _Ready()
	{
		//update when the credit changes
		GetNode("/root/GameGlobals").Connect("CreditAdded", this, "OnCreditAdded");
		OnCreditAdded();		
	}

	private void OnCreditAdded()
	{
		this.Text = $"{GameGlobals.Credits} CREDITS";
	}
}
