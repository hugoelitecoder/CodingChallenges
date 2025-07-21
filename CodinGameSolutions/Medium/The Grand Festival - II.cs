using System;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        var N = int.Parse(Console.ReadLine().Trim());
        var R = int.Parse(Console.ReadLine().Trim());
        var prizes = new long[N];
        for (var i = 0; i < N; i++)
            prizes[i] = long.Parse(Console.ReadLine().Trim());

        var dp = new long[N+1][];
        for (var i = 0; i <= N; i++)
            dp[i] = new long[R+1];
        
        for (int i = N-1; i >= 0; i--)
        {
            for (int j = 0; j <= R; j++)
            {
                long best = dp[i+1][0];
                if (j < R)
                {
                    best = Math.Max(best, prizes[i] + dp[i+1][j+1]);
                }
                dp[i][j] = best;
            }
        }

        var result = new List<int>();
        int consec = 0;
        for (int i = 0; i < N; i++)
        {
            long skipVal = dp[i+1][0];
            long playVal = consec < R ? prizes[i] + dp[i+1][consec+1] : long.MinValue;
            if (playVal >= skipVal)
            {
                result.Add(i+1);
                consec++;
            }
            else
            {
                consec = 0;
            }
        }

        Console.WriteLine(string.Join('>', result));
    }
}
