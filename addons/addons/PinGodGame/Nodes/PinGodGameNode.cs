using Godot;

/// <summary>
/// Uses Godot Node as a base to provide PinGodGame access
/// </summary>
public abstract class PinGodGameNode : Node
{
    /// <summary>
    /// A reference to PinGodGame node
    /// </summary>
    public PinGodGameBase PinGod;

    public override void _EnterTree()
    {
        PinGod = GetNode("/root/PinGodGame") as PinGodGameBase;        
    }
}