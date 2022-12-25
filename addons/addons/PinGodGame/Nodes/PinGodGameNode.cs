using Godot;

/// <summary>
/// Uses Godot Node as a base to provide PinGodGame access to a class
/// </summary>
public abstract class PinGodGameNode : Node
{
    /// <summary>
    /// A reference to PinGodGame node
    /// </summary>
    public PinGodGame pinGod;

    /// <summary>
    /// Gets a reference to <see cref="pinGod"/> in the root /root/PinGodGame
    /// </summary>
    public override void _EnterTree()
    {
        pinGod = GetNodeOrNull("/root/PinGodGame") as PinGodGame;        
    }
}