using System;

class Solution
{
    public static void Main()
    {
        var N = int.Parse(Console.ReadLine().Trim());
        var T = 50 - N;
        if (T <= 0 || T > 48) { Console.WriteLine(0); return; }
        var M = new int[13];
        M[1] = 1;
        for (var v = 2; v <= 12; v++) M[v] = 2;
        var dp = new int[5, 51];
        dp[0, 0] = 1;
        for (var l = 0; l < 4; l++)
            for (var s = 0; s <= T; s++)
            {
                var ways = dp[l, s];
                if (ways == 0) continue;
                for (var v = 1; v <= 12; v++)
                {
                    var s2 = s + v;
                    if (s2 > T) break;
                    dp[l + 1, s2] += ways * M[v];
                }
            }
        var result = 0;
        for (var l = 1; l <= 4; l++) result += dp[l, T];
        Console.WriteLine(result);
    }
}
