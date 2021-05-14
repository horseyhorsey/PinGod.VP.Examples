using Godot;
using System;
using static Godot.GD;
using static GameGlobals;

public class Game : Node2D
{
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("sw21")) //Left outlane
		{
			Print("sw: l_outlane");
			AddPoints(100);
		}
		if (@event.IsActionPressed("sw22")) //Left inlane
		{
			Print("sw: l_inlane");
			AddPoints(100);
		}
		if (@event.IsActionPressed("sw23")) //Right outlane
		{
			Print("sw: r_inlane");
			AddPoints(100);
		}
		if (@event.IsActionPressed("sw24")) //Left inlane
		{
			Print("sw: r_outlane");
			AddPoints(100);
		}
		if (@event.IsActionPressed("sw25")) //Left sling
		{
			Print("sw: l_sling");
			AddPoints(50);
		}
		if (@event.IsActionPressed("sw26"))//Right sling
		{
			Print("sw: r_sling");
			AddPoints(50);
		}
	}
}
