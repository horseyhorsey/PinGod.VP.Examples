using Godot;
using System;

public class Snake : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    bool loop = false;
    internal void ChangeSnakeAnimation(string name, bool loop)
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play(name);
        this.loop = loop;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    void AnimationPlayer_animation_finished(string name)
    {
        if(loop)
            GetNode<AnimationPlayer>("AnimationPlayer").Play(name);
    }
}
