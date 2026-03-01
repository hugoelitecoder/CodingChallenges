using System;
using System.Collections.Generic;
using System.Diagnostics;

class Solution
{
    public static void Main(string[] args)
    {
        var stopwatch = Stopwatch.StartNew();
        var firstLine = Console.ReadLine();
        if (firstLine == null) firstLine = "0";
        var n = int.Parse(firstLine);
        var vertices = new List<Point>(n);
        var debugInput = new List<string>(n + 1);
        debugInput.Add(firstLine);
        for (var i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            if (line == null) line = "0 0";
            debugInput.Add(line);
            var parts = line.Split(' ');
            var x = int.Parse(parts[0]);
            var y = int.Parse(parts[1]);
            vertices.Add(new Point(x, y));
        }
        var analyzer = new PolygonSupportAnalyzer(vertices);
        analyzer.Analyze(out var supportingSegments, out var staticEquilibria);
        stopwatch.Stop();
        Console.WriteLine(supportingSegments);
        Console.WriteLine(staticEquilibria);
        Console.Error.WriteLine("[DEBUG] InputLines=" + debugInput.Count);
        Console.Error.WriteLine("[DEBUG] n=" + n);
        Console.Error.WriteLine("[DEBUG] SupportingSegments=" + supportingSegments);
        Console.Error.WriteLine("[DEBUG] StaticEquilibria=" + staticEquilibria);
        Console.Error.WriteLine("[DEBUG] ElapsedMs=" + stopwatch.Elapsed.TotalMilliseconds);
    }
}

struct Point
{
    public double X;
    public double Y;
    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }
}

class PolygonSupportAnalyzer
{
    private readonly List<Point> _vertices;
    private readonly List<Point> _hull;
    private double _centerX;
    private double _centerY;
    public PolygonSupportAnalyzer(List<Point> vertices)
    {
        _vertices = vertices;
        _hull = new List<Point>();
    }
    public void Analyze(out int supportingSegments, out int staticEquilibria)
    {
        ComputeCenterOfMass();
        BuildConvexHull();
        CountSegments(out supportingSegments, out staticEquilibria);
    }
    private void ComputeCenterOfMass()
    {
        var n = _vertices.Count;
        var area2 = 0.0;
        var cxSum = 0.0;
        var cySum = 0.0;
        for (var i = 0; i < n; i++)
        {
            var j = i + 1;
            if (j == n) j = 0;
            var xi = _vertices[i].X;
            var yi = _vertices[i].Y;
            var xj = _vertices[j].X;
            var yj = _vertices[j].Y;
            var cross = xi * yj - xj * yi;
            area2 += cross;
            cxSum += (xi + xj) * cross;
            cySum += (yi + yj) * cross;
        }
        var factor = 1.0 / (3.0 * area2);
        _centerX = cxSum * factor;
        _centerY = cySum * factor;
    }
    private void BuildConvexHull()
    {
        var pts = new List<Point>(_vertices.Count);
        for (var i = 0; i < _vertices.Count; i++)
        {
            pts.Add(_vertices[i]);
        }
        pts.Sort((a, b) =>
        {
            if (a.X < b.X) return -1;
            if (a.X > b.X) return 1;
            if (a.Y < b.Y) return -1;
            if (a.Y > b.Y) return 1;
            return 0;
        });
        if (pts.Count <= 1)
        {
            _hull.Clear();
            for (var i = 0; i < pts.Count; i++)
            {
                _hull.Add(pts[i]);
            }
            return;
        }
        var lower = new List<Point>();
        for (var i = 0; i < pts.Count; i++)
        {
            var p = pts[i];
            while (lower.Count >= 2 && Cross(lower[lower.Count - 2], lower[lower.Count - 1], p) < 0.0)
            {
                lower.RemoveAt(lower.Count - 1);
            }
            lower.Add(p);
        }
        var upper = new List<Point>();
        for (var i = pts.Count - 1; i >= 0; i--)
        {
            var p = pts[i];
            while (upper.Count >= 2 && Cross(upper[upper.Count - 2], upper[upper.Count - 1], p) < 0.0)
            {
                upper.RemoveAt(upper.Count - 1);
            }
            upper.Add(p);
        }
        _hull.Clear();
        for (var i = 0; i < lower.Count; i++)
        {
            _hull.Add(lower[i]);
        }
        for (var i = 1; i < upper.Count - 1; i++)
        {
            _hull.Add(upper[i]);
        }
    }
    private void CountSegments(out int supportingSegments, out int staticEquilibria)
    {
        var m = _hull.Count;
        supportingSegments = 0;
        staticEquilibria = 0;
        if (m < 2) return;
        var processedEdges = 0;
        var startEdge = 0;
        while (processedEdges < m)
        {
            var s = startEdge;
            var nextIndex = s + 1;
            if (nextIndex == m) nextIndex = 0;
            var startPoint = _hull[s];
            var secondPoint = _hull[nextIndex];
            var baseDx = secondPoint.X - startPoint.X;
            var baseDy = secondPoint.Y - startPoint.Y;
            var endIndex = nextIndex;
            processedEdges++;
            while (processedEdges < m)
            {
                var candidateNext = endIndex + 1;
                if (candidateNext == m) candidateNext = 0;
                var fromPoint = _hull[endIndex];
                var toPoint = _hull[candidateNext];
                var dx = toPoint.X - fromPoint.X;
                var dy = toPoint.Y - fromPoint.Y;
                var cross = baseDx * dy - baseDy * dx;
                if (cross != 0.0) break;
                endIndex = candidateNext;
                processedEdges++;
            }
            supportingSegments++;
            var endPoint = _hull[endIndex];
            if (IsStaticEquilibrium(startPoint, endPoint)) staticEquilibria++;
            startEdge = endIndex;
        }
    }
    private static double Cross(Point a, Point b, Point c)
    {
        var abx = b.X - a.X;
        var aby = b.Y - a.Y;
        var acx = c.X - a.X;
        var acy = c.Y - a.Y;
        return abx * acy - aby * acx;
    }
    private bool IsStaticEquilibrium(Point a, Point b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var denom = dx * dx + dy * dy;
        if (denom == 0.0) return false;
        var vx = _centerX - a.X;
        var vy = _centerY - a.Y;
        var t = (vx * dx + vy * dy) / denom;
        var eps = 1e-9;
        if (t < -eps) return false;
        if (t > 1.0 + eps) return false;
        return true;
    }
}