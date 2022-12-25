/// <summary>
/// For playing back record games, events
/// </summary>
public class PlaybackEvent
{
    /// <summary>
    /// Initializes
    /// </summary>
    /// <param name="evtName"></param>
    /// <param name="state"></param>
    /// <param name="time"></param>
    public PlaybackEvent(string evtName, bool state, uint time)
    {
        EvtName = evtName;
        State = state;
        Time = time;
    }
    /// <summary>
    /// event name
    /// </summary>
    public string EvtName { get; }
    /// <summary>
    /// event state
    /// </summary>
    public bool State { get; }
    /// <summary>
    /// At what time event happened
    /// </summary>
    public uint Time { get; }
}