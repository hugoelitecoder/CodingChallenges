using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    struct Point : IComparable<Point>
    {
        public int X, Y;
        public Point(int x, int y) { X = x; Y = y; }
        public int CompareTo(Point other)
            => X != other.X ? X.CompareTo(other.X) : Y.CompareTo(other.Y);
        public override bool Equals(object obj)
            => obj is Point p && p.X == X && p.Y == Y;
        public override int GetHashCode()
            => X * 31 + Y;
        public static bool operator ==(Point a, Point b) => a.Equals(b);
        public static bool operator !=(Point a, Point b) => !a.Equals(b);
    }

    struct Edge
    {
        public Point A, B;
        public Edge(Point p1, Point p2)
        {
            if (p1.CompareTo(p2) <= 0) { A = p1; B = p2; }
            else                        { A = p2; B = p1; }
        }
        public override bool Equals(object obj)
            => obj is Edge e && A == e.A && B == e.B;
        public override int GetHashCode()
            => A.GetHashCode() * 397 ^ B.GetHashCode();
    }

    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        var grid = new char[N][];
        for (int i = 0; i < N; i++)
            grid[i] = Console.ReadLine().ToCharArray();

        var edges = new HashSet<Edge>();
        for (int i = 0; i < N; i++)
            for (int j = 0; j < N; j++)
                if (grid[i][j] == '#')
                {
                    if (i == 0 || grid[i-1][j] == '.')
                        edges.Add(new Edge(new Point(j, i),     new Point(j+1, i)));
                    if (i == N-1 || grid[i+1][j] == '.')
                        edges.Add(new Edge(new Point(j, i+1),   new Point(j+1, i+1)));
                    if (j == 0 || grid[i][j-1] == '.')
                        edges.Add(new Edge(new Point(j, i),     new Point(j, i+1)));
                    if (j == N-1 || grid[i][j+1] == '.')
                        edges.Add(new Edge(new Point(j+1, i),   new Point(j+1, i+1)));
                }

        var adj = new Dictionary<Point, List<Point>>();
        foreach (var e in edges)
        {
            if (!adj.ContainsKey(e.A)) adj[e.A] = new List<Point>();
            if (!adj.ContainsKey(e.B)) adj[e.B] = new List<Point>();
            adj[e.A].Add(e.B);
            adj[e.B].Add(e.A);
        }

        var start = adj.Keys.Min();
        var curr  = adj[start].Min();
        var prev  = start;
        var path  = new List<Point> { prev, curr };

        while (true)
        {
            var nbrs = adj[curr];
            var next = nbrs[0].Equals(prev) ? nbrs[1] : nbrs[0];
            if (next.Equals(start))
                break;
            path.Add(next);
            prev = curr;
            curr = next;
        }
        path.Add(start);
        path.RemoveAt(path.Count - 1);
        int sides = 0;
        int L = path.Count;
        for (int i = 0; i < L; i++)
        {
            var p0 = path[(i - 1 + L) % L];
            var p1 = path[i];
            var p2 = path[(i + 1) % L];
            int dx1 = p1.X - p0.X, dy1 = p1.Y - p0.Y;
            int dx2 = p2.X - p1.X, dy2 = p2.Y - p1.Y;
            if (dx1 != dx2 || dy1 != dy2)
                sides++;
        }

        Console.WriteLine(sides);
    }
}
