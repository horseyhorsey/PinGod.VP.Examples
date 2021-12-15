using Godot;
using Godot.Collections;
using System;

public class NmAttract : Attract
{
    int audioIndex = 0;
    [Export] Array<AudioStream> _streams;
    private AudioStreamPlayer player;

    public override void _Ready()
    {
        base._Ready();

        player = GetNode<AudioStreamPlayer>(nameof(AudioStreamPlayer));
        player.Connect("finished", this, nameof(Player_finished));
        SetNextStream();
        player.Play();
    }

    public void Player_finished()
    {        
        audioIndex++;
        if (audioIndex > 1) audioIndex = 0;
        player.Stop();
        SetNextStream();
        player.Play();
    }

    int coilState = 1;
    public override void ChangeLayer(bool forward = false)
    {
        base.ChangeLayer(forward);

        //play a lampshow in VP when cycling
        coilState++;
        if (coilState == 3) coilState = 1;
        pinGod.SolenoidOn("vpcoil", Convert.ToByte(coilState));        
    }

    private void SetNextStream() => player.Stream = _streams[audioIndex];
}

