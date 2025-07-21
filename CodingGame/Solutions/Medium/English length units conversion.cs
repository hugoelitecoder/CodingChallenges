using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var first = Console.ReadLine();
        var p0 = first.Split(new[] { " in " }, StringSplitOptions.None);
        var src = p0[0];
        var dst = p0[1];
        int n = int.Parse(Console.ReadLine());

        var graph = new Dictionary<string, List<(string to, long num, long den)>>();

        for (int i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            var p = line.Split(new[] { " = " }, StringSplitOptions.None);
            var left = p[0].Split(new[] { ' ' }, 2);
            var right = p[1].Split(new[] { ' ' }, 2);
            long a = long.Parse(left[0]);
            string u = left[1];
            long b = long.Parse(right[0]);
            string v = right[1];

            if (!graph.ContainsKey(u)) graph[u] = new List<(string, long, long)>();
            if (!graph.ContainsKey(v)) graph[v] = new List<(string, long, long)>();
            graph[u].Add((v, b, a));
            graph[v].Add((u, a, b));
        }

        var seen = new HashSet<string> { src };
        var queue = new Queue<(string u, long num, long den)>();
        queue.Enqueue((src, 1L, 1L));

        long num = 0, den = 0;
        while (queue.Count > 0)
        {
            var (u, n0, d0) = queue.Dequeue();
            if (u == dst)
            {
                num = n0;
                den = d0;
                break;
            }
            foreach (var (v, n1, d1) in graph[u])
                if (seen.Add(v))
                    queue.Enqueue((v, n0 * n1, d0 * d1));
        }

        long g = Gcd(num, den);
        num /= g;
        den /= g;

        Console.WriteLine(
            den == 1
                ? $"1 {src} = {num} {dst}"
                : $"{den} {src} = {num} {dst}"
        );
    }

    static long Gcd(long a, long b)
    {
        while (b != 0)
        {
            long t = b;
            b = a % b;
            a = t;
        }
        return a;
    }
}
