public class BallSearchOptions
{
    public BallSearchOptions(string[] searchCoils, string[] stopSearchSwitches = null, int searchWaitTime = 10)
    {
        SearchCoils = searchCoils;
        StopSearchSwitches = stopSearchSwitches;
        SearchWaitTime = searchWaitTime;
    }

    public bool IsSearchEnabled { get; }
    public string[] SearchCoils { get; }
    public string[] StopSearchSwitches { get; }
    public int SearchWaitTime { get; }

}
