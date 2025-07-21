using System;
using System.Collections.Generic;

class Solution {
    
    static void Main() {
        int H = int.Parse(Console.ReadLine()), W = int.Parse(Console.ReadLine());
        int w = Math.Min(H, W), h = Math.Max(H, W), M = 1 << w;
        var trans = new List<int>[M];
        for (int m = 0; m < M; m++)
            trans[m] = BuildNextMasks(m, w);

        var dp = new long[M];
        dp[0] = 1;
        for (int c = 0; c < h; c++) {
            var ndp = new long[M];
            for (int m = 0; m < M; m++)
                if (dp[m] != 0)
                    foreach (var nm in trans[m])
                        ndp[nm] += dp[m];
            dp = ndp;
        }

        Console.WriteLine(dp[0]);
    }

    private static List<int> BuildNextMasks(int mask, int w) {
        var list = new List<int>();
        void Dfs(int r, int next) {
            if (r == w) { list.Add(next); return; }
            if ((mask & (1 << r)) != 0)
                Dfs(r + 1, next);
            else {
                if (r + 1 < w && (mask & (1 << (r + 1))) == 0)
                    Dfs(r + 2, next);
                Dfs(r + 1, next | (1 << r));
            }
        }
        Dfs(0, 0);
        return list;
    }
}
