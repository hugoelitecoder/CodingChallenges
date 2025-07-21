using System;
using System.Collections.Generic;
using System.Threading;

class Solution
{
    public static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var n = int.Parse(inputs[0]);
        var m = int.Parse(inputs[1]);

        var adj = new List<Edge>[n + 1];
        for (var i = 1; i <= n; i++)
        {
            adj[i] = new List<Edge>();
        }

        for (var i = 0; i < m; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var u = int.Parse(inputs[0]);
            var v = int.Parse(inputs[1]);
            var cost = int.Parse(inputs[2]);
            var edge = new Edge(u, v, cost);
            adj[u].Add(edge);
            adj[v].Add(edge);
        }

        var mstResult = FindMST(n, adj, 1);

        mstResult.Edges.Sort((e1, e2) =>
        {
            var cmpU = e1.U.CompareTo(e2.U);
            if (cmpU != 0)
            {
                return cmpU;
            }
            return e1.V.CompareTo(e2.V);
        });

        Console.WriteLine($"{mstResult.Edges.Count} {mstResult.TotalCost}");
        foreach (var edge in mstResult.Edges)
        {
            Console.WriteLine($"{edge.U} {edge.V} {edge.Cost}");
        }
    }

    private static MinimumSpanningTree FindMST(int n, List<Edge>[] adj, int startNode)
    {
        if (n == 0)
        {
            return new MinimumSpanningTree(new List<Edge>(), 0);
        }
        var totalCost = 0L;
        var mstEdges = new List<Edge>();
        var visited = new bool[n + 1];
        var pq = new SortedSet<Edge>();
        
        visited[startNode] = true;
        foreach (var edge in adj[startNode])
        {
            pq.Add(edge);
        }
        
        var edgesCount = 0;
        while (pq.Count > 0 && edgesCount < n - 1)
        {
            var edge = pq.Min;
            pq.Remove(edge);
            var u = edge.U;
            var v = edge.V;
            var isUVisited = visited[u];
            var isVVisited = visited[v];
            if (isUVisited && isVVisited)
            {
                continue;
            }
            mstEdges.Add(edge);
            totalCost += edge.Cost;
            edgesCount++;
            var newNode = isUVisited ? v : u;
            visited[newNode] = true;
            foreach (var nextEdge in adj[newNode])
            {
                if (!visited[nextEdge.U] || !visited[nextEdge.V])
                {
                    pq.Add(nextEdge);
                }
            }
        }
        return new MinimumSpanningTree(mstEdges, totalCost);
    }
}

class Edge : IComparable<Edge>
{
    private static int _nextId = 0;
    public int Id { get; }
    public int U { get; }
    public int V { get; }
    public int Cost { get; }

    public Edge(int u, int v, int cost)
    {
        U = u;
        V = v;
        Cost = cost;
        Id = Interlocked.Increment(ref _nextId);
    }

    public int CompareTo(Edge other)
    {
        var costComparison = this.Cost.CompareTo(other.Cost);
        if (costComparison != 0)
        {
            return costComparison;
        }
        return this.Id.CompareTo(other.Id);
    }
}

class MinimumSpanningTree
{
    public List<Edge> Edges { get; }
    public long TotalCost { get; }

    public MinimumSpanningTree(List<Edge> edges, long totalCost)
    {
        Edges = edges;
        TotalCost = totalCost;
    }
}
