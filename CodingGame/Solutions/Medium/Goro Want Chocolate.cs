using System;

class Solution
{
    static void Main()
    {
        var parts = Console.ReadLine().Split();
        int H = int.Parse(parts[0]);
        int W = int.Parse(parts[1]);

        var dp = new int[H + 1][];
        for (int h = 0; h <= H; h++)
            dp[h] = new int[W + 1];

        for (int h = 1; h <= H; h++)
        {
            for (int w = 1; w <= W; w++)
            {
                if (h == w)
                {
                    dp[h][w] = 1;
                }
                else
                {
                    int best = int.MaxValue;
                    for (int k = 1; k <= h / 2; k++)
                        best = Math.Min(best, dp[k][w] + dp[h - k][w]);
                    for (int k = 1; k <= w / 2; k++)
                        best = Math.Min(best, dp[h][k] + dp[h][w - k]);
                    dp[h][w] = best;
                }
            }
        }

        Console.WriteLine(dp[H][W]);
    }
}
