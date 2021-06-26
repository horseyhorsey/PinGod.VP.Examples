#if TOOLS
using Godot;
using System;

[Tool]
public class PinGodGameAddOn : EditorPlugin
{
	public override void _EnterTree()
	{
		if (!Engine.EditorHint)
		{
			base._EnterTree();
		}			
	}

	public override void _ExitTree()
	{
		if (!Engine.EditorHint)
		{
			base._ExitTree();
		}		
	}
}
#endif
