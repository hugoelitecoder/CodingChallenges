using System;

class Solution
{
    static void Main()
    {
        int radius = int.Parse(Console.ReadLine());
        string center = Console.ReadLine();
        int n = center.Length;

        int[][] weights = new int[n][];
        for (int i = 0; i < n; i++)
        {
            int c = center[i] - 'a';
            int maxd = Math.Min(radius, Math.Max(c, 25 - c));
            var w = new int[radius + 1];
            for (int k = 0; k <= maxd; k++)
            {
                int cnt = 0;
                if (c + k <= 25) cnt++;
                if (k > 0 && c - k >= 0) cnt++;
                w[k] = cnt;
            }
            weights[i] = w;
        }

        var dp = new int[radius + 1];
        var next = new int[radius + 1];

        Array.Copy(weights[0], dp, radius + 1);

        for (int i = 1; i < n; i++)
        {
            Array.Clear(next, 0, next.Length);
            var w = weights[i];
            for (int dist = 0; dist <= radius; dist++)
            {
                int ways = dp[dist];
                if (ways == 0) continue;
                for (int k = 0; k + dist <= radius; k++)
                {
                    int cnt = w[k];
                    if (cnt == 0) continue;
                    next[dist + k] += ways * cnt;
                }
            }
            var tmp = dp; dp = next; next = tmp;
        }

        long ans = 0;
        for (int j = 0; j <= radius; j++)
            ans += dp[j];

        Console.WriteLine(ans);
    }
}
