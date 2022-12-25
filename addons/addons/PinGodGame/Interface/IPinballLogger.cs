/// <summary>
/// Logging wrapper around Godot's Print methods and PushErrors
/// </summary>
public interface IPinballLogger
{
    /// <summary>
    /// 
    /// </summary>
    PinGodLogLevel LogLevel { get; }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="what"></param>
    void LogDebug(params object[] what);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="what"></param>
    void LogError(string message = null, params object[] what);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="what"></param>
    void LogInfo(params object[] what);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="what"></param>
    void LogWarning(string message = null, params object[] what);
}
