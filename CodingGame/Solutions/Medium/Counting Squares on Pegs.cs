using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var points = new Point2D[N];
        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split();
            points[i] = new Point2D(int.Parse(parts[0]), int.Parse(parts[1]));
        }
        var set = new HashSet<Point2D>(points);
        long count = 0;

        for (int i = 0; i < N; i++)
        {
            var p1 = points[i];
            for (int j = i + 1; j < N; j++)
            {
                var p2 = points[j];
                int dx = p2.X - p1.X;
                int dy = p2.Y - p1.Y;

                var c1 = new Point2D(p1.X - dy, p1.Y + dx);
                var c2 = new Point2D(p2.X - dy, p2.Y + dx);
                if (set.Contains(c1) && set.Contains(c2)) count++;

                var d1 = new Point2D(p1.X + dy, p1.Y - dx);
                var d2 = new Point2D(p2.X + dy, p2.Y - dx);
                if (set.Contains(d1) && set.Contains(d2)) count++;
            }
        }

        Console.WriteLine(count / 4);
    }

    struct Point2D : IEquatable<Point2D>
    {
        public int X { get; }
        public int Y { get; }

        public Point2D(int x, int y) => (X, Y) = (x, y);

        public bool Equals(Point2D other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is Point2D p && Equals(p);
        public override int GetHashCode() => HashCode.Combine(X, Y);
    }
}
