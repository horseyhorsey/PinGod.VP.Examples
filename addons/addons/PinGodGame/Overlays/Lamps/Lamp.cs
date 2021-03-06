using Godot;
using System;

public class Lamp : ColorRect
{
	private AnimationPlayer _animPlayer;
	private Label _label;

	public int Number { get; set; }

	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_label = GetNode("LampNumLabel") as Label;		
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	/// <summary>
	/// Sets the lamp state
	/// </summary>
	/// <param name="state"></param>
	internal void SetState(int state)
	{
		switch (state)
		{
			case 1:
				_animPlayer.CurrentAnimation = "on";
				break;
			case 2:
				_animPlayer.CurrentAnimation = "blink";
				break;				
			default:
				_animPlayer.CurrentAnimation = "off";
				break;
		}
	}

	internal void SetLabel(string label)
	{
		_label.Text = $"{Number}\n{label}";
		this.Update();
	}
}
