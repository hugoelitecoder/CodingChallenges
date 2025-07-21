using System;
using System.Text;
using System.Text.RegularExpressions;

class Solution
{
    static void Main()
    {
        string input = Console.ReadLine() ?? string.Empty;
        string result = FormatText(input);
        Console.WriteLine(result);
    }

    static string FormatText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        text = Regex.Replace(text, "\\s+", " ");
        text = Regex.Replace(text, "([\\p{P}])(?:\\s*\\1)+", "$1");
        text = Regex.Replace(text, "\\s+([\\p{P}])", "$1");
        text = Regex.Replace(text, "([\\p{P}])(?=[A-Za-z0-9])", "$1 ");
        text = Regex.Replace(text, "([\\p{P}])\\1+", "$1");
        text = Regex.Replace(text, " \\(?([\\p{P}])", "$1");
        text = Regex.Replace(text, "([\\p{P}])(?=[A-Za-z0-9])", "$1 ");
        text = text.ToLowerInvariant();

        var sb = new StringBuilder(text.Length);
        bool newSentence = true;
        foreach (char c in text)
        {
            if (newSentence && char.IsLetter(c))
            {
                sb.Append(char.ToUpperInvariant(c));
                newSentence = false;
            }
            else
            {
                sb.Append(c);
                if (c == '.') newSentence = true;
                else if (!char.IsWhiteSpace(c) && c != '\r' && c != '\n')
                    newSentence = false;
            }
        }
        return sb.ToString().Trim();
    }
}
