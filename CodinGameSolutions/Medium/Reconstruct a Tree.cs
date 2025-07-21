using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var code = Console.ReadLine()
                          .Split(' ')
                          .Select(int.Parse)
                          .ToList();
        int R = int.Parse(Console.ReadLine());
        
        int m = code.Count;
        int n = m + 2;
        
        var degree = new int[n+1];
        for (int i = 1; i <= n; i++) degree[i] = 1;
        foreach (var v in code) degree[v]++;
        
        var edges = new List<(int u,int v)>(n-1);
        var leafs = new SortedSet<int>();
        for (int i = 1; i <= n; i++)
            if (degree[i] == 1) leafs.Add(i);
        
        foreach (var v in code)
        {
            int u = leafs.Min;
            leafs.Remove(u);
            edges.Add((u,v));
            degree[u]--;
            degree[v]--;
            if (degree[v] == 1)
                leafs.Add(v);
        }
        var rem = leafs.ToList();
        edges.Add((rem[0], rem[1]));
        
        var adj = new List<int>[n+1];
        for (int i = 1; i <= n; i++) adj[i] = new List<int>();
        foreach (var (u,v) in edges)
        {
            adj[u].Add(v);
            adj[v].Add(u);
        }
        
        var children = new List<int>[n+1];
        for (int i = 1; i <= n; i++) children[i] = new List<int>();
        var visited = new bool[n+1];
        
        void Dfs(int u)
        {
            visited[u] = true;
            foreach (var v in adj[u].OrderBy(x => x))
            {
                if (!visited[v])
                {
                    children[u].Add(v);
                    Dfs(v);
                }
            }
        }
        
        Dfs(R);
        
        string Print(int u)
        {
            var parts = new List<string>();
            parts.Add(u.ToString());
            foreach (var c in children[u])
                parts.Add(Print(c));
            return "(" + string.Join(" ", parts) + ")";
        }
        
        Console.WriteLine(Print(R));
    }
}
