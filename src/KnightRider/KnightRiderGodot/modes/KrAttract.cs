using Godot;
/// <summary>
/// Adds all Canvas Items in the "AttractLayers". These cycle on a timer and can be cycled with Flipper actions
/// </summary>
public class KrAttract : Attract
{
	public override void _Ready()
	{
		base._Ready();
		pinGod.SolenoidOn("show_attract", 1);
		var vid = GetNode<VideoPlayerPinball>(nameof(VideoPlayerPinball));
		vid.Stream = pinGod.GetResources().GetResource("kitt_bonnet") as VideoStreamTheora;
		vid.Play();
		pinGod.PlayMusic("KRTVintro");
	}

	public override void _ExitTree()
	{
		pinGod.SolenoidOn("show_attract", 0);
		base._ExitTree();		
	}
}
