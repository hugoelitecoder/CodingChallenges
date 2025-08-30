using System;
using System.Collections.Generic;

public class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var W = int.Parse(inputs[0]);
        var H = int.Parse(inputs[1]);
        var heights = new int[H, W];
        for (var i = 0; i < H; i++)
        {
            var rowInputs = Console.ReadLine().Split(' ');
            for (var j = 0; j < W; j++)
            {
                heights[i, j] = int.Parse(rowInputs[j]);
            }
        }

        var map = new DrainageMap(W, H, heights);
        var solver = new DrainageSystem(map);
        
        var result = solver.CalculateMinimumDrains();
        Console.WriteLine(result);
    }
}

public readonly record struct Point2D(int R, int C);

public class DrainageMap
{
    public int W { get; }
    public int H { get; }
    private readonly int[,] _heights;

    public DrainageMap(int w, int h, int[,] heights)
    {
        W = w;
        H = h;
        _heights = heights;
    }

    public int GetHeight(Point2D p)
    {
        return _heights[p.R, p.C];
    }

    public bool IsValid(Point2D p)
    {
        return p.R >= 0 && p.R < H && p.C >= 0 && p.C < W;
    }
}

public class DrainageSystem
{
    private readonly DrainageMap _map;
    private readonly Point2D[,] _sinks;
    private readonly Point2D _unassigned = new Point2D(-1, -1);

    private static readonly int[] dR = { -1, 1, 0, 0 };
    private static readonly int[] dC = { 0, 0, -1, 1 };

    public DrainageSystem(DrainageMap map)
    {
        _map = map;
        _sinks = new Point2D[map.H, map.W];
        for (var i = 0; i < map.H; i++)
        {
            for (var j = 0; j < map.W; j++)
            {
                _sinks[i, j] = _unassigned;
            }
        }
    }

    public int CalculateMinimumDrains()
    {
        var uniqueSinks = new HashSet<Point2D>();
        for (var r = 0; r < _map.H; r++)
        {
            for (var c = 0; c < _map.W; c++)
            {
                var p = new Point2D(r, c);
                if (_sinks[p.R, p.C] == _unassigned)
                {
                    var sink = FindSink(p);
                    uniqueSinks.Add(sink);
                    MarkCatchmentArea(sink);
                }
            }
        }
        return uniqueSinks.Count;
    }

    private Point2D FindSink(Point2D start)
    {
        var visited = new HashSet<Point2D>();
        var current = start;
        while (true)
        {
            visited.Add(current);
            var currentHeight = _map.GetHeight(current);
            var nextMove = _unassigned;

            for (var i = 0; i < 4; i++)
            {
                var neighbor = new Point2D(current.R + dR[i], current.C + dC[i]);
                if (_map.IsValid(neighbor) &&
                    _map.GetHeight(neighbor) <= currentHeight &&
                    !visited.Contains(neighbor))
                {
                    nextMove = neighbor;
                    break;
                }
            }

            if (nextMove != _unassigned)
            {
                current = nextMove;
            }
            else
            {
                return current;
            }
        }
    }

    private void MarkCatchmentArea(Point2D sink)
    {
        var queue = new Queue<Point2D>();
        
        queue.Enqueue(sink);
        _sinks[sink.R, sink.C] = sink;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentHeight = _map.GetHeight(current);

            for (var i = 0; i < 4; i++)
            {
                var neighbor = new Point2D(current.R + dR[i], current.C + dC[i]);
                if (_map.IsValid(neighbor) && _sinks[neighbor.R, neighbor.C] == _unassigned)
                {
                    if (_map.GetHeight(neighbor) >= currentHeight)
                    {
                        _sinks[neighbor.R, neighbor.C] = sink;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
    }
}