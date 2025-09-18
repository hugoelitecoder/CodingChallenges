using System;
class Solution
{
    public static void Main(string[] args)
    {
        var wh = Console.ReadLine().Split(' ');
        var w = int.Parse(wh[0]);
        var h = int.Parse(wh[1]);
        var grid = new int[h, w];
        for (var i = 0; i < h; i++)
        {
            var line = Console.ReadLine().Split(' ');
            for (var j = 0; j < w; j++)
                grid[i, j] = int.Parse(line[j]);
        }
        var dp = new int[h, w];
        dp[0, 0] = grid[0, 0];
        for (var i = 1; i < h; i++)
            dp[i, 0] = dp[i - 1, 0] + grid[i, 0];
        for (var j = 1; j < w; j++)
            dp[0, j] = dp[0, j - 1] + grid[0, j];
        for (var i = 1; i < h; i++)
            for (var j = 1; j < w; j++)
                dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]) + grid[i, j];
        Console.WriteLine(dp[h - 1, w - 1]);
    }
}
