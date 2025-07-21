using System;

class Solution
{
    struct Point { public long x, y; }

    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var poly = new Point[N];
        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split();
            poly[i].x = long.Parse(parts[0]);
            poly[i].y = long.Parse(parts[1]);
        }

        int M = int.Parse(Console.ReadLine());
        for (int i = 0; i < M; i++)
        {
            var parts = Console.ReadLine().Split();
            var px = long.Parse(parts[0]);
            var py = long.Parse(parts[1]);
            Console.WriteLine(IsInside(poly, N, px, py) ? "hit" : "miss");
        }
    }

    static bool IsInside(Point[] poly, int N, long px, long py)
    {
        for (int i = 0; i < N; i++)
        {
            var a = poly[i];
            var b = poly[(i + 1) % N];
            // Cross product (b - a) x (p - a)
            var cross = (b.x - a.x) * (py - a.y) - (b.y - a.y) * (px - a.x);
            if (cross < 0) return false;
        }
        return true;
    }
}
