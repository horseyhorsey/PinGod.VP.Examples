using Godot;
using static Godot.GD;

/// <summary>
/// The Crater is the saucer in middle of playfield. Making shot runs a small Moon landing video mode. See <see cref="MoonLander"/> for scene and script
/// </summary>
public class Crater : Control
{
    const string LAMP_SHOW = "lampshow_1";
	/// <summary>
	/// This name is the same for the switch and the coil
	/// </summary>
    const string SAUCER_COIL_SWITCH = "crater_saucer";
    const float TIME_OUT = 5.0f;

    /// <summary>
    /// Video mode scene
    /// </summary>
    private PackedScene _moonLanderScene;
    private Node moonLanderInstance;
    private PinGodGame pinGod;
    private Timer saucerTimer;
    public AudioStreamPlayer AudioStream { get; private set; }	

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;

		//init the timer for saucer and connect it's timed out signal
		saucerTimer = new Timer() { OneShot = true, Autostart = false };
		AddChild(saucerTimer);
		saucerTimer.Connect("timeout", this, "_timeout");		

		//get the instance of the player
		AudioStream = GetNode("AudioStreamPlayer") as AudioStreamPlayer;

		//load the moon lander video mode
		_moonLanderScene = Load("moonstation_lander/MoonLander.tscn") as PackedScene;
	}

	/// <summary>
	/// Crater saucer. Start timer and exit when finished.
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (pinGod.GameInPlay && !pinGod.IsTilted)
		{
			if (pinGod.SwitchOn(SAUCER_COIL_SWITCH, @event))
			{
				//create an instance of the moon lander scene and add to tree
				moonLanderInstance = _moonLanderScene.Instance();
				AddChild(moonLanderInstance);

				//make this crater scene visible
				Visible = true;

				//play a lamp show and start time out
				pinGod.SolenoidOn(LAMP_SHOW, 1);
				saucerTimer.Start(TIME_OUT);

				//show the scene and play the audio
				this.Show();
				AudioStream?.Play();
			}
		}
		else
		{
			pinGod.SolenoidPulse(SAUCER_COIL_SWITCH);
		}
	}

	private void _timeout()
	{
		pinGod.LogDebug("crater timed out, freeing video mode");
		moonLanderInstance.QueueFree();
		RemoveChild(moonLanderInstance);
		
		//kick out the ball and turn off the lampshow
		pinGod.SolenoidPulse(SAUCER_COIL_SWITCH);
		pinGod.SolenoidOn(LAMP_SHOW, 0);
		Hide();
	}
}
