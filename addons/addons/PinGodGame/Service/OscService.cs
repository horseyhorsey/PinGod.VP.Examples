using Godot;
using Rug.Osc;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Godot.GD;

/// <summary>
/// Godot singleton to send/receive OSC message over loopback
/// </summary>
public class OscService : Node
{
	private OscReceiver receiver;
	private Task oscTask;
	private CancellationToken _token;
	private static OscSender sender;
	CancellationTokenSource _tokenSource;

	public int SendPort { get; set; } = 9001;
	public int ReceivePort { get; set; } = 9000;
	public bool LogActions { get; set; } = false;

	public static uint GameStartTime { get; private set; }

	StringBuilder stringBuilder;

	/// <summary>
	/// Records the actions / switches
	/// </summary>
	public bool RecordGame { get; set; } = false;

	/// <summary>
	/// Sets up <see cref="receiver"/> to listen for actions from Sim. Connects the sender. Sends Ready to controllers.
	/// </summary>
	public override void _Ready()
	{
		receiver = new OscReceiver(IPAddress.Loopback, ReceivePort);
		receiver.Connect();

		//create then start new task and listen new thread...
		oscTask = new Task(() =>
		{
			while (!_token.IsCancellationRequested)
			{
				var packet = receiver.Receive();
				var bytes = packet.ToByteArray();
				var message = OscMessage.Read(bytes, bytes.Length);

				//Print($"address:{message.Address} - args:{message.Count}");
				if (message.Address == "/action")
				{
					if (message.Count == 2)
					{
						bool actionState = (bool)Convert(message[1], Variant.Type.Bool);
						var ev = new InputEventAction() { Action = message[0].ToString(), Pressed = actionState };
						if (LogActions) { Print($"in:{ev.Action}-state:{ev.Pressed}"); }

						if (RecordGame)
						{
							var switchTime = OS.GetTicksMsec() - GameStartTime;
							var recordLine = $"{ev.Action}|{ev.Pressed}|{switchTime}";
							stringBuilder?.AppendLine(recordLine);
						}
						Input.ParseInputEvent(ev);
					}
				}
			}

		}, _token);
		oscTask.Start();

		if (sender == null)
		{
			sender = new OscSender(IPAddress.Loopback, SendPort);
			sender.Connect();
		}

		Print("OSC ready. Sending game_ready");
		SendGameReady(); //Let the listeners know that the game is ready

		//Game start time, fully loaded...
		GameStartTime = OS.GetTicksMsec();
		if (RecordGame)
		{
			stringBuilder = new StringBuilder();
		}
	}

	public override void _EnterTree()
	{
		_tokenSource = new CancellationTokenSource();
		_token = _tokenSource.Token;
		_token.Register(() => Print("osc receive task stopped"));
	}

	public override void _ExitTree()
	{
		Print("exiting osc service");
		if (oscTask.Status == TaskStatus.Running)
		{
			SetCoilState(0, 1); //send game ended to VP via a coil. User could close this window first.
			_tokenSource.Cancel();
		}
		Print("exited OSC service");
	}

	/// <summary>
	/// Sends game_ready message over the /evt address
	/// </summary>
	public static void SendGameReady()
	{
		sender.Send(new OscMessage("/evt", "game_ready"));
	}

	/// <summary>
	/// Sends message over the /coils address
	/// </summary>
	/// <param name="num"></param>
	/// <param name="state"></param>
	public static void SetCoilState(byte num, int state)
	{
		sender.Send(new OscMessage("/coils", num, state));
	}

	/// <summary>
	/// Sends message over the /lamps address
	/// </summary>
	/// <param name="lampId"></param>
	/// <param name="lampState"></param>
	public static void SetLampState(int lampId, int lampState)
	{
		sender.Send(new OscMessage("/lamps", lampId, lampState));
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWmQuitRequest)
		{
			if (RecordGame)
			{
				Print("saving record file");
				var userDir = OS.GetUserDataDir();
				using (var txtFile = System.IO.File.CreateText(userDir + "/recording.txt"))
				{
					txtFile.Write(stringBuilder.ToString());
				}
				stringBuilder.Clear();
				Print("file saved");
			}
		}
	}

	/// <summary>
	/// Listens for the Quit action
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("quit"))
		{
			GetTree().Quit(0);
		}
	}

	/// <summary>
	/// Runs task to pulse 255ms. TODO: why new task?
	/// </summary>
	/// <param name="coilId"></param>
	public static void PulseCoilState(byte coilId, byte pulseTime = 125)
	{
		Task.Run(() =>
		{
			sender.Send(new OscMessage("/coils", coilId, 1));
			//sender.WaitForAllMessagesToComplete();
			Task.Delay(pulseTime).Wait();
			sender.Send(new OscMessage("/coils", coilId, 0));
		});
	}
}
