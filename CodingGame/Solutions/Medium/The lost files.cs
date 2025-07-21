using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int E = int.Parse(Console.ReadLine());
        var adj = new Dictionary<int, List<int>>();
        var verts = new HashSet<int>();

        for (int i = 0; i < E; i++)
        {
            var parts = Console.ReadLine().Split();
            int u = int.Parse(parts[0]), v = int.Parse(parts[1]);
            if (!adj.ContainsKey(u)) adj[u] = new List<int>();
            if (!adj.ContainsKey(v)) adj[v] = new List<int>();
            adj[u].Add(v);
            adj[v].Add(u);
            verts.Add(u);
            verts.Add(v);
        }

        var seen = new HashSet<int>();
        int continents = 0;
        foreach (var start in verts)
        {
            if (seen.Contains(start)) continue;
            continents++;
            var stack = new Stack<int>();
            stack.Push(start);
            seen.Add(start);
            while (stack.Count > 0)
            {
                int x = stack.Pop();
                foreach (var y in adj[x])
                    if (!seen.Contains(y))
                    {
                        seen.Add(y);
                        stack.Push(y);
                    }
            }
        }

        int V = verts.Count;
        int tiles = E - V + continents;
        Console.WriteLine($"{continents} {tiles}");
    }
}
