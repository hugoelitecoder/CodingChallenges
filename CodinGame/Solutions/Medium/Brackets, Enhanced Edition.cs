using System;
using System.Linq;

class Solution
{
    const string BR = "()[]{}<>";

    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        while (N-- > 0)
            Console.WriteLine(CanFlip(Console.ReadLine()) ? "true" : "false");
    }

    static bool CanFlip(string s)
    {
        var br = s.Where(c => BR.Contains(c)).ToArray();
        int m = br.Length;
        var t = br.Select(c => BR.IndexOf(c) / 2).ToArray();
        var dp = new bool[m + 1, m + 1];
        for (int i = 0; i <= m; i++) dp[i, i] = true;
        for (int len = 2; len <= m; len += 2)
            for (int i = 0; i + len <= m; i++)
            {
                int j = i + len;
                for (int k = i + 1; k < j; k += 2)
                    if (t[i] == t[k]
                     && dp[i + 1, k]
                     && dp[k + 1, j])
                    {
                        dp[i, j] = true;
                        break;
                    }
            }

        return dp[0, m];
    }
}
