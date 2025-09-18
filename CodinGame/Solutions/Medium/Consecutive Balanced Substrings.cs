using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        string s = Console.ReadLine();

        //prefix sum
        int[] ps = new int[n + 1];
        for (int i = 0; i < n; i++)
            ps[i + 1] = ps[i] + (s[i] == '1' ? 1 : -1);

        //positions
        var map = new Dictionary<int, List<int>>();
        for (int i = 0; i <= n; i++)
        {
            if (!map.TryGetValue(ps[i], out var list))
            {
                list = new List<int>();
                map[ps[i]] = list;
            }
            list.Add(i);
        }

        //max consecutive balanced substrings
        var dp = new int[n + 1];
        int answer = 0;
        for (int i = n - 1; i >= 0; i--)
        {
            int best = 0;
            foreach (int j in map[ps[i]])
            {
                if (j <= i) continue;
                best = Math.Max(best, 1 + dp[j]);
            }
            dp[i] = best;
            answer = Math.Max(answer, best);
        }

        Console.WriteLine(answer);
    }
}