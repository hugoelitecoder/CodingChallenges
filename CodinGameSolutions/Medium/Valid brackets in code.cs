using System;
using System.Collections.Generic;

class Solution
{
    const char StringDelim = '"';
    const char EscapeChar  = '\\';
    const string Openers   = "([{";
    const string Closers   = ")]}";

    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var stack = new Stack<char>();
        bool inString = false, sawBracket = false, invalid = false;

        for (int line = 0; line < n && !invalid; line++)
        {
            string codeLine = Console.ReadLine();
            for (int i = 0; i < codeLine.Length && !invalid; i++)
            {
                char c = codeLine[i];
                if (inString)
                {
                    if (c == EscapeChar && i + 1 < codeLine.Length)
                        i++;
                    else if (c == StringDelim)
                        inString = false;
                }
                else
                {
                    if (c == StringDelim)
                    {
                        inString = true;
                    }
                    else if (Openers.IndexOf(c) >= 0)
                    {
                        sawBracket = true;
                        stack.Push(c);
                    }
                    else if (Closers.IndexOf(c) >= 0)
                    {
                        sawBracket = true;
                        if (stack.Count == 0 ||
                            Openers.IndexOf(stack.Pop()) != Closers.IndexOf(c))
                            invalid = true;
                    }
                }
            }
        }

        string result = !sawBracket ? "No brackets"
                      : invalid || stack.Count > 0 ? "Invalid"
                                                   : "Valid";
        Console.WriteLine(result);
    }
}
