using Godot;
using Godot.Collections;

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

    private void SetNextStream() => player.Stream = _streams[audioIndex];
}

