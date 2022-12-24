using Godot;

/// <summary>
/// Display grid of lamps on screen. Scenes\LampMatrix.tscn
/// </summary>
public class LampMatrix : GridContainer
{
	[Export] int _lamp_count = 64;
	[Export] Vector2 _lamp_size_pixels = new Vector2(50, 50);
	[Export] string _lamp_scene = "res://addons/PinGodGame/Scenes/Lamp.tscn";

    private Godot.Collections.Array<Lamp> _lamps = null;
	private PackedScene _lampScene;

	/// <summary>
	/// Creates instances of the <see cref="_lampScene"/> for all the <see cref="_lamp_count"/>
	/// </summary>
	public override void _EnterTree()
	{
		base._EnterTree();

		_lampScene = ResourceLoader.Load<PackedScene>(_lamp_scene);
		_lamps = new Godot.Collections.Array<Lamp>();
		for (int i = 0; i < _lamp_count; i++)
		{
			var lamp = _lampScene.Instance() as Lamp;
			lamp.RectMinSize = _lamp_size_pixels;
			lamp.RectSize = _lamp_size_pixels;
			lamp.Number = i + 1;

			_lamps.Add(lamp);
			AddChild(lamp);
		}
	}

	/// <summary>
	/// Sets the label
	/// </summary>
	/// <param name="num"></param>
	/// <param name="label"></param>
	public void SetLabel(int num, string label)
	{
		if (_lamps?.Count > 0)
		{
			_lamps[num - 1].SetLabel(label);
		}
	}

	/// <summary>
	/// Sets light state
	/// </summary>
	/// <param name="num"></param>
	/// <param name="state"></param>
	public void SetState(int num, int state)
	{
		if(_lamps?.Count > 0)
		{
			var s = state < 3 ? (LightState)state : LightState.Off;
            _lamps[num - 1].SetState(s);
		}		
	}
}
