/// <summary>
/// Adds all Canvas Items in the "AttractLayers". These cycle on a timer and can be cycled with Flipper actions
/// </summary>
public class KrAttract : Attract
{
	public override void _Ready()
	{
		base._Ready();
		pinGod.SolenoidOn("show_attract", 1);
		pinGod.PlayMusic("KRTVintro");
	}

	public override void _ExitTree()
	{
		pinGod.SolenoidOn("show_attract", 0);
		base._ExitTree();		
	}
}
