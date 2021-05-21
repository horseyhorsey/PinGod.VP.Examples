public interface IPinballSendReceive
{
	bool LogActions { get; set; }
	void PulseCoilState(byte coil, byte pulseTime = 125);
	int ReceivePort { get; set; }
	bool Record { get; set; }	
	int SendPort { get; set; }
	void SetLampState(int lampId, int lampState);
    void SetCoilState(byte coil, int state);
	/// <summary>
	/// Sets up <see cref="receiver"/> to listen for actions from Sim. Connects the sender. Sends Ready to controllers.
	/// </summary>
	void Start();
	/// <summary>
	/// Stops receiving and sending
	/// </summary>
	void Stop();
    void SaveRecording();
}
