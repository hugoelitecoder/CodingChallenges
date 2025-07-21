using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var xs = long.Parse(inputs[0]);
        var ys = long.Parse(inputs[1]);
        inputs = Console.ReadLine().Split(' ');
        var xd = long.Parse(inputs[0]);
        var yd = long.Parse(inputs[1]);
        var n = int.Parse(Console.ReadLine());

        var clouds = new List<(long X, long Y, long W, long H)>(n);
        for (var i = 0; i < n; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var xi = long.Parse(inputs[0]);
            var yi = long.Parse(inputs[1]);
            var wi = long.Parse(inputs[2]);
            var hi = long.Parse(inputs[3]);
            clouds.Add((xi, yi, wi, hi));
        }

        var pathfinder = new PathFinder(xs, ys, xd, yd, clouds);
        var distance = pathfinder.FindShortestPath();
        Console.WriteLine(distance);
    }
}

public class PathFinder
{
    private readonly (long X, long Y) _start;
    private readonly (long X, long Y) _dest;

    private readonly long[] _xMap;
    private readonly long[] _yMap;
    private readonly Dictionary<long, int> _xToIndex;
    private readonly Dictionary<long, int> _yToIndex;
    private readonly bool[,] _traversableMap;

    private struct Event : IComparable<Event>
    {
        public readonly int XIndex;
        public readonly int YStartIndex;
        public readonly int YEndIndex;
        public readonly int Type;

        public Event(int x, int y1, int y2, int type)
        {
            XIndex = x;
            YStartIndex = y1;
            YEndIndex = y2;
            Type = type;
        }

        public int CompareTo(Event other) => XIndex.CompareTo(other.XIndex);
    }

    public PathFinder(long xs, long ys, long xd, long yd, List<(long X, long Y, long W, long H)> clouds)
    {
        _start = (xs, ys);
        _dest = (xd, yd);

        var xCoords = new HashSet<long> { _start.X, _dest.X };
        var yCoords = new HashSet<long> { _start.Y, _dest.Y };

        foreach (var cloud in clouds)
        {
            long x1 = cloud.X;
            long w = cloud.W;
            long y1 = cloud.Y;
            long h = cloud.H;
            xCoords.Add(x1 - 1); xCoords.Add(x1); xCoords.Add(x1 + w - 1); xCoords.Add(x1 + w);
            yCoords.Add(y1 - 1); yCoords.Add(y1); yCoords.Add(y1 + h - 1); yCoords.Add(y1 + h);
        }

        _xMap = xCoords.Where(c => c >= 0).Distinct().OrderBy(c => c).ToArray();
        _yMap = yCoords.Where(c => c >= 0).Distinct().OrderBy(c => c).ToArray();
        
        _xToIndex = new Dictionary<long, int>(_xMap.Length);
        for (var i = 0; i < _xMap.Length; i++) _xToIndex[_xMap[i]] = i;
        _yToIndex = new Dictionary<long, int>(_yMap.Length);
        for (var i = 0; i < _yMap.Length; i++) _yToIndex[_yMap[i]] = i;
        
        _traversableMap = new bool[_xMap.Length, _yMap.Length];
        var events = new List<Event>(clouds.Count * 2);
        
        foreach (var cloud in clouds)
        {
            if (!_xToIndex.TryGetValue(cloud.X, out var xIdx1) || !_xToIndex.TryGetValue(cloud.X + cloud.W, out var xIdx2)) continue;
            if (!_yToIndex.TryGetValue(cloud.Y, out var yIdx1) || !_yToIndex.TryGetValue(cloud.Y + cloud.H, out var yIdx2)) continue;
            
            events.Add(new Event(xIdx1, yIdx1, yIdx2, 1));
            events.Add(new Event(xIdx2, yIdx1, yIdx2, -1));
        }
        events.Sort();

        var blockageCount = new int[_yMap.Length];
        var eventIndex = 0;
        for (var i = 0; i < _xMap.Length; i++)
        {
            while (eventIndex < events.Count && events[eventIndex].XIndex == i)
            {
                var e = events[eventIndex];
                for (var j = e.YStartIndex; j < e.YEndIndex; j++)
                {
                    blockageCount[j] += e.Type;
                }
                eventIndex++;
            }
            for (var j = 0; j < _yMap.Length; j++)
            {
                _traversableMap[i, j] = (blockageCount[j] == 0);
            }
        }
    }

    public long FindShortestPath()
    {
        if (!_xToIndex.ContainsKey(_start.X) || !_yToIndex.ContainsKey(_start.Y))
        {
            return Math.Abs(_start.X - _dest.X) + Math.Abs(_start.Y - _dest.Y);
        }

        var startNode = (_xToIndex[_start.X], _yToIndex[_start.Y]);
        var destNode = (_xToIndex[_dest.X], _yToIndex[_dest.Y]);
        var dists = new Dictionary<(int, int), long>();
        var pq = new PriorityQueue<(int, int), long>();

        dists[startNode] = 0;
        pq.Enqueue(startNode, 0);

        while (pq.TryDequeue(out var currentNode, out var currentDist))
        {
            if (currentNode.Equals(destNode)) return currentDist;
            if (currentDist > dists[currentNode]) continue;

            int ix = currentNode.Item1;
            int iy = currentNode.Item2;
            
            var neighbors = new (int, int)[] { (ix + 1, iy), (ix - 1, iy), (ix, iy + 1), (ix, iy - 1) };
            foreach(var nextNode in neighbors)
            {
                int nix = nextNode.Item1;
                int niy = nextNode.Item2;

                if (nix < 0 || nix >= _xMap.Length || niy < 0 || niy >= _yMap.Length) continue;

                if (_traversableMap[nix, niy])
                {
                    long edgeWeight = Math.Abs(_xMap[ix] - _xMap[nix]) + Math.Abs(_yMap[iy] - _yMap[niy]);
                    long newDist = currentDist + edgeWeight;
                    if (!dists.ContainsKey(nextNode) || newDist < dists[nextNode])
                    {
                        dists[nextNode] = newDist;
                        pq.Enqueue(nextNode, newDist);
                    }
                }
            }
        }
        return -1;
    }
}
