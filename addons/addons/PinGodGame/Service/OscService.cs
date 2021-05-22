using Godot;
using Rug.Osc;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Godot.GD;

/// <summary>
/// Godot singleton to send/receive OSC message over loopback
/// </summary>
public class OscService : IPinballSendReceive
{
	public uint GameStartTime { get; private set; }
	public int SendPort { get; set; } = 9001;
	public int ReceivePort { get; set; } = 9000;
	public bool LogActions { get; set; } = false;

	private OscReceiver receiver;
	private OscSender sender;

	private Task oscTask;
	private CancellationToken _token;	
	CancellationTokenSource _tokenSource;

	/// <summary>
	/// recording actions
	/// </summary>
	StringBuilder stringBuilder;

	public OscService()
	{
		_tokenSource = new CancellationTokenSource();
		_token = _tokenSource.Token;
		_token.Register(() => Print("osc receive task stopped"));
	}
	/// <summary>
	/// Records the actions / switches
	/// </summary>
	public bool Record { get; set; } = false;
	/// <summary>
	/// Sends game_ready message over the /evt address
	/// </summary>
	public void SendGameReady()
	{
		sender?.Send(new OscMessage("/evt", "game_ready"));
	}
	public void SendCoilStates(string json)
	{
		sender?.Send(new OscMessage("/all_coils", json));
		//sender.WaitForAllMessagesToComplete();
	}
	public void SendLampStates(string json)
	{
		sender?.Send(new OscMessage("/all_lamps", json));
	}
	public void SendLedStates(string json)
	{
		//Print(json);
		sender?.Send(new OscMessage("/all_leds", json));
	}
	public void Start()
	{
		if(receiver != null)
		{
			Print("osc service already initialized");
			return;
		}

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

						if (Record)
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
		if (Record)
		{
			stringBuilder = new StringBuilder();
		}
	}	
	public void SaveRecording()
	{
		Print("saving record file");
		var userDir = OS.GetUserDataDir();
		var file = DateTime.Now.ToFileTime()+".record";
		using (var txtFile = System.IO.File.CreateText(userDir + $"/recordings/{file}"))
		{
			txtFile.Write(stringBuilder.ToString());
		}
		stringBuilder.Clear();
		Print("file saved");
	}
	public void Stop()
	{
		if (oscTask?.Status == TaskStatus.Running)
		{
			SendCoilStates("[[0, 1]]"); //send game ended to VP via a coil. User could close this window first.
			_tokenSource.Cancel();
		}
	}
}
