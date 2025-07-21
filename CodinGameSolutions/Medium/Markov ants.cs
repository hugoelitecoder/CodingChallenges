using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var step = int.Parse(Console.ReadLine().Trim());
        var dims = Console.ReadLine().Trim().Split().Select(int.Parse).ToArray();
        var w = dims[0];
        var h = dims[1];
        var grid = Enumerable.Range(0, h)
                             .Select(_ => Console.ReadLine().ToCharArray())
                             .ToArray();

        var cells = grid.SelectMany(
                        (row, r) => row.Select((ch, c) => (r, c, ch)))
                        .Where(t => t.ch == '.' || t.ch == 'A')
                        .Select((t, i) => (pos: (t.r, t.c), ch: t.ch, idx: i))
                        .ToList();

        var M = cells.Count;
        var startIdx = cells.First(t => t.ch == 'A').idx;
        var idxMap = cells.ToDictionary(t => t.pos, t => t.idx);

        var A = Enumerable.Range(0, M)
                          .Select(_ => new double[M])
                          .ToArray();
        var b = Enumerable.Repeat(4.0, M).ToArray();
        var dirs = new[] {(-1,0),(1,0),(0,-1),(0,1)};

        foreach (var (pos, _, i) in cells)
        {
            A[i][i] = 4.0;
            foreach (var (dr, dc) in dirs)
            {
                var nr = pos.r + dr * step;
                var nc = pos.c + dc * step;
                if (idxMap.TryGetValue((nr, nc), out var j))
                    A[i][j] -= 1.0;
            }
        }

        GaussianElimination(A, b);
        Console.WriteLine(b[startIdx].ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
    }

    private static void GaussianElimination(double[][] A, double[] b)
    {
        var N = b.Length;
        for (var i = 0; i < N; i++)
        {
            var piv = Enumerable.Range(i, N - i)
                                .OrderByDescending(r => Math.Abs(A[r][i]))
                                .First();
            (A[i], A[piv]) = (A[piv], A[i]);
            (b[i], b[piv]) = (b[piv], b[i]);

            var diag = A[i][i];
            for (var c = i; c < N; c++) A[i][c] /= diag;
            b[i] /= diag;

            for (var r = 0; r < N; r++)
            {
                if (r == i) continue;
                var factor = A[r][i];
                for (var c = i; c < N; c++) A[r][c] -= factor * A[i][c];
                b[r] -= factor * b[i];
            }
        }
    }
}