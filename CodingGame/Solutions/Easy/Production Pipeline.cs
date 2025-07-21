using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        int K = int.Parse(Console.ReadLine());

        var adj = new List<int>[N + 1];
        var indegree = new int[N + 1];
        for (int i = 1; i <= N; i++)
            adj[i] = new List<int>();

        for (int i = 0; i < K; i++)
        {
            string line = Console.ReadLine().Trim();
            var parts = line.Split('<');
            int u = int.Parse(parts[0]);
            int v = int.Parse(parts[1]);
            adj[u].Add(v);
            indegree[v]++;
        }

        var available = new SortedSet<int>();
        for (int i = 1; i <= N; i++)
            if (indegree[i] == 0)
                available.Add(i);

        var order = new List<int>();
        while (available.Count > 0)
        {
            int u = available.Min;
            available.Remove(u);
            order.Add(u);

            foreach (int w in adj[u])
            {
                indegree[w]--;
                if (indegree[w] == 0)
                    available.Add(w);
            }
        }

        if (order.Count < N)
        {
            Console.WriteLine("INVALID");
        }
        else
        {
            Console.WriteLine(string.Join(" ", order));
        }
    }
}
