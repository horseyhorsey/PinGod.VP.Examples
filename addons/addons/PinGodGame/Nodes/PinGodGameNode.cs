using Godot;

/// <summary>
/// Uses Godot Node as a base to provide PinGodGame access
/// </summary>
public abstract class PinGodGameNode : Node
{
    /// <summary>
    /// A reference to PinGodGame node
    /// </summary>
    public PinGodGame pinGod;

    public override void _EnterTree()
    {
        pinGod = GetNodeOrNull("/root/PinGodGame") as PinGodGame;        
    }
}