public interface IPinballSendReceive
{
	bool LogActions { get; set; }
	int ReceivePort { get; set; }
	bool Record { get; set; }	
	int SendPort { get; set; }
	/// <summary>
	/// byte[,]
	/// </summary>
	/// <param name="json"></param>
	void SendCoilStates(string json);
	/// <summary>
	/// byte[,]
	/// </summary>
	/// <param name="json"></param>
	void SendLampStates(string json);
	/// <summary>
	/// Sets up <see cref="receiver"/> to listen for actions from Sim. Connects the sender. Sends Ready to controllers.
	/// </summary>
	void Start();
	/// <summary>
	/// Stops receiving and sending
	/// </summary>
	void Stop();
    void SaveRecording();
    void SendLedStates(string v);
}