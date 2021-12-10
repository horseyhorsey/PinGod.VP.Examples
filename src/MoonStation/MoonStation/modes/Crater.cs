using Godot;
using static Godot.GD;

/// <summary>
/// The Crater is the saucer in middle of playfield. Making shot runs a small Moon landing video mode. See <see cref="MoonLander"/> for scene and script
/// </summary>
public class Crater : Control
{
   const string LAMP_SHOW = "lampshow_1";
	/// <summary>
	/// Time to wait before kicking ball when mode over
	/// </summary>
    const float TIME_OUT = 3.0f;

    /// <summary>
    /// Video mode scene
    /// </summary>
    private PackedScene _moonLanderScene;
    private Node moonLanderInstance;
    private PinGodGame pinGod;

	/// <summary>
	/// The crater saucer
	/// </summary>
    private BallStackPinball saucer;

    public AudioStreamPlayer AudioStream { get; private set; }	

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;

		saucer = GetNode<BallStackPinball>("CraterSaucer");

		//get the instance of the player
		AudioStream = GetNode("AudioStreamPlayer") as AudioStreamPlayer;

		//load the moon lander video mode
		_moonLanderScene = Load("moonstation_lander/MoonLander.tscn") as PackedScene;
	}

	public void OnCraterSaucerSwitchActive()
    {
		if (pinGod.GameInPlay && !pinGod.IsTilted)
		{
			//create an instance of the moon lander scene and add to tree
			moonLanderInstance = _moonLanderScene.Instance();
			AddChild(moonLanderInstance);

			//make this crater scene visible
			Visible = true;

			//play a lamp show and start time out
			pinGod.SolenoidOn(LAMP_SHOW, 1);
			saucer.Start(TIME_OUT);

			//show the scene and play the audio
			this.Show();
			AudioStream?.Play();
		}
		else
		{
			saucer.Start(1f); //kick the ball with time from the ballstack
		}
	}

	/// <summary>
	/// Timeout from saucer. Removes the video mode and kicks the ball
	/// </summary>
	private void OnCraterSaucer_timeout()
    {
		pinGod.LogDebug("crater timed out, freeing video mode");
		moonLanderInstance.QueueFree();
		RemoveChild(moonLanderInstance);

		//kick out the ball and turn off the lampshow
		saucer.SolenoidPulse();
		pinGod.SolenoidOn(LAMP_SHOW, 0);

		Hide();
	}
}
