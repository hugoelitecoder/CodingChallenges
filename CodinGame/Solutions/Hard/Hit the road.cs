using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var np = Console.ReadLine().Split(' ');
        var n = int.Parse(np[0]);
        var m = int.Parse(np[1]);
        var ntw = int.Parse(np[2]);
        var st = Console.ReadLine().Split(' ');
        var s = int.Parse(st[0]);
        var t = int.Parse(st[1]);
        var junctions = new Junction[n];
        for (var i = 0; i < n; i++)
            junctions[i] = new Junction(i);
        for (var i = 0; i < ntw; i++)
        {
            var tw = Console.ReadLine().Split(' ');
            var v = int.Parse(tw[0]);
            var b = int.Parse(tw[1]);
            var e = int.Parse(tw[2]);
            junctions[v].SetTimeWindow(b, e);
        }
        var roads = new List<Road>();
        for (var i = 0; i < m; i++)
        {
            var edge = Console.ReadLine().Split(' ');
            var from = int.Parse(edge[0]);
            var to = int.Parse(edge[1]);
            var duration = int.Parse(edge[2]);
            roads.Add(new Road(from, to, duration));
        }
        var network = new RoadNetwork(junctions, roads);
        var possible = network.CanReach(s, t);
        Console.WriteLine(possible ? "true" : "false");
    }
}

class Junction
{
    public int Id { get; }
    public int WindowStart { get; private set; }
    public int WindowEnd { get; private set; }

    public Junction(int id)
    {
        Id = id;
        WindowStart = 0;
        WindowEnd = 50;
    }

    public void SetTimeWindow(int b, int e)
    {
        WindowStart = b;
        WindowEnd = e;
    }

    public bool IsOpenAt(int time)
    {
        return time >= WindowStart && time <= WindowEnd;
    }
}

class Road
{
    public int From { get; }
    public int To { get; }
    public int Duration { get; }

    public Road(int from, int to, int duration)
    {
        From = from;
        To = to;
        Duration = duration;
    }
}

class RoadNetwork
{
    private readonly Junction[] _junctions;
    private readonly List<int>[] _edges;
    private readonly int[,] _durations;

    public RoadNetwork(Junction[] junctions, List<Road> roads)
    {
        _junctions = junctions;
        var n = junctions.Length;
        _edges = new List<int>[n];
        _durations = new int[n, n];
        for (var i = 0; i < n; i++) _edges[i] = new List<int>();
        foreach (var road in roads)
        {
            _edges[road.From].Add(road.To);
            _durations[road.From, road.To] = road.Duration;
        }
    }

    public bool CanReach(int start, int end)
    {
        var maxTime = 99;
        var n = _junctions.Length;
        var visited = new bool[n, maxTime + 1];
        var queue = new Queue<(int node, int time)>();
        queue.Enqueue((start, 0));
        while (queue.Count > 0)
        {
            var (node, time) = queue.Dequeue();
            if (!_junctions[node].IsOpenAt(time)) continue;
            if (visited[node, time]) continue;
            visited[node, time] = true;
            if (node == end) return true;
            foreach (var next in _edges[node])
            {
                var ntime = time + _durations[node, next];
                if (ntime > maxTime) continue;
                queue.Enqueue((next, ntime));
            }
        }
        return false;
    }
}
