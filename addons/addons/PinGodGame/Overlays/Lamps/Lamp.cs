using Godot;

/// <summary>
/// Lamp overlay. For on screen debugging. Uses a Label and animation player to set light states
/// </summary>
public class Lamp : ColorRect
{
	private AnimationPlayer _animPlayer;
	private Label _label;

	/// <summary>
	/// Number of the lamp
	/// </summary>
	public int Number { get; set; }

    /// <summary>
    /// Called when the node enters the scene tree for the first time. <para/>
	/// Initializes the <see cref="_animPlayer"/> , <see cref="_label"/>
    /// </summary>
    public override void _Ready()
	{
		_label = GetNode("LampNumLabel") as Label;		
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	/// <summary>
	/// Sets the lamp state, <see cref="_animPlayer"/> animation
	/// </summary>
	/// <param name="state"></param>
	internal void SetState(LightState state) => _animPlayer.CurrentAnimation = state.ToString().ToLower();

	/// <summary>
	/// Sets the label which includes number with label
	/// </summary>
	/// <param name="label"></param>
    internal void SetLabel(string label)
	{
		_label.Text = $"{Number}\n{label}";
		this.Update();
	}
}
