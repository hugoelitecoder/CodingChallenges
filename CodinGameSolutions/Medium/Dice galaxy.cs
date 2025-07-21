using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int W = int.Parse(Console.ReadLine());
        int H = int.Parse(Console.ReadLine());
        var grid = Enumerable.Range(0, H)
            .Select(_ => Console.ReadLine().ToCharArray())
            .ToArray();

        var output = grid.Select(r => (char[])r.Clone()).ToArray();
        var seen = new bool[H, W];

        for (int r = 0; r < H; r++)
            for (int c = 0; c < W; c++)
                if (grid[r][c] != '.' && !seen[r, c])
                    BalanceNet(grid, output, seen, H, W, new Point2D(r, c));

        foreach (var row in output)
            Console.WriteLine(new string(row));
    }

    static readonly (int dr, int dc)[] NetDirs =
    {
        (0, 1), (0, -1), (1, 0), (-1, 0)
    };

    struct Point2D
    {
        public int R, C;
        public Point2D(int r, int c) { R = r; C = c; }
        public override bool Equals(object obj)
            => obj is Point2D p && p.R == R && p.C == C;
        public override int GetHashCode()
            => R * 31 + C;
    }

    struct Vector3D
    {
        public int X, Y, Z;
        public Vector3D(int x, int y, int z) { X = x; Y = y; Z = z; }
        public static Vector3D operator -(Vector3D v)
            => new Vector3D(-v.X, -v.Y, -v.Z);
    }

    class FaceFrame
    {
        public Vector3D U, V, N;
        public FaceFrame(Vector3D n, Vector3D u, Vector3D v)
        {
            N = n; U = u; V = v;
        }
    }

    static Vector3D Cross(Vector3D a, Vector3D b)
        => new Vector3D(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );

    static void BalanceNet(
        char[][] grid,
        char[][] output,
        bool[,] seen,
        int H, int W,
        Point2D start)
    {
        var comp = FloodFill(grid, seen, H, W, start);
        var one = comp.First(p => grid[p.R][p.C] == '1');
        var frames = FoldNet(grid, comp, one);
        var opp = frames.First(kv => kv.Value.N.Z == -1).Key;

        foreach (var p in comp)
        {
            int r = p.R, c = p.C;
            output[r][c] = grid[r][c] == '1'
                ? '1'
                : (r == opp.R && c == opp.C ? '6' : '#');
        }
    }

    static List<Point2D> FloodFill(
        char[][] grid,
        bool[,] seen,
        int H, int W,
        Point2D start)
    {
        var comp = new List<Point2D>();
        var q = new Queue<Point2D>();
        seen[start.R, start.C] = true;
        q.Enqueue(start);

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            comp.Add(p);
            foreach (var (dr, dc) in NetDirs)
            {
                int nr = p.R + dr, nc = p.C + dc;
                if (nr < 0 || nr >= H || nc < 0 || nc >= W) continue;
                if (seen[nr, nc] || grid[nr][nc] == '.') continue;
                seen[nr, nc] = true;
                q.Enqueue(new Point2D(nr, nc));
            }
        }
        return comp;
    }

    static Dictionary<Point2D, FaceFrame> FoldNet(
        char[][] grid,
        List<Point2D> comp,
        Point2D start)
    {
        var frames = new Dictionary<Point2D, FaceFrame>();
        var q = new Queue<Point2D>();

        frames[start] = new FaceFrame(
            new Vector3D(0, 0, 1),
            new Vector3D(1, 0, 0),
            new Vector3D(0, 1, 0)
        );
        q.Enqueue(start);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            var F = frames[cur];

            foreach (var (dr, dc) in NetDirs)
            {
                var np = new Point2D(cur.R + dr, cur.C + dc);
                if (!comp.Contains(np) || frames.ContainsKey(np)) continue;

                Vector3D n2, u2, v2;
                if (dc == 1)
                {
                    n2 = F.U; v2 = F.V; u2 = Cross(v2, n2);
                }
                else if (dc == -1)
                {
                    n2 = -F.U; v2 = F.V; u2 = Cross(v2, n2);
                }
                else if (dr == 1)
                {
                    n2 = F.V; u2 = F.U; v2 = Cross(n2, u2);
                }
                else
                {
                    n2 = -F.V; u2 = F.U; v2 = Cross(n2, u2);
                }

                frames[np] = new FaceFrame(n2, u2, v2);
                q.Enqueue(np);
            }
        }
        return frames;
    }
}
