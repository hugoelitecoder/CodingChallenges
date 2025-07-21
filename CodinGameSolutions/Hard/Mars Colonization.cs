using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var M = int.Parse(inputs[0]);
        var S = int.Parse(inputs[1]);
        
        var stations = new List<Station>(M);
        for (var i = 0; i < M; i++)
        {
            var stationInputs = Console.ReadLine().Split(' ');
            var x = int.Parse(stationInputs[0]);
            var y = int.Parse(stationInputs[1]);
            stations.Add(new Station(x, y));
        }

        var edges = new List<Edge>();
        for (var i = 0; i < M; i++)
        {
            for (var j = i + 1; j < M; j++)
            {
                var station1 = stations[i];
                var station2 = stations[j];
                var dx = (long)station1.X - station2.X;
                var dy = (long)station1.Y - station2.Y;
                var distSq = dx * dx + dy * dy;
                edges.Add(new Edge(i, j, distSq));
            }
        }

        edges.Sort((e1, e2) => e1.DistSq.CompareTo(e2.DistSq));
        
        var dsu = new DisjointSetUnion(M);
        var requiredDistSq = 0L;

        foreach (var edge in edges)
        {
            if (dsu.Count <= S)
            {
                break;
            }
            if (dsu.Union(edge.U, edge.V))
            {
                requiredDistSq = edge.DistSq;
            }
        }
        
        var minDistance = Math.Sqrt(requiredDistSq);
        Console.WriteLine($"{minDistance:F2}");
    }
}

public record Station(int X, int Y);

public record Edge(int U, int V, long DistSq);

public class DisjointSetUnion
{
    private readonly int[] _parent;
    private readonly int[] _rank;
    public int Count { get; private set; }

    public DisjointSetUnion(int n)
    {
        Count = n;
        _parent = new int[n];
        _rank = new int[n];
        for (var i = 0; i < n; i++)
        {
            _parent[i] = i;
            _rank[i] = 0;
        }
    }

    public int Find(int i)
    {
        if (_parent[i] == i)
        {
            return i;
        }
        return _parent[i] = Find(_parent[i]);
    }

    public bool Union(int i, int j)
    {
        var rootI = Find(i);
        var rootJ = Find(j);
        if (rootI == rootJ)
        {
            return false;
        }
        if (_rank[rootI] < _rank[rootJ])
        {
            _parent[rootI] = rootJ;
        }
        else if (_rank[rootI] > _rank[rootJ])
        {
            _parent[rootJ] = rootI;
        }
        else
        {
            _parent[rootJ] = rootI;
            _rank[rootI]++;
        }
        Count--;
        return true;
    }
}
