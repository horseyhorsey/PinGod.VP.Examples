using Godot;
using System;
using static Godot.GD;

/// <summary>
/// Simple wrapper for OGV. note: webm wasn't good in windows tests.
/// </summary>
public class VideoLayer : Control
{
	private VideoPlayer videoPlayer;

	[Export] private bool _loop;
	[Export] private bool _hold;
	[Export] private string _resource_path;
	[Export] private bool _auto_play = true;
	private VideoStreamTheora stream;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//makes the video pause?
		PauseMode = PauseModeEnum.Stop;

		videoPlayer = GetNode<VideoPlayer>("VideoPlayer");
		videoPlayer.Bus = "Music";

		if(!string.IsNullOrWhiteSpace(_resource_path) && videoPlayer != null)
		{
			var f = new File();
			if (f.FileExists(_resource_path))
			{
				stream = new VideoStreamTheora();
				stream.File = _resource_path;				
				videoPlayer.Stream = stream;

				if (_auto_play)
				{
					videoPlayer.Play();
				}					
			}
		}
	}

	/// <summary>
	/// When videos ended we need to play again if set to looping.
	/// </summary>
	private void _on_VideoPlayer_finished()
	{		
		if (_loop)
		{
			videoPlayer.Play();
		}			
		else if(!_hold)
		{
			videoPlayer.Hide();
		}
	}
}
