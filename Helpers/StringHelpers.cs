using System.Text.RegularExpressions;

namespace Helpers;

public class StringHelpers
{
    public static string StripHtml(string content)
    {
        return Regex.Replace(content, @"<.*?>", string.Empty);
    }
}