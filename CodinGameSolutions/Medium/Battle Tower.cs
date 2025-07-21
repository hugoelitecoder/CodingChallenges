using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var adj = new List<int>[N + 1];
        for (int i = 1; i <= N; i++) adj[i] = new List<int>();
        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split();
            int id = int.Parse(parts[0]);
            int xCount = int.Parse(parts[1]);
            for (int j = 0; j < xCount; j++)
                adj[id].Add(int.Parse(parts[2 + j]));
        }
        if (N == 1)
        {
            Console.WriteLine(1);
            return;
        }
        var calc = new VertexCoverCalculator(adj);
        Console.WriteLine(calc.Compute());
    }
}

class VertexCoverCalculator
{
    private readonly List<int>[] _adj;
    private readonly int[,] _dp;

    public VertexCoverCalculator(List<int>[] adj)
    {
        _adj = adj;
        int n = adj.Length - 1;
        _dp = new int[n + 1, 2];
    }

    public int Compute()
    {
        DFS(1, 0);
        return Math.Min(_dp[1, 0], _dp[1, 1]);
    }

    private void DFS(int u, int parent)
    {
        _dp[u, 1] = 1;
        _dp[u, 0] = 0;
        foreach (var v in _adj[u])
        {
            if (v == parent) continue;
            DFS(v, u);
            _dp[u, 1] += Math.Min(_dp[v, 0], _dp[v, 1]);
            _dp[u, 0] += _dp[v, 1];
        }
    }
}