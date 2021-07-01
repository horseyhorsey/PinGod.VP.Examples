using Godot;

/// <summary>
/// Simple scene to show blinking ball save and remove when timer ends
/// </summary>
public class BallSave : Control
{
	[Export] float _remove_after_time = 2f;

	public override void _EnterTree()
	{
		base._EnterTree();
		GetNode<Timer>("Timer").WaitTime = _remove_after_time;
	}

	public void SetRemoveAfterTime(float time) => _remove_after_time = time;
	void _on_Timer_timeout() => this.QueueFree();
}
