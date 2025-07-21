using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int K = int.Parse(Console.ReadLine());
        long N = long.Parse(Console.ReadLine());
        int cur = int.Parse(Console.ReadLine());
        var pts = Enumerable.Range(0, K)
            .Select(_ => {
                var a = Console.ReadLine().Split().Select(int.Parse).ToArray();
                return (x: a[0], y: a[1]);
            })
            .ToArray();

        var cnt = new long[K];
        cnt[cur]++;
        var seen = new Dictionary<(int, int), (long step, long[] snapshot)>();
        int prev = -1;
        long done = 0;
        double PI = Math.PI;

        double ang(int u, int v)
        {
            var (xu, yu) = pts[u];
            var (xv, yv) = pts[v];
            double a = Math.Atan2(yv - yu, xv - xu);
            if (a < 0) a += 2 * PI;
            if (a >= PI) a -= PI;
            return a;
        }

        while (done < N)
        {
            if (prev >= 0)
            {
                var st = (cur, prev);
                if (seen.TryGetValue(st, out var old))
                {
                    long cycle = done - old.step;
                    long times = (N - done) / cycle;
                    if (times > 0)
                    {
                        for (int i = 0; i < K; i++)
                            cnt[i] += (cnt[i] - old.snapshot[i]) * times;
                        done += cycle * times;
                    }
                }
                else
                {
                    seen[st] = (done, (long[])cnt.Clone());
                }
            }
            if (done >= N) break;

            double baseAng = prev < 0 ? 0 : ang(cur, prev);
            double best = double.PositiveInfinity;
            int nxt = -1;
            for (int j = 0; j < K; j++) if (j != cur)
            {
                double a = ang(cur, j);
                double d = (baseAng - a + PI) % PI;
                if (j == prev || d < 1e-12) d = PI;
                if (d < best) { best = d; nxt = j; }
            }

            prev = cur;
            cur = nxt;
            cnt[cur]++;
            done++;
        }

        Console.WriteLine(cur);
        Console.WriteLine(string.Join("\n", cnt));
    }
}
