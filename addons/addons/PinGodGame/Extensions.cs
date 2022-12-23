using System;
using System.Globalization;
using System.Linq;

/// <summary>
/// C# Extensions for int, string
/// </summary>
public static class PinGodExtensions
{
    /// <summary>
    /// Formats the score
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static string ToScoreString(this long points) => points.ToString("N0", CultureInfo.InvariantCulture);

    /// <summary>
    /// Formats the score
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static string ToScoreString(this int points) => points.ToString("N0", CultureInfo.InvariantCulture);

    /// <summary>
    /// Randomizes an array and returns a given length
    /// </summary>
    /// <param name="arr"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static int[] ShuffleArray(this int[] arr, int length)
    {
        Random random = new Random();
        return arr.OrderBy(x => random.Next()).Take(length).ToArray();
    }
}
