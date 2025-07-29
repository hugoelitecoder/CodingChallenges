using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

class Solution
{
    static void Main()
    {
        var line = Console.ReadLine().Split(' ');
        var src = line[0];
        var dst = line[1];
        int N = int.Parse(Console.ReadLine());
        var adj = new Dictionary<string, List<(string to, double dist)>>();
        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            var u = parts[0];
            var v = parts[1];
            var d = double.Parse(parts[2], CultureInfo.InvariantCulture);
            if (!adj.ContainsKey(u)) adj[u] = new List<(string, double)>();
            if (!adj.ContainsKey(v)) adj[v] = new List<(string, double)>();
            adj[u].Add((v, d));
            adj[v].Add((u, d));
        }

        var path = new List<string>();
        var visited = new HashSet<string>();
        bool found = Dfs(src);
        if (!found) return; 

        var dists = new List<double>();
        for (int i = 0; i + 1 < path.Count; i++)
        {
            var u = path[i];
            var v = path[i + 1];
            var pair = adj[u].First(x => x.to == v);
            dists.Add(pair.dist);
        }
        double trainMin = 0;
        trainMin += 35;
        for (int i = 0; i < dists.Count; i++)
        {
            double d = dists[i];
            double side = Math.Min(3.0, d / 2.0);
            double mid = d - 2 * side;
            trainMin += (2 * side) / 50.0 * 60.0;
            trainMin += mid / 284.0 * 60.0;
            if (i < dists.Count - 1)
                trainMin += 8;
        }
        trainMin += 30;

        double carMin = 0;
        for (int i = 0; i < dists.Count; i++)
        {
            double d = dists[i];
            double side = Math.Min(7.0, d / 2.0);
            double mid = d - 2 * side;
            carMin += (2 * side) / 50.0 * 60.0;
            carMin += mid / 105.0 * 60.0;
        }

        bool trainFaster = trainMin < carMin;
        double best = trainFaster ? trainMin : carMin;
        int mins = (int)Math.Floor(best);
        int hh = mins / 60;
        int mm = mins % 60;

        var mode = trainFaster ? "TRAIN" : "CAR";
        Console.WriteLine($"{mode} {hh}:{mm:D2}");

        bool Dfs(string u)
        {
            visited.Add(u);
            path.Add(u);
            if (u == dst) return true;
            foreach (var edge in adj[u])
            {
                if (visited.Contains(edge.to)) continue;
                if (Dfs(edge.to)) return true;
            }
            path.RemoveAt(path.Count - 1);
            return false;
        }
    }
}
