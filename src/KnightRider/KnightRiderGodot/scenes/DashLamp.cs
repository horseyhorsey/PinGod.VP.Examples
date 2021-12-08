using Godot;

public class DashLamp : Control
{
	[Export] Color _colour = Colors.Green;
	[Export] string _text = "LABEL\nTEXT";
	private ColorRect rect;
	private Label label;

	public override void _EnterTree()
	{
		base._EnterTree();
		rect = GetNode<ColorRect>("ColorRect");
		label = GetNode<Label>("Label");

		rect.Color = _colour;
		label.Text = _text;
	}

	public void Color(Color color) { _colour = color; rect.Color = _colour; }
	public void Text(string text) { _text = text; label.Text = text; }

	internal void LampOn(int v)
	{		
		if(v == 1)
		{
			rect.Color = _colour;
		}
		else
		{
			rect.Color = _colour.Darkened(0.8f);
		}
	}
}
