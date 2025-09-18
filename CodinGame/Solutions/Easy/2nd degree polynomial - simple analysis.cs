using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

class Solution
{
    static void Main()
    {
        var parts = Console.ReadLine().Split(' ');
        double a = double.Parse(parts[0], CultureInfo.InvariantCulture);
        double b = double.Parse(parts[1], CultureInfo.InvariantCulture);
        double c = double.Parse(parts[2], CultureInfo.InvariantCulture);

        const double EPS = 1e-12;
        var pts = new List<(double x, double y)>();
        if (Math.Abs(a) < EPS && Math.Abs(b) < EPS && Math.Abs(c) < EPS)
        {
            pts.Add((0, 0));
        }
        else
        {
            if (Math.Abs(a) < EPS)
            {
                if (Math.Abs(b) > EPS)
                {
                    double xr = -c / b;
                    pts.Add((xr, 0));
                }
            }
            else
            {
                double delta = b * b - 4 * a * c;
                if (delta > EPS)
                {
                    double s = Math.Sqrt(delta);
                    double x1 = (-b - s) / (2 * a);
                    double x2 = (-b + s) / (2 * a);
                    pts.Add((x1, 0));
                    pts.Add((x2, 0));
                }
                else if (Math.Abs(delta) <= EPS)
                {
                    double x0 = -b / (2 * a);
                    pts.Add((x0, 0));
                }
            }

            bool dup = Math.Abs(c) < EPS
                       && pts.Any(p => Math.Abs(p.x) < EPS && Math.Abs(p.y) < EPS);
            if (!dup)
                pts.Add((0, c));
        }

        pts = pts
            .OrderBy(p => p.x)
            .ThenBy(p => p.y)
            .ToList();

        var uniq = new List<(double x, double y)>();
        foreach (var p in pts)
        {
            if (!uniq.Any(q =>
                Math.Abs(q.x - p.x) < EPS &&
                Math.Abs(q.y - p.y) < EPS))
                uniq.Add(p);
        }

        string F(double v)
        {
            if (Math.Abs(v) < EPS) v = 0;
            double r = Math.Round(v, 2, MidpointRounding.AwayFromZero);
            if (Math.Abs(r) < EPS) r = 0;
            return r.ToString("0.##", CultureInfo.InvariantCulture);
        }

        var outPts = uniq
            .Select(p => $"({F(p.x)},{F(p.y)})");

        Console.WriteLine(string.Join(",", outPts));
    }
}
