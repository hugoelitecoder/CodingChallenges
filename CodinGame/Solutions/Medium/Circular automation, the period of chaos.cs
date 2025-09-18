using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int lineLength = int.Parse(Console.ReadLine());
        int maxIter    = int.Parse(Console.ReadLine());
        int ruleNumber = int.Parse(Console.ReadLine());

        bool[] rule = new bool[8];
        for (int i = 0; i < 8; i++)
            rule[i] = ((ruleNumber >> i) & 1) == 1;

        var curr = new bool[lineLength];
        var next = new bool[lineLength];
        curr[lineLength/2] = true;

        var seen = new Dictionary<string,int>();

        string s = ToStr(curr);
        Console.Error.WriteLine($"0: {s}");
        seen[s] = 0;

        for (int iter = 1; iter <= maxIter; iter++)
        {
            for (int x = 0; x < lineLength; x++)
            {
                int left  = curr[(x - 1 + lineLength) % lineLength] ? 1 : 0;
                int self  = curr[x]                                 ? 1 : 0;
                int right = curr[(x + 1)         % lineLength]    ? 1 : 0;
                int idx   = (left << 2) | (self << 1) | right;
                next[x] = rule[idx];
            }

            var tmp = curr;
            curr = next;
            next = tmp;

            s = ToStr(curr);
            Console.Error.WriteLine($"{iter}: {s}");

            if (seen.TryGetValue(s, out int firstIter))
            {
                Console.WriteLine(iter - firstIter);
                return;
            }
            seen[s] = iter;
        }

        Console.WriteLine("BIG");
    }

    static string ToStr(bool[] a)
    {
        return new string(a.Select(b => b ? '1' : '.').ToArray());
    }
}
