using System;
using System.Text.RegularExpressions;

class Solution
{
    private static readonly Regex StripNonBrackets = new Regex(@"[^\[\]\(\)\{\}\<\>]");

    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        while (N-- > 0)
        {
            string expr = StripNonBrackets
                            .Replace(Console.ReadLine(), "")
                            .Replace(")", "(")
                            .Replace("]", "[")
                            .Replace("}", "{")
                            .Replace(">", "<");

            if ((expr.Length & 1) == 1 || expr.Length > 25)
            {
                Console.WriteLine("false");
                continue;
            }

            bool valid = DFS(expr, 0, "");
            Console.WriteLine(valid ? "true" : "false");
        }
    }

    private static bool DFS(string expr, int i, string st)
    {
        if (i == expr.Length)
            return st.Length == 0;
        char b = expr[i];
        if (st.Length > 0 && st[^1] == b)
        {
            if (DFS(expr, i + 1, st[..^1]))
                return true;
        }
        return DFS(expr, i + 1, st + b);
    }
}
