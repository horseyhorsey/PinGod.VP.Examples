#if TOOLS
using Godot;
using System;

/// <summary>
/// Not used anywhere?
/// </summary>
[Tool]
public class PinGodGameAddOn : EditorPlugin
{
	/// <summary>
	/// 
	/// </summary>
	public override void _EnterTree()
	{
		if (!Engine.EditorHint)
		{
			base._EnterTree();
		}			
	}

	/// <summary>
	/// 
	/// </summary>
	public override void _ExitTree()
	{
		if (!Engine.EditorHint)
		{
			base._ExitTree();
		}		
	}
}
#endif
