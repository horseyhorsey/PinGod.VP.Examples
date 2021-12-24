using Godot;

public class MotuMainScene : MainScene
{
    [Export] public PackedScene _beastmanMode;
    [Export] public PackedScene _graySkullMultiball;
    [Export] public PackedScene _ripperMultiball;
    [Export] public PackedScene _skeleMultiball;
    [Export] public PackedScene _stratosMode;
    [Export] public PackedScene _sorceressMode;
    [Export] public PackedScene _motuMultiball;

    public override void _Ready()
    {
        base._Ready();

        //hack: give some last changed time...
        Machine.Switches["inner_spinner"].Time = 5000;
        Machine.Switches["loop_left"].Time = 5000;
        Machine.Switches["loop_right"].Time = 5000;
        Machine.Switches["skele_trigger"].Time = 5000;
    }
}