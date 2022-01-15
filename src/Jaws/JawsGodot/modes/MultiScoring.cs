using Godot;
using System;
using System.Linq;

public class MultiScoring : Control
{
	Texture _barrelTexture;
	bool _displayBusy = false;
	private TextureRect _iconLayer;
	private Label _label;
	Texture _orcaTexture;
	Texture _sharkTexture;
	private Tween _tween;
	private Game game;	
	private JawsPinGodGame pinGod;
	public bool IsScoreModeRunning { get; private set; }
	public int JackpotSeedValue { get; private set; }
	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as JawsPinGodGame;
		game = GetParent().GetParent() as Game;
		_tween = GetNode("Tween") as Tween;
		_iconLayer = GetNode("IconLayer") as TextureRect;

		_label = GetNode("CenterContainer/Label") as Label;

		//_barrelLayer.RectPosition
		_orcaTexture = GD.Load($"res://assets/img/orcaLayer.png") as Texture;
		_barrelTexture = GD.Load($"res://assets/img/barrelLayer.png") as Texture;
		_sharkTexture = GD.Load($"res://assets/img/sharktoon.png") as Texture;

		JackpotSeedValue = 2500000;
	}

	public override void _Input(InputEvent @event)
	{
		if (IsScoreModeRunning)
		{
			if (pinGod.SwitchOn("bumper_0", @event))
			{
				game.AddPoints(GetScore(true));
			}
			if (pinGod.SwitchOn("bumper_1", @event))
			{
				game.AddPoints(GetScore(true));
			}
			if (pinGod.SwitchOn("bumper_2", @event))
			{
				game.AddPoints(GetScore(true));
			}

			if(pinGod.GetLastSwitchChangedTime("orbit_m") > 2000 && pinGod.GetLastSwitchChangedTime("plunger_lane") > 2000)
			{
				if (pinGod.SwitchOn("orbit_m", @event))
				{
					pinGod.LogInfo("mball orbit_m");
					var score = game.AddPoints(GetScore(false));
					_label.Text = PinGodExtensions.ToScoreString(score);
					game.currentPlayer.Bonus += game.DoublePlayfield ? Game.BONUS_VALUE * 2 : Game.BONUS_VALUE;
					game.currentPlayer.SuperjackpotValue += 100000;
					PlayScoreScene(_orcaTexture);
					pinGod.SolenoidOn("vpcoil", 1);
				}
			}

			if (pinGod.SwitchOn("orca_magnet", @event))
			{
				var score = game.AddPoints(GetScore(false));
				_label.Text = PinGodExtensions.ToScoreString(score);
				game.currentPlayer.Bonus += game.DoublePlayfield ? Game.BONUS_VALUE * 2 : Game.BONUS_VALUE;
				game.currentPlayer.SuperjackpotValue += 100000;
				PlayScoreScene(_orcaTexture);
				pinGod.SolenoidOn("vpcoil", 2);
			}

			if (pinGod.SwitchOn("bruce_vuk", @event))
			{
				var score = game.AddPoints(GetScore(false));
				_label.Text = PinGodExtensions.ToScoreString(score);
				game.currentPlayer.BonusBruce += game.DoublePlayfield ? Game.BONUS_BRUCEY_VALUE * 2 : Game.BONUS_BRUCEY_VALUE;
				game.currentPlayer.SuperjackpotValue += 100000;
				PlayScoreScene(_sharkTexture);
				pinGod.SolenoidOn("vpcoil", 1);
			}

			if (pinGod.SwitchOn("barrel_kicker", @event))
			{
				var score = game.AddPoints(GetScore(false));
				_label.Text = PinGodExtensions.ToScoreString(score);
				game.currentPlayer.BonusBarrel += game.DoublePlayfield ? Game.BONUS_BARREL_VALUE * 2 : Game.BONUS_BARREL_VALUE;
				game.currentPlayer.SuperjackpotValue += 100000;
				PlayScoreScene(_barrelTexture);
				pinGod.SolenoidOn("vpcoil", 3);
			}
		}
	}

	/// <summary>
	/// Set the input to not process on start. The game should enabled StartScoringMode
	/// </summary>
	public override void _Ready()
	{
		SetProcessInput(false);
		pinGod.LogInfo("mscoring active: input processing off");
		this.Visible = false;
	}
	public void StartScoringMode()
	{
		if (!IsScoreModeRunning)
		{
			IsScoreModeRunning = true;

			if (!game.IsBruceMultiballRunning())
				pinGod.EnableJawsToy(true);

			UpdateLamps();
			SetProcessInput(true);
			pinGod.LogInfo("scoring mode started");
		}
		else
		{
			pinGod.LogInfo("scoring mode already started");
		}
	}

	public void StopScoringMode()
	{
		if (IsScoreModeRunning)
		{
			IsScoreModeRunning = false;
		}

		game.IsJawsLockReady();
		SetProcessInput(false);
		UpdateLamps();
		this.Visible = false;
	}

	/// <summary>
	/// Updates the jackpot lights and jaws target for bruce multiball
	/// </summary>
	public void UpdateLamps()
	{
		if (IsScoreModeRunning)
		{
			pinGod.SetLampState("jp_0", 2);
			pinGod.SetLampState("jp_1", 2);
			pinGod.SetLampState("jp_2", 2);
			pinGod.SetLampState("jp_3", 2);
		}
		else
		{
			pinGod.SetLampState("jp_0", 0);
			pinGod.SetLampState("jp_1", 0);
			pinGod.SetLampState("jp_2", 0);
			pinGod.SetLampState("jp_3", 0);
			pinGod.SetLampState("jaws_l", 0);
			pinGod.SetLampState("jaws_r", 0);
		}
	}

	private void _on_Tween_tween_completed(Godot.Object @object, NodePath key)
	{
		pinGod.LogInfo("tween completed");
		if (!_displayBusy)
		{
			Visible = false;
		}
	}
	private void EndMultiball()
	{
        if (IsScoreModeRunning)
        {
			pinGod.LogInfo("mscoring: end multiball");
			StopScoringMode();
		}		
	}

	/// <summary>
	/// Get the score depending on balls in play
	/// </summary>
	/// <param name="bumpers">Special bumper score?</param>
	/// <returns></returns>
	int GetScore(bool bumpers)
	{
		var balls = pinGod.BallsInPlay();
		if (bumpers)
			return balls * 10000;
		else
			return balls * JackpotSeedValue;
	}

	private void PlayScoreScene(Texture _texture)
	{
		pinGod.PlayVoice("jackpot");
		if (!_displayBusy)
		{
			if (!_tween.IsActive())
			{
				if (!Visible) Visible = true;
				_iconLayer.Texture = _texture;
				_tween.InterpolateProperty(_iconLayer, "rect_position", new Vector2(-173, 0), new Vector2(570, 0), 1);
				_tween.Start();
			}			
		}
	}
}
