using System;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        var n = int.Parse(Console.ReadLine());
        var m = int.Parse(Console.ReadLine());
        var points1 = new List<Point>();
        var points2 = new List<Point>();
        for (var i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split();
            var x = int.Parse(parts[0]);
            var y = int.Parse(parts[1]);
            points1.Add(new Point(x, y));
        }
        for (var i = 0; i < m; i++)
        {
            var parts = Console.ReadLine().Split();
            var x = int.Parse(parts[0]);
            var y = int.Parse(parts[1]);
            points2.Add(new Point(x, y));
        }
        var poly1 = Polygon.FromPoints(points1);
        var poly2 = Polygon.FromPoints(points2);
        var intersection = Polygon.Intersect(poly1, poly2);
        var area = intersection.AreaCeil();
        Console.WriteLine(area);
    }
}

class Point
{
    public double X { get; }
    public double Y { get; }
    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }
}

class Polygon
{
    private List<Point> _points;
    public IReadOnlyList<Point> Points => _points;
    public Polygon(IEnumerable<Point> pts)
    {
        _points = new List<Point>(pts);
    }
    public static Polygon FromPoints(List<Point> pts)
    {
        var hull = ConvexHull(pts);
        return new Polygon(hull);
    }
    public static Polygon Intersect(Polygon a, Polygon b)
    {
        var result = new List<Point>();
        foreach (var pt in a._points)
            result.Add(new Point(pt.X, pt.Y));
        for (var i = 0; i < b._points.Count; i++)
        {
            var A = b._points[i];
            var B = b._points[(i + 1) % b._points.Count];
            var input = result;
            result = new List<Point>();
            if (input.Count == 0) break;
            var prev = input[input.Count - 1];
            foreach (var curr in input)
            {
                if (IsInside(A, B, curr))
                {
                    if (!IsInside(A, B, prev))
                        result.Add(Intersection(prev, curr, A, B));
                    result.Add(curr);
                }
                else if (IsInside(A, B, prev))
                {
                    result.Add(Intersection(prev, curr, A, B));
                }
                prev = curr;
            }
        }
        return new Polygon(result);
    }
    public int AreaCeil()
    {
        var area = Math.Abs(SignedArea());
        return (int)Math.Ceiling(area);
    }
    private double SignedArea()
    {
        var area = 0.0;
        var n = _points.Count;
        for (var i = 0; i < n; i++)
        {
            var a = _points[i];
            var b = _points[(i + 1) % n];
            area += a.X * b.Y - a.Y * b.X;
        }
        return 0.5 * area;
    }
    private static List<Point> ConvexHull(List<Point> pts)
    {
        pts.Sort((a, b) => a.X != b.X ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y));
        var hull = new List<Point>();
        foreach (var p in pts)
        {
            while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }
        var t = hull.Count + 1;
        for (var i = pts.Count - 2; i >= 0; i--)
        {
            var p = pts[i];
            while (hull.Count >= t && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }
        hull.RemoveAt(hull.Count - 1);
        return hull;
    }
    private static double Cross(Point a, Point b, Point c)
    {
        var x1 = b.X - a.X;
        var y1 = b.Y - a.Y;
        var x2 = c.X - a.X;
        var y2 = c.Y - a.Y;
        return x1 * y2 - x2 * y1;
    }
    private static bool IsInside(Point a, Point b, Point p)
    {
        var cp = (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);
        return cp >= -1e-9;
    }
    private static Point Intersection(Point p1, Point p2, Point a, Point b)
    {
        var A1 = p2.Y - p1.Y;
        var B1 = p1.X - p2.X;
        var C1 = A1 * p1.X + B1 * p1.Y;
        var A2 = b.Y - a.Y;
        var B2 = a.X - b.X;
        var C2 = A2 * a.X + B2 * a.Y;
        var det = A1 * B2 - A2 * B1;
        if (Math.Abs(det) < 1e-9)
            return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        var x = (B2 * C1 - B1 * C2) / det;
        var y = (A1 * C2 - A2 * C1) / det;
        return new Point(x, y);
    }
}
