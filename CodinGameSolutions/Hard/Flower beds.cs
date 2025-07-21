using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        var poly = new List<(int x, int y)>();
        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split();
            poly.Add((int.Parse(parts[0]), int.Parse(parts[1])));
        }
        long flowers = CountInteriorLatticePoints(poly);
        Console.WriteLine(flowers);
    }

    static long CountInteriorLatticePoints(List<(int x, int y)> poly)
    {
        long twiceArea = 0;
        int n = poly.Count;
        long boundary = 0;
        for (int i = 0; i < n; i++)
        {
            var (x1, y1) = poly[i];
            var (x2, y2) = poly[(i + 1) % n];
            twiceArea += (long)x1 * y2 - (long)x2 * y1;
            boundary += Gcd(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
        }
        long area = Math.Abs(twiceArea) / 2;
        long interior = area - boundary / 2 + 1;
        return interior;
    }

    static long Gcd(long a, long b)
    {
        while (b != 0)
        {
            long t = b;
            b = a % b;
            a = t;
        }
        return a;
    }
}
