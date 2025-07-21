using System;
using System.Globalization;
using System.Linq;

class Solution
{
    static void Main()
    {
        var reference = ReadPoint();
        int n = int.Parse(Console.ReadLine());
        var points = Enumerable.Range(0, n)
            .Select(_ => ReadPoint())
            .ToArray();

        int count = (from i in Enumerable.Range(0, n - 2)
                     from j in Enumerable.Range(i + 1, n - 1 - i)
                     from k in Enumerable.Range(j + 1, n - 1 - j)
                     let A = points[i]
                     let B = points[j]
                     let C = points[k]
                     let area = TriangleArea(A, B, C)
                     where area > 1e-9
                     let sum = TriangleArea(reference, B, C)
                               + TriangleArea(A, reference, C)
                               + TriangleArea(A, B, reference)
                     where Math.Abs(sum - area) < 1e-9
                     select 0).Count();

        Console.WriteLine(count);
    }

    static (double X, double Y) ReadPoint()
    {
        var parts = Console.ReadLine().Split(' ');
        var coords = parts[1].Split(';');
        return (
            double.Parse(coords[0], CultureInfo.InvariantCulture),
            double.Parse(coords[1], CultureInfo.InvariantCulture)
        );
    }

    static double TriangleArea((double X, double Y) p1, (double X, double Y) p2, (double X, double Y) p3) =>
        Math.Abs((p2.X - p1.X) * (p3.Y - p1.Y) - (p3.X - p1.X) * (p2.Y - p1.Y)) * 0.5;
}
