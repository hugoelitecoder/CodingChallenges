using System;
using System.Linq;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        var H1 = Console.ReadLine();
        var H2 = Console.ReadLine();
        var V1 = Console.ReadLine();
        var V2 = Console.ReadLine();

        var output = Solve(H1, H2, V1, V2);
        foreach (var line in output)
            Console.WriteLine(line);
    }

    static string[] Solve(string H1, string H2, string V1, string V2)
    {
        var p1 = Matches(H1, V1);
        var p2 = Matches(H1, V2);
        var p3 = Matches(H2, V1);
        var p4 = Matches(H2, V2);

        var uniqueGrids = new HashSet<string>();

        foreach (var (i1, j1) in p1)
        foreach (var (i2, j2) in p2)
        {
            int dc = i2 - i1;
            if (Math.Abs(dc) < 2) continue;

            foreach (var (i3, j3) in p3)
            {
                int dr = j3 - j1;
                if (Math.Abs(dr) < 2) continue;

                int i4 = i3 + dc;
                int j4 = j2 + dr;
                if (i4 < 0 || i4 >= H2.Length || j4 < 0 || j4 >= V2.Length)
                    continue;

                if (!p4.Any(t => t.Item1 == i4 && t.Item2 == j4))
                    continue;

                var grid = BuildGrid(H1, H2, V1, V2, (i1, j1, i2, j2, i3, j3));
                var key = string.Join("\n", grid);
                uniqueGrids.Add(key);
            }
        }

        if (uniqueGrids.Count == 1)
        {
            return uniqueGrids.First().Split('\n');
        }

        return new[] { uniqueGrids.Count.ToString() };
    }

    static List<(int, int)> Matches(string h, string v)
    {
        var list = new List<(int, int)>();
        for (int i = 0; i < h.Length; i++)
            for (int j = 0; j < v.Length; j++)
                if (h[i] == v[j])
                    list.Add((i, j));
        return list;
    }

    static string[] BuildGrid(
        string H1, string H2,
        string V1, string V2,
        (int a1, int b1, int a2, int b2, int a3, int b3) sol)
    {
        var (a1, b1, a2, b2, a3, b3) = sol;
        int dc = a2 - a1;
        int r1 = b1;
        int r2 = b3;
        int c1 = -a1;
        int c2 = -a3;
        int cV2 = c1 + a2;
        int rV2 = r1 - b2;

        var coords = new List<(int r, int c, char ch)>();
        AddLine(coords, H1, r1, c1, horizontal: true);
        AddLine(coords, H2, r2, c2, horizontal: true);
        AddLine(coords, V1, 0, 0, horizontal: false);
        AddLine(coords, V2, rV2, cV2, horizontal: false);

        int minR = coords.Min(pt => pt.r);
        int maxR = coords.Max(pt => pt.r);
        int minC = coords.Min(pt => pt.c);
        int maxC = coords.Max(pt => pt.c);
        int rows = maxR - minR + 1;
        int cols = maxC - minC + 1;

        var grid = Enumerable.Range(0, rows)
                             .Select(_ => Enumerable.Repeat('.', cols).ToArray())
                             .ToArray();

        foreach (var (r, c, ch) in coords)
            grid[r - minR][c - minC] = ch;

        return grid.Select(row => new string(row)).ToArray();
    }

    static void AddLine(
        List<(int r, int c, char ch)> coords,
        string w, int r0, int c0, bool horizontal)
    {
        for (int k = 0; k < w.Length; k++)
        {
            int r = horizontal ? r0 : r0 + k;
            int c = horizontal ? c0 + k : c0;
            coords.Add((r, c, w[k]));
        }
    }
}
