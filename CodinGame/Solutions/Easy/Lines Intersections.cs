using System;
using System.Collections.Generic;
using System.Diagnostics;
class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var nInput = Console.ReadLine();
        if (nInput == null) return;
        var n = int.Parse(nInput);
        var engine = new LineIntersectionEngine();
        for (var i = 0; i < n; i++)
        {
            var lineInput = Console.ReadLine();
            if (lineInput == null) continue;
            var p = lineInput.Split(' ');
            engine.AddLine(double.Parse(p[0]), double.Parse(p[1]), double.Parse(p[2]), double.Parse(p[3]));
        }
        var results = engine.CalculateUniqueIntersections();
        Console.WriteLine(results.Count);
        foreach (var pt in results)
        {
            Console.WriteLine($"{Format(pt.X)} {Format(pt.Y)}");
        }
        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] Lines processed: {n}");
        Console.Error.WriteLine($"[DEBUG] Unique points: {results.Count}");
        Console.Error.WriteLine($"[DEBUG] Execution time: {sw.ElapsedMilliseconds}ms");
    }
    private static string Format(double val)
    {
        var sanitized = Math.Abs(val) < 1e-7 ? 0.0 : val;
        return sanitized.ToString("F3");
    }
}
public readonly struct Point : IComparable<Point>
{
    public readonly double X;
    public readonly double Y;
    private const double Eps = 1e-7;
    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }
    public int CompareTo(Point other)
    {
        if (Math.Abs(X - other.X) > Eps) return X.CompareTo(other.X);
        if (Math.Abs(Y - other.Y) > Eps) return Y.CompareTo(other.Y);
        return 0;
    }
    public bool Equals(Point other)
    {
        return Math.Abs(X - other.X) < Eps && Math.Abs(Y - other.Y) < Eps;
    }
}
public readonly struct Line
{
    public readonly double A;
    public readonly double B;
    public readonly double C;
    public Line(double x1, double y1, double x2, double y2)
    {
        A = y1 - y2;
        B = x2 - x1;
        C = A * x1 + B * y1;
    }
}
public class LineIntersectionEngine
{
    private readonly List<Line> _lines = new List<Line>();
    private const double DetEps = 1e-10;
    public void AddLine(double x1, double y1, double x2, double y2)
    {
        _lines.Add(new Line(x1, y1, x2, y2));
    }
    public List<Point> CalculateUniqueIntersections()
    {
        var points = new List<Point>();
        for (var i = 0; i < _lines.Count; i++)
        {
            for (var j = i + 1; j < _lines.Count; j++)
            {
                if (TryIntersect(_lines[i], _lines[j], out var p))
                {
                    if (!IsDuplicate(points, p))
                    {
                        points.Add(p);
                    }
                }
            }
        }
        points.Sort();
        return points;
    }
    private bool TryIntersect(Line l1, Line l2, out Point p)
    {
        p = default;
        var det = l1.A * l2.B - l2.A * l1.B;
        if (Math.Abs(det) < DetEps) return false;
        var x = (l1.C * l2.B - l2.C * l1.B) / det;
        var y = (l1.A * l2.C - l2.A * l1.C) / det;
        p = new Point(x, y);
        return true;
    }
    private bool IsDuplicate(List<Point> list, Point p)
    {
        foreach (var existing in list)
        {
            if (existing.Equals(p)) return true;
        }
        return false;
    }
}