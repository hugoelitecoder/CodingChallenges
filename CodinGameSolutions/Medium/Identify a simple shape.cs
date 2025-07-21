using System;
using System.Linq;
using System.Collections.Generic;

struct Point2D
{
    public int x, y;
    public Point2D(int x, int y) { this.x = x; this.y = y; }
    public static Point2D operator -(Point2D a, Point2D b) 
        => new Point2D(a.x - b.x, a.y - b.y);
    public long Dot(Point2D v) 
        => (long)x * v.x + (long)y * v.y;
    public static long Cross(Point2D o, Point2D a, Point2D b) 
        => (long)(a.x - o.x) * (b.y - o.y)
         - (long)(a.y - o.y) * (b.x - o.x);
    public static long Dist2(Point2D a, Point2D b) 
        => (long)(a.x - b.x) * (a.x - b.x)
         + (long)(a.y - b.y) * (a.y - b.y);
    public override bool Equals(object obj) 
        => obj is Point2D c && x == c.x && y == c.y;
    public override int GetHashCode() 
        => HashCode.Combine(x, y);
}

class Solution
{
    static void Main()
    {
        var grid = Enumerable.Range(0, 20)
                             .Select(_ => Console.ReadLine().Split())
                             .ToArray();

        var pts = grid
            .SelectMany((row, y) => row
                .Select((c, x) => (c, x, y)))
            .Where(t => t.c == "#")
            .Select(t => new Point2D(t.x, t.y))
            .ToList();

        if (pts.Count == 0)
        {
            Console.WriteLine("EMPTY");
            return;
        }

        var hull = ConvexHull(pts);
        int n = hull.Count;
        string shape;

        if (n == 1)
            shape = "POINT";
        else if (n == 2)
            shape = "LINE";
        else
        {
            int cx = hull.Sum(p => p.x) / n;
            int cy = hull.Sum(p => p.y) / n;
            bool filled = grid[cy][cx] == "#";
            string prefix = filled ? "FILLED " : "EMPTY ";

            if (n == 3)
                shape = prefix + "TRIANGLE";
            else
            {
                bool isSquare = (hull[1] - hull[0]).Dot(hull[3] - hull[0]) == 0
                                && Point2D.Dist2(hull[0], hull[1]) == Point2D.Dist2(hull[1], hull[2]);
                shape = prefix + (isSquare ? "SQUARE" : "RECTANGLE");
            }
        }

        var output = hull
            .Distinct()
            .OrderBy(p => p.x)
            .ThenBy(p => p.y)
            .Select(p => $"({p.x},{p.y})");

        Console.WriteLine(shape + " " + string.Join(" ", output));
    }

    static List<Point2D> ConvexHull(List<Point2D> pts)
    {
        var P = pts
            .Distinct()
            .OrderBy(p => p.x)
            .ThenBy(p => p.y)
            .ToList();

        if (P.Count < 2) 
            return new List<Point2D>(P);

        var lower = new List<Point2D>();
        foreach (var p in P)
        {
            while (lower.Count >= 2 &&
                   Point2D.Cross(lower[^2], lower[^1], p) <= 0)
                lower.RemoveAt(lower.Count - 1);
            lower.Add(p);
        }

        var upper = new List<Point2D>();
        foreach (var p in ((IEnumerable<Point2D>)P).Reverse())
        {
            while (upper.Count >= 2 &&
                   Point2D.Cross(upper[^2], upper[^1], p) <= 0)
                upper.RemoveAt(upper.Count - 1);
            upper.Add(p);
        }

        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);
        lower.AddRange(upper);
        return lower;
    }
}
