using Godot;
using System;

public class MotuAttract : Attract
{
    private AnimationPlayer _animPlayer;

    public override void _Ready()
    {
        base._Ready();

        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    private void AnimationPlayer_animation_finished(string animName)
    {
        _animPlayer.Stop(); 
    }

    bool attract1 = false;
    public override void ChangeLayer(bool forward = false)
    {
        base.ChangeLayer(forward);

        attract1 = !attract1;
        if(attract1) pinGod.SolenoidOn("vpcoil", 11);
        else pinGod.SolenoidOn("vpcoil", 12);

        if (base.GetCurrentSceneIndex() == 0)
        {
            _animPlayer.Play();
        }
        else
        {
            _animPlayer.Stop();
        }
    }
}
