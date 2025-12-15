using System.Text.RegularExpressions;

namespace MT5Bridge.Core.Text;

public static class StringX
{
    private static readonly Regex RegexByParentheses = new(@"^\((.*?)\):\((.*?)\)$");

    /// <summary>
    /// Splits input string in the format "(string1):(string2)" and returns a tuple (string1, string2, true).
    /// Returns (empty, empty, false) if parsing fails.
    /// </summary>
    public static (string, string, bool) SplitParenthesesTuple(string? strContent)
    {
        if (strContent == null)
        {
            return (string.Empty, string.Empty, false);
        }
        var match = RegexByParentheses.Match(strContent);
        if (match.Success && match.Groups.Count == 3)
        {
            string str1 = match.Groups[1].Value;
            string str2 = match.Groups[2].Value;
            return (str1, str2, true);
        }
        return (string.Empty, string.Empty, false);
    }

    /// <summary>
    /// Splits string by separator into two parts.
    /// Returns (part1, part2, true) on success, (empty, empty, false) on failure.
    /// </summary>
    public static (string, string, bool) SplitBy(string? strContent, string separator)
    {
        if (strContent == null)
        {
            return (string.Empty, string.Empty, false);
        }
        string[] parts = strContent.Split(separator);
        if (parts.Length != 2)
        {
            return (string.Empty, string.Empty, false);
        }

        return (parts[0], parts[1], true);
    }

    /// <summary>
    /// Extracts value from args string, like: "/name:xSyphon Gateway|/address:127.0.0.1:16385|/login:1"
    /// </summary>
    public static string? ExtractStartUpParamValue(string input, string key)
    {
        var pattern = $@"\/{key}:([^|]*)";
        var match = Regex.Match(input, pattern);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Sanitizes command line arguments by masking password values.
    /// </summary>
    public static string ArgsRemovePassword(string[] args)
    {
        var sanitizedArgs = args.Select(arg =>
        {
            if (arg.StartsWith("/password:", StringComparison.OrdinalIgnoreCase))
            {
                return "/password:******";
            }
            return arg;
        });
        return string.Join("|", sanitizedArgs);
    }
}
