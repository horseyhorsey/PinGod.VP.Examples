/// <summary>
/// Use this for snake casing
/// </summary>
public enum PinGodStretchAspect
{
    //
    // Summary:
    //     Fill the window with the content stretched to cover excessive space. Content
    //     may appear stretched.
    ignore = 0,
    //
    // Summary:
    //     Retain the same aspect ratio by padding with black bars on either axis. This
    //     prevents distortion.
    keep = 1,
    //
    // Summary:
    //     Expand vertically. Left/right black bars may appear if the window is too wide.
    keep_width = 2,
    //
    // Summary:
    //     Expand horizontally. Top/bottom black bars may appear if the window is too tall.
    keep_height = 3,
    //
    // Summary:
    //     Expand in both directions, retaining the same aspect ratio. This prevents distortion
    //     while avoiding black bars.
    expand = 4
}
