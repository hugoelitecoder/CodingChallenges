using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var points = new List<Point2D>(n);
        for (var i = 0; i < n; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var x = int.Parse(inputs[0]);
            var y = int.Parse(inputs[1]);
            points.Add(new Point2D(x, y));
        }

        var analyzer = new CrimeSceneAnalyzer(points);
        var rollsNeeded = analyzer.CalculateRequiredRolls();
        
        Console.WriteLine(rollsNeeded);
    }
}

class CrimeSceneAnalyzer
{
    private const double TapeRadius = 3.0;
    private const double RollLength = 5.0;

    private readonly Polygon _clueHull;

    public CrimeSceneAnalyzer(List<Point2D> clueLocations)
    {
        _clueHull = ConvexHullBuilder.Build(clueLocations);
    }

    public long CalculateRequiredRolls()
    {
        var perimeter = _clueHull.CalculatePerimeter();
        var totalTapeLength = perimeter + 2 * Math.PI * TapeRadius;
        var rollsNeeded = (long)Math.Ceiling(totalTapeLength / RollLength);
        return rollsNeeded;
    }
}

static class ConvexHullBuilder
{
    public static Polygon Build(List<Point2D> points)
    {
        var uniquePoints = points.Distinct().ToList();
        uniquePoints.Sort();
        if (uniquePoints.Count <= 2)
        {
            return new Polygon(uniquePoints);
        }

        var lower = new List<Point2D>();
        foreach (var p in uniquePoints)
        {
            while (lower.Count >= 2 && CrossProduct(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
            {
                lower.RemoveAt(lower.Count - 1);
            }
            lower.Add(p);
        }

        var upper = new List<Point2D>();
        for (var i = uniquePoints.Count - 1; i >= 0; i--)
        {
            var p = uniquePoints[i];
            while (upper.Count >= 2 && CrossProduct(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
            {
                upper.RemoveAt(upper.Count - 1);
            }
            upper.Add(p);
        }
        
        var hullVertices = new List<Point2D>();
        hullVertices.AddRange(lower.Take(lower.Count - 1));
        hullVertices.AddRange(upper.Take(upper.Count - 1));
        
        return new Polygon(hullVertices);
    }

    private static long CrossProduct(Point2D a, Point2D b, Point2D c)
    {
        return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
    }
}

class Polygon
{
    private readonly List<Point2D> _vertices;

    public Polygon(List<Point2D> vertices)
    {
        _vertices = vertices ?? new List<Point2D>();
    }

    public double CalculatePerimeter()
    {
        var perimeter = 0.0;
        if (_vertices.Count <= 1)
        {
            return 0.0;
        }

        for (var i = 0; i < _vertices.Count; i++)
        {
            var p1 = _vertices[i];
            var p2 = _vertices[(i + 1) % _vertices.Count];
            perimeter += Distance(p1, p2);
        }
        return perimeter;
    }

    private static double Distance(Point2D a, Point2D b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt((double)dx * dx + (double)dy * dy);
    }
}

public struct Point2D : IComparable<Point2D>
{
    public long X { get; }
    public long Y { get; }

    public Point2D(long x, long y)
    {
        X = x;
        Y = y;
    }

    public int CompareTo(Point2D other)
    {
        var xComparison = X.CompareTo(other.X);
        if (xComparison != 0)
        {
            return xComparison;
        }
        return Y.CompareTo(other.Y);
    }
    
    public override bool Equals(object obj) => obj is Point2D other && Equals(other);

    public bool Equals(Point2D other) => X == other.X && Y == other.Y;

    public override int GetHashCode() => HashCode.Combine(X, Y);
}