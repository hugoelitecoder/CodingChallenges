using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

class Solution
{
    static void Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        var stopwatch = Stopwatch.StartNew();
        var n = ReadInt();
        var halfPlanes = new List<HalfPlane>();
        var possible = true;

        for (var i = 0; i < n; i++)
        {
            var s = Console.ReadLine();
            var hp = new HalfPlane(s);
            
            const double epsilon = 1e-9;
            if (Math.Abs(hp.A) < epsilon && Math.Abs(hp.B) < epsilon)
            {
                if (hp.C < -epsilon)
                {
                    possible = false;
                    break;
                }
            }
            else
            {
                halfPlanes.Add(hp);
            }
        }

        if (!possible)
        {
            Console.WriteLine("No Overlap");
        }
        else
        {
            var result = Solve(halfPlanes);
            Console.WriteLine(result);
        }
        
        stopwatch.Stop();
        Console.Error.WriteLine($"[DEBUG] Input: n={n}");
        Console.Error.WriteLine($"[DEBUG] Calculation time: {stopwatch.Elapsed.TotalMilliseconds:F3} ms");
    }

    private static int ReadInt() => int.Parse(Console.ReadLine());

    public static string Solve(List<HalfPlane> halfPlanes)
    {
        const double M = 2.0e7; 
        const double Epsilon = 1e-9;

        var polygon = ConvexPolygon.CreateBoundingBox(M);

        foreach (var hp in halfPlanes)
        {
            polygon = polygon.Clip(hp, Epsilon);
            if (polygon.VertexCount == 0)
            {
                return "No Overlap";
            }
        }

        if (polygon.VertexCount < 3)
        {
            return "No Overlap";
        }

        var area = polygon.CalculateArea();
        if (area < Epsilon)
        {
            return "No Overlap";
        }

        if (polygon.IsUnbounded(M, Epsilon))
        {
            return "Overlap, But Infinite";
        }
        
        return area.ToString("F3", CultureInfo.InvariantCulture);
    }
}

public readonly struct Point2d
{
    public readonly double X;
    public readonly double Y;
    public Point2d(double x, double y) { X = x; Y = y; }
}

public class HalfPlane
{
    public double A { get; private set; }
    public double B { get; private set; }
    public double C { get; private set; }

    public HalfPlane(string inequality)
    {
        var s = inequality.Replace(" ", "");
        var op = "";
        var opIndex = -1;

        if (s.Contains("<=")) { op = "<="; opIndex = s.IndexOf("<="); }
        else if (s.Contains(">=")) { op = ">="; opIndex = s.IndexOf(">="); }
        else { throw new ArgumentException("Invalid inequality operator."); }

        var lhs = s.Substring(0, opIndex);
        var rhs = s.Substring(opIndex + 2);
        
        C = double.Parse(rhs, CultureInfo.InvariantCulture);
        lhs = lhs.Replace("-", "+-");
        if (lhs.StartsWith("+-")) { lhs = lhs.Substring(1); }
        var terms = lhs.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
        
        A = 0; B = 0;
        
        foreach (var term in terms)
        {
            if (term.EndsWith("x"))
            {
                var coeffStr = term.Substring(0, term.Length - 1);
                if (coeffStr == "" || coeffStr == "+") A += 1;
                else if (coeffStr == "-") A -= 1;
                else A += double.Parse(coeffStr, CultureInfo.InvariantCulture);
            }
            else if (term.EndsWith("y"))
            {
                var coeffStr = term.Substring(0, term.Length - 1);
                if (coeffStr == "" || coeffStr == "+") B += 1;
                else if (coeffStr == "-") B -= 1;
                else B += double.Parse(coeffStr, CultureInfo.InvariantCulture);
            }
        }

        if (op == ">=") { A = -A; B = -B; C = -C; }
    }

    public bool IsInside(Point2d p, double epsilon)
    {
        return A * p.X + B * p.Y - C < epsilon;
    }
    
    public Point2d GetIntersection(Point2d p1, Point2d p2)
    {
        var val1 = A * p1.X + B * p1.Y - C;
        var val2 = A * p2.X + B * p2.Y - C;
        var t = val1 / (val1 - val2);
        var interX = p1.X + t * (p2.X - p1.X);
        var interY = p1.Y + t * (p2.Y - p1.Y);
        return new Point2d(interX, interY);
    }
}

public class ConvexPolygon
{
    public IReadOnlyList<Point2d> Vertices { get; }
    public int VertexCount => Vertices.Count;

    public ConvexPolygon(IEnumerable<Point2d> vertices)
    {
        Vertices = new List<Point2d>(vertices);
    }

    public static ConvexPolygon CreateBoundingBox(double size)
    {
        return new ConvexPolygon(new List<Point2d>
        {
            new Point2d(-size, -size), new Point2d(size, -size),
            new Point2d(size, size),   new Point2d(-size, size)
        });
    }

    public ConvexPolygon Clip(HalfPlane clipper, double epsilon)
    {
        var outputVertices = new List<Point2d>();
        if (VertexCount == 0) return new ConvexPolygon(outputVertices);

        var s = Vertices[VertexCount - 1];
        for (var i = 0; i < VertexCount; i++)
        {
            var e = Vertices[i];
            var sInside = clipper.IsInside(s, epsilon);
            var eInside = clipper.IsInside(e, epsilon);

            if (sInside && eInside) outputVertices.Add(e);
            else if (sInside && !eInside) outputVertices.Add(clipper.GetIntersection(s, e));
            else if (!sInside && eInside)
            {
                outputVertices.Add(clipper.GetIntersection(s, e));
                outputVertices.Add(e);
            }
            s = e;
        }
        return new ConvexPolygon(outputVertices);
    }

    public double CalculateArea()
    {
        if (VertexCount < 3) return 0.0;
        var area = 0.0;
        var j = VertexCount - 1;
        for (var i = 0; i < VertexCount; i++)
        {
            area += (Vertices[j].X + Vertices[i].X) * (Vertices[j].Y - Vertices[i].Y);
            j = i;
        }
        return Math.Abs(area / 2.0);
    }

    public bool IsUnbounded(double bound, double epsilon)
    {
        foreach (var vertex in Vertices)
        {
            if (Math.Abs(Math.Abs(vertex.X) - bound) < epsilon || Math.Abs(Math.Abs(vertex.Y) - bound) < epsilon)
            {
                return true;
            }
        }
        return false;
    }
}