using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var n = int.Parse(Console.ReadLine());
        var sizes = Console.ReadLine().Split(',').Select(int.Parse).ToArray();
        var startCoords = Console.ReadLine().Split(',').Select(int.Parse).ToArray();
        var destCoords = Console.ReadLine().Split(',').Select(int.Parse).ToArray();
        var b = int.Parse(Console.ReadLine());

        var obstacleRects = new List<(int[] p1, int[] p2)>();
        for (var i = 0; i < b; i++)
        {
            var corners = Console.ReadLine().Split(' ');
            var p1 = corners[0].Split(',').Select(int.Parse).ToArray();
            var p2 = corners[1].Split(',').Select(int.Parse).ToArray();
            obstacleRects.Add((p1, p2));
        }

        var pathfinder = new SpacetimePathfinder(n, sizes, obstacleRects);
        var pathLength = pathfinder.FindShortestPath(startCoords, destCoords);

        stopwatch.Stop();
        Console.Error.WriteLine($"[DEBUG] Dimensions (n): {n}");
        Console.Error.WriteLine($"[DEBUG] Maze sizes: {string.Join(",", sizes)}");
        Console.Error.WriteLine($"[DEBUG] Start: {string.Join(",", startCoords)}");
        Console.Error.WriteLine($"[DEBUG] Destination: {string.Join(",", destCoords)}");
        Console.Error.WriteLine($"[DEBUG] Obstacles (b): {b}");
        Console.Error.WriteLine($"[DEBUG] Calculation time: {stopwatch.ElapsedMilliseconds}ms");

        Console.WriteLine(pathLength == -1 ? "NO PATH" : pathLength.ToString());
    }
}

public class SpacetimePathfinder
{
    private readonly int _n;
    private readonly int[] _sizes;
    private readonly List<(int[] p1, int[] p2)> _obstacleRects;
    private readonly long[] _multipliers;

    public SpacetimePathfinder(int n, int[] sizes, List<(int[] p1, int[] p2)> obstacleRects)
    {
        _n = n;
        _sizes = sizes;
        _obstacleRects = obstacleRects;
        _multipliers = new long[n];
        long currentProduct = 1;
        for (var i = 0; i < n; i++)
        {
            _multipliers[i] = currentProduct;
            currentProduct *= sizes[i];
        }
    }

    public int FindShortestPath(int[] startCoords, int[] destCoords)
    {
        var startKey = CoordsToKey(startCoords);
        var destKey = CoordsToKey(destCoords);

        if (startKey == destKey) return 0;

        var q_s = new Queue<long>();
        var dist_s = new Dictionary<long, int>();
        var q_d = new Queue<long>();
        var dist_d = new Dictionary<long, int>();

        q_s.Enqueue(startKey);
        dist_s[startKey] = 0;
        q_d.Enqueue(destKey);
        dist_d[destKey] = 0;
        
        while (q_s.Count > 0 && q_d.Count > 0)
        {
            int dist;
            if (q_s.Count <= q_d.Count)
            {
                dist = ExpandQueue(q_s, dist_s, dist_d);
            }
            else
            {
                dist = ExpandQueue(q_d, dist_d, dist_s);
            }
            if (dist != -1) return dist;
        }
        return -1;
    }

    private int ExpandQueue(Queue<long> q, Dictionary<long, int> distOwn, Dictionary<long, int> distOther)
    {
        var count = q.Count;
        for (var k = 0; k < count; k++)
        {
            var currentKey = q.Dequeue();
            var currentCoords = KeyToCoords(currentKey);
            var currentDist = distOwn[currentKey];

            for (var i = 0; i < _n; i++)
            {
                for (var d = -1; d <= 1; d += 2)
                {
                    var neighborCoords = (int[])currentCoords.Clone();
                    neighborCoords[i] += d;

                    if (!IsInBounds(neighborCoords)) continue;
                    
                    var neighborKey = CoordsToKey(neighborCoords);

                    if (distOther.ContainsKey(neighborKey))
                    {
                        return currentDist + 1 + distOther[neighborKey];
                    }

                    if (!distOwn.ContainsKey(neighborKey))
                    {
                        if (IsObstacle(neighborCoords)) continue;
                        distOwn[neighborKey] = currentDist + 1;
                        q.Enqueue(neighborKey);
                    }
                }
            }
        }
        return -1;
    }

    private bool IsObstacle(int[] coords)
    {
        foreach (var (p1, p2) in _obstacleRects)
        {
            var isInside = true;
            for (var i = 0; i < _n; i++)
            {
                if (coords[i] < p1[i] || coords[i] > p2[i])
                {
                    isInside = false;
                    break;
                }
            }
            if (isInside) return true;
        }
        return false;
    }

    private bool IsInBounds(int[] coords)
    {
        for (var i = 0; i < _n; i++)
        {
            if (coords[i] < 0 || coords[i] >= _sizes[i])
            {
                return false;
            }
        }
        return true;
    }

    private long CoordsToKey(int[] coords)
    {
        long key = 0;
        for (var i = 0; i < _n; i++)
        {
            key += coords[i] * _multipliers[i];
        }
        return key;
    }

    private int[] KeyToCoords(long key)
    {
        var coords = new int[_n];
        var remainder = key;
        for (var i = _n - 1; i >= 0; i--)
        {
            coords[i] = (int)(remainder / _multipliers[i]);
            remainder %= _multipliers[i];
        }
        return coords;
    }
}