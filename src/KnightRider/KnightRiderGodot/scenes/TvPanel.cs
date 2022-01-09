using Godot;
using System;

public class TvPanel : Control
{
	private VideoStream _defaultStream;
	private PinGodGame _pinGod;
	Godot.Collections.Dictionary<string, VideoStreamTheora> _streams = null;
	private Timer _timer;
	private Label _Label;
	private VideoPlayerPinball _video;
	public override void _EnterTree()
    {
        base._EnterTree();
        _video = GetNode<VideoPlayerPinball>("TvPanel/VideoPlayerPinball");        
        _pinGod = GetNode<PinGodGame>("/root/PinGodGame");
        _timer = GetNode<Timer>("Timer");
        _Label = GetNode<Label>("Panel/CenterContainer/Label");

        GetResources();
    }

    private void GetResources()
    {
		var res = _pinGod.GetResources();
        _streams = new Godot.Collections.Dictionary<string, VideoStreamTheora>();
		_streams.Add("kitt_dash", res.GetResource("kitt_dash") as VideoStreamTheora);
		_streams.Add("karr_start", res.GetResource("karr_start") as VideoStreamTheora);
		_streams.Add("kitt_birds", res.GetResource("kitt_birds") as VideoStreamTheora);
		_streams.Add("kitt_bonnet", res.GetResource("kitt_bonnet") as VideoStreamTheora);
		_streams.Add("kitt_corn", res.GetResource("kitt_corn") as VideoStreamTheora);
		_streams.Add("kitt_dark", res.GetResource("kitt_dark") as VideoStreamTheora);
		_streams.Add("karr_dash", res.GetResource("karr_dash") as VideoStreamTheora);
		_streams.Add("kitt_intro_headon", res.GetResource("kitt_intro_headon") as VideoStreamTheora);
		_streams.Add("kitt_jump", res.GetResource("kitt_jump") as VideoStreamTheora);
		_streams.Add("kitt_turboboost", res.GetResource("kitt_turboboost") as VideoStreamTheora);
		_streams.Add("kitt_two_wheel", res.GetResource("kitt_two_wheel") as VideoStreamTheora);
		_streams.Add("kr_pursuit_mode", res.GetResource("kr_pursuit_mode") as VideoStreamTheora);
		_streams.Add("mikes_finger", res.GetResource("mikes_finger") as VideoStreamTheora);
		_streams.Add("truck_entry", res.GetResource("truck_entry") as VideoStreamTheora);
		_streams.Add("truck_side", res.GetResource("truck_side") as VideoStreamTheora);
		_streams.Add("kr_wheel", res.GetResource("kr_wheel") as VideoStreamTheora);

		_defaultStream = _streams["kr_wheel"];
	}

    public override void _Ready()
	{
		base._Ready();

		_pinGod.Connect(nameof(KrPinGodGame.ChangeTvScene), this, nameof(OnTvSceneChanged));
		_Label.Text = string.Empty;
		_on_Timer_timeout(); //loop the drivers hands
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
