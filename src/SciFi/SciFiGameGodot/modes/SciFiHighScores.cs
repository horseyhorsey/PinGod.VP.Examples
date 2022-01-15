using Godot;
using System;

public class SciFiHighScores : HighScores
{
    public override void _Ready()
    {
        base._Ready();

        var stream = pinGod.GetResources().GetResource("cosmic_3") as VideoStreamTheora;
        GetNode<VideoPlayerPinball>(nameof(VideoPlayerPinball)).Stream = stream;
    }
}
