public class BallSearchOptions
{
    public BallSearchOptions(string[] searchCoils, string[] stopSearchSwitches = null, int searchWaitTime = 10, bool _ball_search_enabled = false)
    {
        SearchCoils = searchCoils;
        StopSearchSwitches = stopSearchSwitches;
        SearchWaitTime = searchWaitTime;
        IsSearchEnabled = _ball_search_enabled;
    }

    public bool IsSearchEnabled { get; set; }
    public string[] SearchCoils { get; }
    public string[] StopSearchSwitches { get; }
    public int SearchWaitTime { get; }
}
