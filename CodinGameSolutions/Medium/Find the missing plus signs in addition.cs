using System;
using System.Collections.Generic;

class Solution
{
    static int termCount;
    static long target;
    static string src;
    static List<string> answers = new List<string>();

    static void Main()
    {
        termCount = int.Parse(Console.ReadLine());
        target    = long.Parse(Console.ReadLine());
        src       = Console.ReadLine();
        DFS(0, 0, new List<string>());
        if (answers.Count == 0)
            Console.WriteLine("No solution");
        else
        {
            answers.Sort(StringComparer.Ordinal);
            foreach (var lhs in answers)
                Console.WriteLine($"{lhs}={target}");
        }
    }

    static void DFS(int i, long sum, List<string> parts)
    {
        int rem = termCount - parts.Count;
        if (rem == 1)
        {
            string s = src.Substring(i);
            if ((s.Length == 1 || s[0] != '0')
             && long.TryParse(s, out var v)
             && sum + v == target)
            {
                parts.Add(s);
                answers.Add(string.Join("+", parts));
                parts.RemoveAt(parts.Count - 1);
            }
            return;
        }

        int maxLen = src.Length - i - (rem - 1);
        for (int len = 1; len <= maxLen; len++)
        {
            if (len > 1 && src[i] == '0') break;
            string s = src.Substring(i, len);
            if (!long.TryParse(s, out var v)) continue;
            if (sum + v > target) continue;
            parts.Add(s);
            DFS(i + len, sum + v, parts);
            parts.RemoveAt(parts.Count - 1);
        }
    }
}
