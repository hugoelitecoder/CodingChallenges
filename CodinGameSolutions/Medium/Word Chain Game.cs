using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int n      = int.Parse(Console.ReadLine());
        var words  = Enumerable.Range(0, n)
                              .Select(_ => Console.ReadLine().Trim())
                              .ToArray();
        var starts = words.Select(w => w[0]).ToArray();
        var ends   = words.Select(w => w[^1]).ToArray();
        var adj    = Enumerable.Range(0, n + 1)
                    .Select(i => (i == n
                        ? Enumerable.Range(0, n)
                        : Enumerable.Range(0, n).Where(j => starts[j] == ends[i])
                    ).ToList())
                    .ToArray();

        int total = 1 << n;
        var dp = new bool[total, n + 1];
        for (int mask = total - 1; mask >= 0; mask--)
        {
            for (int last = 0; last <= n; last++)
            {
                bool win = false;
                foreach (int nxt in adj[last])
                {
                    int bit = 1 << nxt;
                    if ((mask & bit) != 0) continue; 
                    if (!dp[mask | bit, nxt])
                    {
                        win = true;
                        break;
                    }
                }
                dp[mask, last] = win;
            }
        }

        Console.WriteLine(dp[0, n] ? "Alice" : "Bob");
    }
}
