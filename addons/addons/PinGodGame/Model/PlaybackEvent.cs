public class PlaybackEvent
{
    public PlaybackEvent(string evtName, bool state, uint time)
    {
        EvtName = evtName;
        State = state;
        Time = time;
    }
    public string EvtName { get; }
    public bool State { get; }
    public uint Time { get; }
}