using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        int virus = int.Parse(Console.ReadLine());
        int m = int.Parse(Console.ReadLine());

        var g = new List<int>[n];
        for (int i = 0; i < n; i++) g[i] = new List<int>();
        for (int i = 0; i < m; i++)
        {
            var parts = Console.ReadLine().Split();
            int u = int.Parse(parts[0]), v = int.Parse(parts[1]);
            g[u].Add(v);
            g[v].Add(u);
        }
        int bestNode = -1, bestProtected = -1;
        var visited = new bool[n];
        var q = new Queue<int>();
        for (int f = 0; f < n; f++)
        {
            if (f == virus) continue;
            Array.Clear(visited, 0, n);
            visited[f] = true;
            visited[virus] = true;
            q.Clear();
            q.Enqueue(virus);
            int reached = 0;
            while (q.Count > 0)
            {
                int u = q.Dequeue();
                reached++;
                foreach (var w in g[u])
                    if (!visited[w])
                    {
                        visited[w] = true;
                        q.Enqueue(w);
                    }
            }
            int prot = n - reached; 
            if (prot > bestProtected)
            {
                bestProtected = prot;
                bestNode = f;
            }
        }
        Console.WriteLine(bestNode);
    }
}
