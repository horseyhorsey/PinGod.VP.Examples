using Godot;

public class PlayboyAward : Control
{
	[Export] string _award_text = "EXTRA BALL";
	private Label label;

	public override void _EnterTree()
	{
		base._EnterTree();
		base._Ready();
		label = GetNode<Label>("Message");
		label.Text = _award_text;
	}

	private void _on_Timer_timeout()
	{
		QueueFree();
	}

	internal void SetAwardText(string awardText) => _award_text = awardText;
}

