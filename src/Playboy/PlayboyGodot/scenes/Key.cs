using Godot;
using System;

public class Key : Node2D
{	
	private void _on_AnimationPlayer_animation_finished(String anim_name)
	{
		var anim = GetNode<AnimationPlayer>("AnimationPlayer");
		switch (anim_name)
		{            
			case "KeyCollected":
				anim.Play("KeyCollected 2");
				break;
			case "KeyCollected 2":
				this.QueueFree();
				break;
			default:
				break;
		}
	}
}
