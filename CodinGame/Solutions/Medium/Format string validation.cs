using System;
using System.Text.RegularExpressions;
using System.Text;

class Solution
{
    static void Main()
    {
        var text = Console.ReadLine();
        var fmt  = Console.ReadLine();
        var regex = "^" + ToRegex(fmt) + "$";
        Console.WriteLine(
            Regex.IsMatch(text, regex)
              ? "MATCH"
              : "FAIL"
        );
    }

    static string ToRegex(string pat)
    {
        var sb = new StringBuilder();
        foreach (char c in pat)
        {
            switch (c)
            {
                case '?': sb.Append('.');   break;
                case '~': sb.Append(".*");  break;
                default:  sb.Append(Regex.Escape(c.ToString())); break;
            }
        }
        return sb.ToString();
    }
}
