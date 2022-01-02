using static Godot.GD;

/// <summary>
/// Custom attract for Jaws
/// </summary>
public class JawsAttract : Attract
{
	public override void _EnterTree()
	{
		base._EnterTree();

		pinGod.SetLampState("gi_0", 1);
		pinGod.SetLampState("gi_1", 1);
		pinGod.SetLampState("gi_2", 1);
	}

	public override void _Ready()
	{
		pinGod.LogInfo("Jaws Attract _ready");
		//pinGodGame.SolenoidPulse("disable_shows");
	}
}
