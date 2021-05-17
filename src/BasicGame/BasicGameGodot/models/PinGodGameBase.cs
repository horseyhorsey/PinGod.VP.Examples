using Godot;
using System.Collections.Generic;

public class PinGodGameBase : Node
{
	#region Public Properties - Standard Pinball / Players

	public const byte TroughSolenoid = 1;
	public const byte FlippersEnableCoil = 2;
	public const byte CreditButtonNum = 2;
	public const byte SlamTiltSwitch = 16;
	public const byte TiltSwitchNum = 17;
	public const byte StartSwitchNum = 19;	
	public static byte Credits { get; set; }
	public static byte CurrentPlayerIndex { get; set; }
	public static byte BallInPlay { get; set; }
	public static byte BallsPerGame { get; set; } = 3;
	public static bool GameInPlay { get; set; }
	public static PinGodPlayer Player { get; set; }
	public static List<PinGodPlayer> Players { get; set; }
	public static bool IsTilted { get; set; }
	public static byte Tiltwarnings { get; set; }
	public static bool InBonusMode = false;
	public static byte MaxPlayers = 4;
	public static byte FlippersEnabled = 0;
	#endregion

	public PinGodGameBase()
    {
		Players = new List<PinGodPlayer>();
    }

	public void EnableFlippers(byte enabled)
	{
		FlippersEnabled = enabled;
		OscService.SetCoilState(FlippersEnableCoil, enabled);
	}
}
