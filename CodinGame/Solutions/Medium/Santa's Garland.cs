using System;
using System.Collections.Generic;
using System.Diagnostics;
class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var line1 = Console.ReadLine();
        if (string.IsNullOrEmpty(line1)) return;
        var p1 = line1.Split(' ');
        var n = int.Parse(p1[0]);
        var m = int.Parse(p1[1]);
        var line2 = Console.ReadLine();
        if (string.IsNullOrEmpty(line2)) return;
        var p2 = line2.Split(' ');
        var startNode = int.Parse(p2[0]);
        var endNode = int.Parse(p2[1]);
        var adj = new List<Edge>[n];
        for (var i = 0; i < n; i++) adj[i] = new List<Edge>();
        for (var i = 0; i < m; i++)
        {
            var lineEdge = Console.ReadLine();
            if (string.IsNullOrEmpty(lineEdge)) continue;
            var pE = lineEdge.Split(' ');
            var u = int.Parse(pE[0]);
            var v = int.Parse(pE[1]);
            var t = int.Parse(pE[2]);
            adj[u].Add(new Edge(v, t));
            adj[v].Add(new Edge(u, t));
        }
        Console.Error.WriteLine($"[DEBUG] Nodes: {n}, Wires: {m}");
        Console.Error.WriteLine($"[DEBUG] Start: {startNode}, End: {endNode}");
        if (startNode == endNode)
        {
            Console.WriteLine(0);
            return;
        }
        var engine = new GarlandPathEngine(n, adj);
        var result = engine.FindShortestValidPath(startNode, endNode);
        if (result == -1)
        {
            Console.WriteLine("IMPOSSIBLE");
        }
        else
        {
            Console.WriteLine(result);
        }
        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] Calculation time: {sw.ElapsedMilliseconds}ms");
    }
}
public readonly struct Edge
{
    public readonly int To;
    public readonly int Fuse;
    public Edge(int to, int fuse)
    {
        To = to;
        Fuse = fuse;
    }
}
public class GarlandPathEngine
{
    private readonly int _n;
    private readonly List<Edge>[] _adj;
    public GarlandPathEngine(int n, List<Edge>[] adj)
    {
        _n = n;
        _adj = adj;
    }
    public int FindShortestValidPath(int start, int end)
    {
        var q = new Queue<(int pos, int dist)>();
        var visited = new bool[_n];
        q.Enqueue((end, 0));
        visited[end] = true;
        while (q.Count > 0)
        {
            var curr = q.Dequeue();
            foreach (var edge in _adj[curr.pos])
            {
                if (edge.Fuse <= curr.dist) continue;
                var nextDist = curr.dist + 1;
                if (edge.To == start) return nextDist;
                if (!visited[edge.To])
                {
                    visited[edge.To] = true;
                    q.Enqueue((edge.To, nextDist));
                }
            }
        }
        return -1;
    }
}