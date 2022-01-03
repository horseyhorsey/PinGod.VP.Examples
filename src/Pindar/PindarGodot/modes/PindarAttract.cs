using Godot;
using System.Collections.Generic;

/// <summary>
/// A basic attract mode that can start a game and cycle scenes with flippers. Add scenes into the "AttractLayers" in scene tree
/// </summary>
public class PindarAttract : Attract
{
    public override void _Ready()
    {
        base._Ready();

        pinGod.PlayVoice("meet_me", "Voice");
    }

    bool played = true;
    public override void ChangeLayer(bool forward = false)
    {
        base.ChangeLayer(forward);

        pinGod.LogInfo("attract change layer");

        played = !played;
        if(played)
            pinGod.PlayVoice("meet_me", "Voice");
        else
            pinGod.PlayVoice("megotyou", "Voice");
    }
}
