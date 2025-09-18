using System;
using System.Collections.Generic;
using System.Globalization;

class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var islands = new List<Point3D>();
        var culture = CultureInfo.InvariantCulture;
        for (var i = 0; i < n; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var x = double.Parse(inputs[0], culture);
            var y = double.Parse(inputs[1], culture);
            var z = double.Parse(inputs[2], culture);
            islands.Add(new Point3D(x, y, z));
        }
        var solver = new PandoraSolver(islands);
        var (totalLength, numTrees) = solver.Solve();
        var truncatedLength = Math.Truncate(totalLength * 100) / 100;
        Console.WriteLine(truncatedLength.ToString("F2", culture));
        Console.WriteLine(numTrees);
    }
}

internal class PandoraSolver
{
    private readonly List<Point3D> _points;
    private const double TreeLength = 1000.0;

    public PandoraSolver(List<Point3D> islands)
    {
        _points = new List<Point3D>(islands);
        _points.Add(new Point3D(1.0, 1.0, 1.0));
    }

    public (double totalLength, int numTrees) Solve()
    {
        var edges = GenerateValidEdges();
        var (totalLength, mstBridgeLengths) = FindMST(edges);
        var numTrees = CalculateTrees(mstBridgeLengths);
        return (totalLength, numTrees);
    }
    
    private List<Edge> GenerateValidEdges()
    {
        var edges = new List<Edge>();
        var numPoints = _points.Count;
        for (var i = 0; i < numPoints; i++)
        {
            for (var j = i + 1; j < numPoints; j++)
            {
                var p1 = _points[i];
                var p2 = _points[j];
                var dx = p1.X - p2.X;
                var dy = p1.Y - p2.Y;
                var dz = p1.Z - p2.Z;
                var hDistSq = dx * dx + dy * dy;
                var vDistSq = dz * dz;
                if (vDistSq >= hDistSq) continue;
                var dist = Math.Sqrt(hDistSq + vDistSq);
                if (dist > TreeLength) continue;
                edges.Add(new Edge(i, j, dist));
            }
        }
        return edges;
    }

    private (double totalLength, List<double> bridgeLengths) FindMST(List<Edge> edges)
    {
        edges.Sort();
        var numPoints = _points.Count;
        var dsu = new DisjointSetUnion(numPoints);
        var totalLength = 0.0;
        var bridgeLengths = new List<double>();
        var edgesCount = 0;
        foreach (var edge in edges)
        {
            if (dsu.Find(edge.U) != dsu.Find(edge.V))
            {
                dsu.Union(edge.U, edge.V);
                totalLength += edge.Weight;
                bridgeLengths.Add(edge.Weight);
                edgesCount++;
                if (edgesCount == numPoints - 1) break;
            }
        }
        return (totalLength, bridgeLengths);
    }
    
    private int CalculateTrees(List<double> bridgeLengths)
    {
        bridgeLengths.Sort();
        bridgeLengths.Reverse();
        var leftovers = new List<double>();
        var trees = 0;
        foreach (var length in bridgeLengths)
        {
            var found = false;
            for (var i = 0; i < leftovers.Count; i++)
            {
                if (leftovers[i] >= length)
                {
                    leftovers[i] -= length;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                trees++;
                var newLeftover = TreeLength - length;
                if (newLeftover > 1e-9) 
                {
                    leftovers.Add(newLeftover);
                }
            }
        }
        return trees;
    }
}

internal readonly struct Point3D
{
    public readonly double X;
    public readonly double Y;
    public readonly double Z;
    public Point3D(double x, double y, double z) { X = x; Y = y; Z = z; }
}

internal class Edge : IComparable<Edge>
{
    public readonly int U;
    public readonly int V;
    public readonly double Weight;
    public Edge(int u, int v, double weight) { U = u; V = v; Weight = weight; }
    public int CompareTo(Edge other) { return Weight.CompareTo(other.Weight); }
}

internal class DisjointSetUnion
{
    private readonly int[] _parent;
    private readonly int[] _rank;

    public DisjointSetUnion(int n)
    {
        _parent = new int[n];
        _rank = new int[n];
        for (var i = 0; i < n; i++)
        {
            _parent[i] = i;
        }
    }

    public int Find(int i)
    {
        if (_parent[i] == i) return i;
        return _parent[i] = Find(_parent[i]);
    }

    public void Union(int i, int j)
    {
        var rootI = Find(i);
        var rootJ = Find(j);
        if (rootI != rootJ)
        {
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
        }
    }
}