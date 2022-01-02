using Godot;
using System;

public class PlayboyAttract : Attract
{    
    public override void _Ready()
    {
        base._Ready();

        pinGod.PlayMusic("horsepin_i_see_you_move_it");
    }
}
