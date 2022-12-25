/// <summary>
/// Ball search with coils, switches
/// </summary>
public class BallSearchOptions
{
    /// <summary>
    /// Init properties
    /// </summary>
    /// <param name="searchCoils"></param>
    /// <param name="stopSearchSwitches"></param>
    /// <param name="searchWaitTime"></param>
    /// <param name="_ball_search_enabled"></param>
    public BallSearchOptions(string[] searchCoils, string[] stopSearchSwitches = null, int searchWaitTime = 10, bool _ball_search_enabled = false)
    {
        SearchCoils = searchCoils;
        StopSearchSwitches = stopSearchSwitches;
        SearchWaitTime = searchWaitTime;
        IsSearchEnabled = _ball_search_enabled;
    }

    /// <summary>
    /// Enabled?
    /// </summary>
    public bool IsSearchEnabled { get; set; }
    /// <summary>
    /// Coil names to search for ball with in ball search
    /// </summary>
    public string[] SearchCoils { get; }
    /// <summary>
    /// Switch names that stop the ball search
    /// </summary>
    public string[] StopSearchSwitches { get; }
    /// <summary>
    /// Time to wait in seconds before searching
    /// </summary>
    public int SearchWaitTime { get; }
}
