using System;
class Solution
{
    public static void Main(string[] args)
    {
        var N = int.Parse(Console.ReadLine());
        var xs = new int[N];
        var ys = new int[N];
        for (var i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split();
            xs[i] = int.Parse(parts[0]);
            ys[i] = int.Parse(parts[1]);
        }
        var visited = new bool[N];
        var current = 0;
        visited[0] = true;
        var count = 1;
        var total = 0.0;
        while (count < N)
        {
            var minDist = double.MaxValue;
            var minIdx = -1;
            for (var i = 0; i < N; i++)
            {
                if (visited[i]) continue;
                var dx = xs[i] - xs[current];
                var dy = ys[i] - ys[current];
                var d = Math.Sqrt(dx * dx + dy * dy);
                if (d < minDist || (d == minDist && i < minIdx))
                {
                    minDist = d;
                    minIdx = i;
                }
            }
            visited[minIdx] = true;
            total += minDist;
            current = minIdx;
            count++;
        }
        var dx0 = xs[current] - xs[0];
        var dy0 = ys[current] - ys[0];
        total += Math.Sqrt(dx0 * dx0 + dy0 * dy0);
        var result = (int)Math.Round(total, MidpointRounding.AwayFromZero);
        Console.WriteLine(result);
    }
}
