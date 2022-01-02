using Godot;
using System;
using System.Linq;

public class KeysMode : PinballTargetsControl
{
	private PackedScene _keyScene;
	private PlayboyPlayer _player;
	private PackedScene _grotoScene;
    private Game game;

    public override void _EnterTree()
	{
		base._EnterTree();

		_keyScene = ResourceLoader.Load("res://scenes/KeyCollected.tscn") as PackedScene;
		_grotoScene = ResourceLoader.Load("res://scenes/GrottoScene.tscn") as PackedScene;

		game = GetParent().GetParent() as Game;
	}

    public override void _Input(InputEvent @event)
    {
		base._Input(@event);

		//check the saucer switch, kick the ball if game isn't running
		if (pinGod.SwitchOn("saucer", @event))
		{
			if (pinGod.GameInPlay && !pinGod.IsTilted)
			{
				OnGrottoEntered();
			}
            else
            {
				pinGod.SolenoidPulse("saucer");
			}
		}		
	}

	/// <summary>
	/// making 5 1st time 2x, 3x, 5x
	/// </summary>
	/// <param name="reset"></param>
    public override void TargetsCompleted(bool reset = true)
	{
		pinGod.AddPoints(Game.MINSCORE);
		if (_player != null)
		{
			_player.GrottoComplete = true;
			_player.KeyFeatureComplete += 1;
			switch (_player.KeyFeatureComplete)
			{
				case 1:
					_player.BonusMultiplier = 2;
					break;
				case 2:
					_player.BonusMultiplier = 3;
					break;
				case 3:
					_player.BonusMultiplier = 5;
					break;					
				default:
					if(_player.LeftSpecialLit || _player.SpecialRightLit)
						pinGod.AddPoints(50000);
                    else
                    {
						pinGod.AddPoints(25000);
					}
					break;
			}
		}

		base.TargetsCompleted(reset);
	}

	public override bool SetTargetComplete(int index)
	{
		var result = base.SetTargetComplete(index);
		pinGod.AddPoints(Game.MINSCORE);
		
		if (index == 4 && _player != null)
		{
			_player.AdvanceBonus(5); //key targets advances bonus 5?
		}

		if (result)
		{
			if (_player != null)
			{
				_player.SpecialRightLit = true; //todo: wrong variable, needs arrow r
				//key targets advances bonus
				if (index == 4)
				{
					_player.AdvanceBonus(1);
				}
			}

			CallDeferred(nameof(AddKeyScene));
		}

		
		if (game != null)
		{
			game.UpdateLamps();
		}		

		return result;
	}

	private void AddKeyScene()
	{
		AddChild(_keyScene.Instance()); //todo: after instance set some text in the scene, like how many complete
	}

	public void OnBallDrained()
	{
		pinGod.LogInfo("keys: ball drained");
		this.ResetTargets();
		_player.KeyFeatureComplete = 0;
	}

	public void OnBallStarted()
	{
		_player = pinGod.Player as PlayboyPlayer;
		UpdateLamps();
	}

	private void AddGrottoScene()
	{
		var grotto = _grotoScene.Instance() as GrottoScene;
		if (_player != null)
		{			
			if(_player.KeyFeatureComplete > 0)
            {
				_player.AdvanceBonus(5);
				pinGod.AddPoints(30000);
				grotto.BonusAdvanced = 5;
				grotto.ScoreToDisplay = 30000;
			}
            else
            {
				var keyCount = this._targetValues.Where(x => x).Count();
				grotto.BonusAdvanced = keyCount;
				_player.AdvanceBonus((byte)keyCount);
				pinGod.AddPoints(1000 * keyCount);
				grotto.ScoreToDisplay = 1000 * keyCount;
			}
		}
		else
		{
			grotto.BonusAdvanced = 2;
		}
		AddChild(grotto);

		game.UpdateLamps();
	}

	/// <summary>
	/// Scores 1000 & advance bonus for each lit number
	/// </summary>
	private void OnGrottoEntered()
	{
		AudioServer.SetBusEffectEnabled(1, 0, true);
		pinGod.AddPoints(Game.MINSCORE * 2);
		if (_player != null)
		{
			if (_player.GrottoComplete) pinGod.AddPoints(25000);			
		}

		CallDeferred(nameof(AddGrottoScene));
	}

	public override void UpdateLamps()
	{
		base.UpdateLamps();		

		//updates bonus multipliers
		if(_player != null)
		{
			//grotto lamps
			if(_player.KeyFeatureComplete > 0)
			{
				pinGod.SetLampState("grot_0", 1);
				pinGod.SetLampState("grot_1", 1);
				pinGod.SetLampState("grot_2", 1);
				pinGod.SetLampState("grot_3", 1);
				pinGod.SetLampState("grot_4", 1);
				pinGod.SetLampState("grot_25k", 1);
			}
			else
			{
				for (int i = 0; i < this._targetValues.Length; i++)
				{
					pinGod.SetLampState("grot_"+i, _targetValues[i] ? (byte)1 : (byte)0);
				}
			}

			switch (_player.BonusMultiplier)
			{
				case 2:
					pinGod.SetLampState("x_2", 1);
					break;
				case 3:
					pinGod.SetLampState("x_2", 0);
					pinGod.SetLampState("x_3", 1);
					break;
				case 5:
					pinGod.SetLampState("x_3", 0);
					pinGod.SetLampState("x_5", 1);
					break;
				default:
					break;
			}
		}
	}
}
