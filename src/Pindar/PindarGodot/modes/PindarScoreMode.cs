using Godot;
using System;

public class PindarScoreMode : ScoreMode
{
    public override void _Ready()
    {
        base._Ready();

        pinGod.LogInfo("pindar score mode ready");
    }

    internal void ChangeSnakeAnimation(string name, bool loop=false)
    {
        var snake = GetNode<Node>("Viewport/ScoreScene").GetNode<Snake>("snake");
        snake.ChangeSnakeAnimation(name, loop);
    }
}