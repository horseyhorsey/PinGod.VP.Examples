using Godot;

/// <summary>
/// Custom attract for Jaws
/// </summary>
public class JawsAttract : Attract
{
    private JawsPinGodGame jawsPingod;

    public override void _EnterTree()
	{
		base._EnterTree();

		jawsPingod = pinGod as JawsPinGodGame;		
		pinGod.SetLampState("gi_0", 1);
		pinGod.SetLampState("gi_1", 1);
		pinGod.SetLampState("gi_2", 1);
	}

	public override void _Ready()
	{
		base._Ready();

		pinGod.LogInfo("Jaws Attract _ready");

        //put this here because it doesn't seem to stay on top when it errors loading
  //      if (!jawsPingod.gamePlayed)
  //      {
		//	jawsPingod.gamePlayed = true; //flag to stop it doing it again, otherwise lose VP focus
		//	OS.SetWindowAlwaysOnTop(ProjectSettings.GetSetting("display/window/size/always_on_top").ToString() == "true" ? true : false);
		//}		

		//pinGodGame.SolenoidPulse("disable_shows");
	}
}
