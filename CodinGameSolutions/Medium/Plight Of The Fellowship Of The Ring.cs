using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var N  = int.Parse(Console.ReadLine());
        var M  = int.Parse(Console.ReadLine());
        var L  = int.Parse(Console.ReadLine());
        var xs = new int[N];
        var ys = new int[N];
        for (var i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split();
            xs[i] = int.Parse(parts[0]);
            ys[i] = int.Parse(parts[1]);
        }
        var orcs = new List<(int x,int y)>();
        for (var i = 0; i < M; i++)
        {
            var parts = Console.ReadLine().Split();
            orcs.Add((int.Parse(parts[0]), int.Parse(parts[1])));
        }
        var adj = new List<int>[N];
        for (var i = 0; i < N; i++) adj[i] = new List<int>();
        for (var i = 0; i < L; i++)
        {
            var parts = Console.ReadLine().Split();
            var u = int.Parse(parts[0]);
            var v = int.Parse(parts[1]);
            adj[u].Add(v);
            adj[v].Add(u);
        }
        var S = int.Parse(Console.ReadLine());
        var E = int.Parse(Console.ReadLine());

        var minSq = new long[N];
        for (var i = 0; i < N; i++) minSq[i] = long.MaxValue;
        for (var i = 0; i < N; i++)
            foreach (var (ox,oy) in orcs)
            {
                var dx = xs[i] - ox;
                var dy = ys[i] - oy;
                var d2 = (long)dx*dx + (long)dy*dy;
                if (d2 < minSq[i]) minSq[i] = d2;
            }

        var visited = new bool[N];
        var dist    = new int[N];
        var prev    = new int[N];
        var queue   = new Queue<int>();
        if (minSq[S] <= 0)
        {
            Console.WriteLine("IMPOSSIBLE");
            return;
        }

        visited[S] = true;
        dist[S]    = 0;
        prev[S]    = -1;
        queue.Enqueue(S);
        var found = false;
        while (queue.Count > 0)
        {
            var u = queue.Dequeue();
            if (u == E)
            {
                found = true;
                break;
            }
            var t  = dist[u] + 1;
            var t2 = (long)t*t;
            foreach (var v in adj[u])
            {
                if (visited[v] || minSq[v] <= t2) continue;
                visited[v] = true;
                dist[v]    = t;
                prev[v]    = u;
                queue.Enqueue(v);
            }
        }
        if (!found)
        {
            Console.WriteLine("IMPOSSIBLE");
            return;
        }

        var path = new Stack<int>();
        for (var cur = E; cur != -1; cur = prev[cur]) {
            path.Push(cur);
        }
        Console.WriteLine(string.Join(" ", path));
    }
}
