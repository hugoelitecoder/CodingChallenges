using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int L = int.Parse(Console.ReadLine());
        int N = int.Parse(Console.ReadLine());

        var lengths = new List<int>();
        var values = new List<long>();
        for (int i = 0; i < N; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            long len = long.Parse(inputs[0]);
            long val = long.Parse(inputs[1]);
            if (len <= L)
            {
                lengths.Add((int)len);
                values.Add(val);
            }
        }

        long[] dp = new long[L + 1];
        for (int i = 1; i <= L; i++)
        {
            long best = 0;
            for (int j = 0; j < lengths.Count; j++)
            {
                int len = lengths[j];
                if (len <= i)
                {
                    long candidate = dp[i - len] + values[j];
                    if (candidate > best)
                        best = candidate;
                }
            }
            dp[i] = best;
        }
        Console.WriteLine(dp[L]);
    }
}