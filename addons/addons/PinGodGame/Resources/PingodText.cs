using Pingod.Local.resources;
using System.Globalization;

public static class PingodText
{
    /// <summary>
    /// Used to ovverride the language
    /// </summary>
    /// <param name="culture"></param>
    public static void InitResourceManager(string culture)
    {
        if(!string.IsNullOrWhiteSpace(culture))
        {
            ResourceText.Culture = CultureInfo.GetCultureInfo(culture);
        }   
    }

    public static string Get(string key) => ResourceText.ResourceManager.GetString(key);
}