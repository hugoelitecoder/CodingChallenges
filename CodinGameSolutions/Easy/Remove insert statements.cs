using System;
using System.Text.RegularExpressions;

public class Solution
{
    public static void Main()
    {
        if (!int.TryParse(Console.ReadLine(), out var n)) return;
        var bypass = false;
        for (var i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            if (line == null) break;
            var result = line;
            var idx = result.IndexOf("--", StringComparison.Ordinal);
            var comment = idx >= 0 ? result[(idx + 2)..] : "";
            if (idx >= 0) result = result[..(idx + 2)];
            var trimmed = result.Trim();
            if (Regex.IsMatch(trimmed, "^BEGIN$", RegexOptions.IgnoreCase)) bypass = true;
            else if (Regex.IsMatch(trimmed, "^END;$",  RegexOptions.IgnoreCase)) bypass = false;
            var len = result.Length;
            if (!bypass
                && trimmed.IndexOf("INSERT ", StringComparison.OrdinalIgnoreCase) >= 0
                && trimmed.IndexOf(" INTO ",  StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (result.Contains(";"))
                    result = Regex.Replace(result, "(?i)INSERT [^;]*;", "");
                else
                {
                    do { line = Console.ReadLine(); i++; }
                    while (line != null && !line.Contains(";"));
                    result = "";
                }
            }
            if (!(len != result.Length && result == ""))
                Console.WriteLine(result + comment);
        }
    }
}
