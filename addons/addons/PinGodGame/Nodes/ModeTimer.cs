using Godot;

public class ModeTimer : Timer
{
	private Label titleLabel;
	private Label timeLeftLabel;

	[Export] int _ModeTime = 30;
	[Export] string _ModeName = "ModeName";
	[Export] string _ModeTitle = "Mode Title";

	public override void _Ready()
	{
		titleLabel = GetNode("VBoxContainer/Title") as Label;
		titleLabel.Text = _ModeTitle;
		timeLeftLabel = GetNode("VBoxContainer/TimeLeftLabel") as Label;
	}

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
