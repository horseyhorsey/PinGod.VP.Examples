using Godot;

/// <summary>
/// Basic credit Label to listen for Coin events to update the scene <para/>
/// CreditsLayer.tscn
/// </summary>
public class CreditsLayer : Label
{
	[Export]
	private int _CreditButtonNum = 2;

	public override void _Ready()
	{
		UpdateCreditsText();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("sw"+_CreditButtonNum))
		{
			UpdateCreditsText();
		}
	}
	private void UpdateCreditsText()
	{
		this.Text = $"{GameGlobals.Credits} CREDITS";
	}
}
