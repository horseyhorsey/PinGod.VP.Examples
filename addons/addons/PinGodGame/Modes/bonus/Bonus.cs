using Godot;

/// <summary>
/// A bonus layer / mode. Display at end of ball. Bonus.tscn scene <para/>
/// Sends <see cref="PinGodGame.BonusEnded"/> signal on <see cref="_display_for_seconds"/> timeout
/// </summary>
public class Bonus : Control
{
    [Export] protected string _defaultText = string.Empty;
    [Export] protected float _display_for_seconds = 5;
    internal Label label;
	protected PinGodGame pinGod;
	protected Timer timer;

	/// <summary>
	/// Sets up scene
	/// </summary>
	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		//get nodes from this scene tree
		timer = GetNode("Timer") as Timer;
		label = GetNode("Label") as Label;

		if(string.IsNullOrWhiteSpace(_defaultText))
			_defaultText = Tr("BONUS_EOB");
	}

	public override void _Ready()
	{
		if (!timer.IsStopped())
			timer.Stop();
	}

	/// <summary>
	/// Stops the timer and emits the BonusEnded signal when complete
	/// </summary>
	public virtual void OnTimedOut()
    {
        pinGod.LogInfo("bonus: BonusEnded");
        timer.Stop();
        this.Visible = false;
        pinGod.EmitSignal("BonusEnded");
    }

    /// <summary>
    /// Creates bonus text to display with players bonus
    /// </summary>
    /// <returns></returns>
    public virtual string SetBonusText(string text = "")
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            text = _defaultText;
        }

		//use extension method to create formatted score eg: "1,000,000"
		text += "\n" + pinGod.Player?.Bonus.ToScoreString();

		return text;
    }

	/// <summary>
	/// Starts display for the amount of seconds set
	/// </summary>
	/// <param name="visible">can hide the display but still use timer to let game know bonus ended</param>
	public virtual void StartBonusDisplay(bool visible = true)
	{
        if (visible)
        {
			label.Text = SetBonusText();
			pinGod.LogDebug("bonus: set label text to:", label.Text);
		}

		pinGod.LogInfo("bonus displaying for " + _display_for_seconds);
		timer.Start(_display_for_seconds);
		Visible = visible;

		if (pinGod.Player != null)
		{
			pinGod.AddPoints(pinGod.Player.Bonus);
		}
	}
	/// <summary>
	/// Bonus has times out. Hide the display and send <see cref="PinGodGame.BonusEnded"/>
	/// </summary>
	private void _on_Timer_timeout()
	{
		OnTimedOut();		
	}
}
