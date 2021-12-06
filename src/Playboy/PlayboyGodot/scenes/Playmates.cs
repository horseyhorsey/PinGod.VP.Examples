using Godot;
using System;

public class Playmates : Control
{
	//private Signals _signals;
	private Sprite[] pMateSprites;

	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//_signals = (Signals)GetNode("/root/Signals");
		//_signals.Connect("playmate_complete", this, "PlaymateComplete");

		pMateSprites = new Sprite[]
		{
			GetNode<Sprite>("PlaymatesBar/0"),
			GetNode<Sprite>("PlaymatesBar/1"),
			GetNode<Sprite>("PlaymatesBar/2"),
			GetNode<Sprite>("PlaymatesBar/3"),
			GetNode<Sprite>("PlaymatesBar/4"),
		};
		PlaymateReset();
	}

	public void PlaymateComplete(int index)
	{
		GD.Print("playmate completed");
		pMateSprites[index].Visible = true;
	}

	private void PlaymateReset()
	{
		foreach (var pmate in pMateSprites)
		{
			pmate.Visible = false;
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
