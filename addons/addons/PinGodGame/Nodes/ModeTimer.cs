using Godot;

/// <summary>
/// A scene timer to display time left. Set mode name and mode title in scene file and initial time
/// </summary>
public class ModeTimer : Timer
{
	private Label titleLabel;
	private Label timeLeftLabel;

	[Export] int _ModeTime = 30;
	[Export] string _ModeName = "ModeName";
	[Export] string _ModeTitle = "Mode Title";

	/// <summary>
	/// Sets up the labels
	/// </summary>
	public override void _Ready()
	{
		titleLabel = GetNode("VBoxContainer/Title") as Label;
		titleLabel.Text = _ModeTitle;
		timeLeftLabel = GetNode("VBoxContainer/TimeLeftLabel") as Label;
	}

    /// <summary>
    /// Updates time left text. When time runs out a ModeTimedOut signal with the mode name is emitted
    /// </summary>
    private void _on_ModeTimer_timeout()
	{		
		timeLeftLabel.Text = _ModeTime.ToString();
		_ModeTime--;
		if(_ModeTime <= 0)
		{
			this.Stop();
			EmitSignal("ModeTimedOut", _ModeName);
		}
	}
}
