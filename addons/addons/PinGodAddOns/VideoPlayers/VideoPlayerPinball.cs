using Godot;
using System;

/// <summary>
/// Tool: Video Player for Pinball
/// <para/>
/// </summary>
[Tool]
public class VideoPlayerPinball : VideoPlayer
{
	[Export] private bool _hold = false;
	[Export] private bool _loop = false;
	[Export] private bool _play_when_visible = false;
	[Export] private bool _pause_when_hidden = false;

	#region Public Methods
	/// <summary>
	/// Connects the videos finished , hide and visibility_changed signals from the inherited scene
	/// </summary>
	public override void _EnterTree()
	{
		//makes the video pause?
		PauseMode = PauseModeEnum.Stop;

		if (!Engine.EditorHint)
		{
			// Code to execute when in game.
			//connect up to these signals to stop the timer, no need to be running all the time not in view?
			this.Connect("hide", this, "_hide");
			this.Connect("visibility_changed", this, "_visibility_changed");
			this.Connect("finished", this, "_finished");
		}		
	}

	/// <summary>
	/// videos finished signal. We need to play again if set to looping.
	/// </summary>
	void _finished()
	{
		//GD.Print("finished");
		if (_loop)
		{
			this.Play();
		}
		else if (_hold)
		{
			this.Stop();
		}
		else if (!_hold)
		{
			this.Hide();
		}
	}

	internal void SetLoopAndHold(bool loop)
	{
		_loop = loop;
		if (!loop) _hold = true; else _hold = false;
	}

	#endregion

	/// <summary>
	/// When visibility changes if pause when hidden then pause is false, or play when visible
	/// </summary>
	void _visibility_changed()
	{
		if (_pause_when_hidden)
		{
			this.Paused = false;
		}
		else if (_play_when_visible)
		{
			//this.StreamPosition = 0;
			this.Play();
		}			
	}

	/// <summary>
	/// Stop the time when hidden.
	/// </summary>
	void _hide()
	{		
		if (_pause_when_hidden)
		{			
			this.Paused = true;
		}
		else if (_play_when_visible)
		{
			if (this.Paused)
			{
				this.Paused = false;
			}				
			else
			{
				this.Play();
			}
		}
		else
		{ 
			this.Stop();
		}		
	}
}
