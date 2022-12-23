/// <summary>
/// Window sizing
/// </summary>
public enum PinGodStretchAspect
{
    /// <summary>
    /// Fill the window with the content stretched to cover excessive space. Content may appear stretched.
    /// </summary>
    ignore = 0,
    /// <summary>
    /// Retain the same aspect ratio by padding with black bars on either axis. This prevents distortion.
    /// </summary>
    keep = 1,
    /// <summary>
    /// Expand vertically. Left/right black bars may appear if the window is too wide.
    /// </summary>
    keep_width = 2,
    /// <summary>
    /// Expand horizontally. Top/bottom black bars may appear if the window is too tall.
    /// </summary>
    keep_height = 3,
    /// <summary>
    /// Expand in both directions, retaining the same aspect ratio. This prevents distortion while avoiding black bars.
    /// </summary>
    expand = 4
}
