using Godot;
using System;

/// <summary>
/// A label that blinks on a timer with optional methods for controlling the blink
/// </summary>
[Tool]
public class BlinkingLabel : Label
{
	/// <summary>
	/// Speed of the timer
	/// </summary>
	[Export]
	private float _blinking = 0.3f;
	internal float Blinking => _blinking;

	private Timer _timer;
	private Color modulateDefaultColor;
	private bool blinkVisible;

	#region Public Methods
	/// <summary>
	/// Creates the timer and adds to the tree
	/// </summary>
	public override void _EnterTree()
	{
		if (!Engine.EditorHint)
		{
			// Code to execute when in game.
			//connect up to these signals to stop the timer, no need to be running all the time not in view?
			this.Connect("hide", this, "_hide");
			this.Connect("visibility_changed", this, "_visibility_changed");
			_timer = new Timer() { Autostart = true, OneShot = false, WaitTime = _blinking <= 0 ? 0.3f : _blinking };
			_timer.Connect("timeout", this, "timeout");
			AddChild(_timer);
		}
		blinkVisible = this.Visible;
	}

	/// <summary>
	/// Sets the color of the text, when blinks alpha is used to hide and reset to this color
	/// </summary>
	public override void _Ready()
	{
		modulateDefaultColor = this.Modulate;
	}

	/// <summary>
	/// Stops the timer
	/// </summary>
	public override void _ExitTree()
	{
		_timer?.Stop();
		_timer?.QueueFree();
	}

	/// <summary>
	/// Restarts the timer with new speed
	/// </summary>
	/// <param name="speed"></param>
	public void Start(float speed = 0.3f) => _timer?.Start(speed <= 0 ? 0.3f : speed);

	/// <summary>
	/// Stops the blinking and optionally hold text on screen 
	/// </summary>
	/// <param name="showText">Hold the frame, show the text no blinking</param>
	public void Stop(bool showText = false)
	{
		if (_timer == null) return;
		_timer.Stop();
		this.Visible = showText;
	}
	#endregion

	bool blinked = true;
	/// <summary>
	/// change the font color to alpha to blink text
	/// </summary>
	void timeout()
	{
		if (!blinked)
		{
			this.Modulate = new Color(0, 0, 0, 0);
		}
		else
		{
			this.Modulate = modulateDefaultColor;
		}

		blinked = !blinked;
	}

	/// <summary>
	/// When visibility checks if <see cref="blinkVisible"/>, if <see cref="_timer"/> is stopped it starts the blinking
	/// </summary>
	void _visibility_changed()
	{
		if (_timer == null) return;
		if (blinkVisible)
		{
			if (_timer.IsStopped())
			{
				blinked = true;
				Start(this.Blinking);
			}
		}
	}

	/// <summary>
	/// Stop the time when hidden.
	/// </summary>
	void _hide()
	{
		if (_timer == null) return;
		blinked = false;
		_timer.Stop();
	}
}
