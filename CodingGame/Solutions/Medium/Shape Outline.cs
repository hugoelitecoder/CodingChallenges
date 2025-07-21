using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        int H = int.Parse(Console.ReadLine());
        int W = int.Parse(Console.ReadLine());
        int S = int.Parse(Console.ReadLine());

        bool[,] pix = new bool[H, W];
        for (int r = 0; r < H; r++)
        {
            var line = Console.ReadLine();
            for (int c = 0; c < W; c++)
                pix[r, c] = (line[c] == '#');
        }

        int VH = H + 1, VW = W + 1, N = VH * VW;
        var edges = new HashSet<long>();

        void Toggle(int u, int v)
        {
            if (u > v) (u, v) = (v, u);
            long key = ((long)u << 32) | (uint)v;
            if (!edges.Remove(key)) edges.Add(key);
        }

        for (int r = 0; r < H; r++)
        for (int c = 0; c < W; c++)
        {
            if (!pix[r, c]) continue;
            int tl = r * VW + c;
            int tr = tl + 1;
            int bl = (r + 1) * VW + c;
            int br = bl + 1;
            Toggle(tl, tr);
            Toggle(tr, br);
            Toggle(br, bl);
            Toggle(bl, tl);
        }

        var adj = edges
            .Select(k => new { u = (int)(k >> 32), v = (int)(k & 0xffffffff) })
            .SelectMany(p => new[] { p, new { u = p.v, v = p.u } })
            .GroupBy(p => p.u, p => p.v)
            .ToDictionary(g => g.Key, g => g.ToList());

        int start = adj.Keys.Min();
        int sr = start / VW, sc = start % VW;
        int next = adj[start].First(v => v / VW == sr && v % VW > sc);

        var outline = new List<(int x,int y)>();
        int prev = start, cur = next;
        outline.Add((sc * S, sr * S));
        outline.Add((cur % VW * S, cur / VW * S));

        while (true)
        {
            var nbrs = adj[cur];
            int nxt = nbrs[0] == prev ? nbrs[1] : nbrs[0];
            if (nxt == start) break;
            outline.Add((nxt % VW * S, nxt / VW * S));
            prev = cur;
            cur = nxt;
        }

        var result = new List<(int x,int y)>();
        int M = outline.Count;
        for (int i = 0; i < M; i++)
        {
            var (x0,y0) = outline[(i-1+M)%M];
            var (x1,y1) = outline[i];
            var (x2,y2) = outline[(i+1)%M];
            if ((x1-x0)*(y2-y1) != (y1-y0)*(x2-x1))
                result.Add((x1,y1));
        }

        foreach (var (x,y) in result)
            Console.WriteLine($"{x} {y}");
    }
}
