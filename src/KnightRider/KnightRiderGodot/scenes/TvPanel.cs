using Godot;
using System;

public class TvPanel : Control
{
	private VideoStream _defaultStream;
	private PinGodGame _pinGod;
	[Export] Godot.Collections.Dictionary<string, VideoStreamTheora> _streams = null;
	private Timer _timer;
	private Label _Label;
	private VideoPlayerPinball _video;
	public override void _EnterTree()
	{
		base._EnterTree();
		_video = GetNode<VideoPlayerPinball>("TvPanel/VideoPlayerPinball");
		_defaultStream = _video.Stream;
		_pinGod = GetNode<PinGodGame>("/root/PinGodGame");
		_timer = GetNode<Timer>("Timer");
		_Label = GetNode<Label>("Panel/CenterContainer/Label");
	}

	public override void _Ready()
	{
		base._Ready();

		_pinGod.Connect(nameof(KrPinGodGame.ChangeTvScene), this, nameof(OnTvSceneChanged));
		_Label.Text = string.Empty;
	}

	private void _on_Timer_timeout()
	{
		_Label.Visible = false;
		_video.Stream = _defaultStream;
		_video.SetLoopAndHold(true);
		_video.Play();
	}

	private void OnTvSceneChanged(string name, string label = "", float clear = 2f, bool loop = true)
	{
		CallDeferred(nameof(ChangeTvScene), name, label, clear, loop);
	}

	private void ChangeTvScene(string name, string label, float clear, bool loop = true)
	{
		_pinGod.LogDebug($"tv changed: {name}");
		//_pinGod.LogDebug("stream count: " + _streams.Count);
		_video.Stream = _streams[name];
		_video.SetLoopAndHold(loop);
		_video.Play();
		if (clear > 0)
		{
			_timer.Stop();
			_timer.Start(clear);
		}

		if (!string.IsNullOrWhiteSpace(label))
		{
			_Label.Text = label;
			_Label.Visible = true;
		}
	}
}
