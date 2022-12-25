using Godot;

/// <summary>
/// Simple scene to show blinking ball save and disposes (queue free) when the timer ends
/// </summary>
public class BallSave : Control
{
	[Export] float _remove_after_time = 2f;

	/// <summary>
	/// Finds a Timer named Timer and sets the wait time to <see cref="_remove_after_time"/>
	/// </summary>
	public override void _EnterTree()
	{
		base._EnterTree();
		GetNode<Timer>("Timer").WaitTime = _remove_after_time;
	}
	/// <summary>
	/// Resets . sets the <see cref="_remove_after_time"/>
	/// </summary>
	/// <param name="time"></param>
	public void SetRemoveAfterTime(float time) => _remove_after_time = time;
	void _on_Timer_timeout() => this.QueueFree();
}
