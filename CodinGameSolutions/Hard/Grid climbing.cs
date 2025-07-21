using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var costs = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
        var grid = new int[n, n];
        for (var i = 0; i < n; i++)
        {
            var row = Console.ReadLine();
            for (var j = 0; j < n; j++)
            {
                grid[i, j] = row[j] - '0';
            }
        }
        var solver = new GridSolver(n, costs, grid);
        var minCost = solver.FindMinCost();
        Console.WriteLine(minCost);
    }
}

public class Point
{
    public int Row { get; }
    public int Col { get; }
    public Point(int row, int col)
    {
        Row = row;
        Col = col;
    }
    public override bool Equals(object obj)
    {
        return obj is Point other && Row == other.Row && Col == other.Col;
    }
    public override int GetHashCode()
    {
        return (Row << 16) ^ Col;
    }
}

public class QueueEntry : IComparable<QueueEntry>
{
    public int Cost { get; }
    public Point Pos { get; }
    public QueueEntry(int cost, Point pos)
    {
        Cost = cost;
        Pos = pos;
    }
    public int CompareTo(QueueEntry other)
    {
        if (Cost != other.Cost) return Cost.CompareTo(other.Cost);
        if (Pos.Row != other.Pos.Row) return Pos.Row.CompareTo(other.Pos.Row);
        return Pos.Col.CompareTo(other.Pos.Col);
    }
}

public class GridSolver
{
    private readonly int _n;
    private readonly int[] _costs;
    private readonly int[,] _grid;
    private readonly int[,] _dist;
    public GridSolver(int n, int[] costs, int[,] grid)
    {
        _n = n;
        _costs = costs;
        _grid = grid;
        _dist = new int[n, n];
        for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++)
                _dist[i, j] = int.MaxValue;
    }

    public int FindMinCost()
    {
        _dist[0, 0] = _grid[0, 0];
        var pq = new SortedSet<QueueEntry>();
        pq.Add(new QueueEntry(_grid[0, 0], new Point(0, 0)));

        while (pq.Count > 0)
        {
            var entry = ExtractMin(pq);
            var currCost = entry.Cost;
            var i = entry.Pos.Row;
            var j = entry.Pos.Col;

            if (currCost > _dist[i, j])
                continue;

            foreach (var neighbor in GetAllNeighbors(i, j))
            {
                var d = MaxDistance(i, j, neighbor.Row, neighbor.Col);
                var stepCost = _costs[d - 1];
                var newCost = currCost + stepCost + _grid[neighbor.Row, neighbor.Col];
                if (newCost < _dist[neighbor.Row, neighbor.Col])
                {
                    _dist[neighbor.Row, neighbor.Col] = newCost;
                    pq.Add(new QueueEntry(newCost, neighbor));
                }
            }
        }
        return _dist[_n - 1, _n - 1];
    }

    private IEnumerable<Point> GetAllNeighbors(int i, int j)
    {
        for (var ni = 0; ni < _n; ni++)
        {
            for (var nj = 0; nj < _n; nj++)
            {
                if (ni == i && nj == j) continue;
                yield return new Point(ni, nj);
            }
        }
    }

    private int MaxDistance(int i1, int j1, int i2, int j2)
    {
        return Math.Max(Math.Abs(i1 - i2), Math.Abs(j1 - j2));
    }

    private QueueEntry ExtractMin(SortedSet<QueueEntry> pq)
    {
        var min = pq.Min;
        pq.Remove(min);
        return min;
    }
}
