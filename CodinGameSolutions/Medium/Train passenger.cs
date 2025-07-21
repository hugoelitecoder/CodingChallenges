using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var src  = Console.ReadLine().Trim();
        var dst  = Console.ReadLine().Trim();
        int cnt  = int.Parse(Console.ReadLine());

        var adj = Enumerable.Range(0, cnt)
            .Select(_ => Console.ReadLine().Split())
            .SelectMany(p => new[] { (u: p[0], v: p[1]), (u: p[1], v: p[0]) })
            .GroupBy(e => e.u)
            .ToDictionary(g => g.Key, g => g.Select(e => e.v).ToList());

        if (src == dst)
        {
            Console.WriteLine(src);
            return;
        }

        var seen   = new HashSet<string> { src };
        var parent = new Dictionary<string, string>();
        var path = BFS(adj, dst, new List<string> { src }, seen, parent)
                   ?? new List<string> { src };

        Console.WriteLine(string.Join(" > ", path));
    }

    static List<string> BFS(
        Dictionary<string, List<string>> adj,
        string dst,
        List<string> frontier,
        HashSet<string> seen,
        Dictionary<string, string> parent
    )
    {
        if (frontier.Count == 0) return null;

        var nextLayer = new List<string>();
        foreach (var u in frontier)
        {
            if (!adj.TryGetValue(u, out var nbrs)) continue;
            foreach (var v in nbrs)
            {
                if (seen.Contains(v)) continue;
                seen.Add(v);
                parent[v] = u;
                if (v == dst)
                {
                    var result = new List<string>();
                    for (var cur = dst; cur != null; parent.TryGetValue(cur, out cur))
                        result.Add(cur);
                    result.Reverse();
                    return result;
                }
                nextLayer.Add(v);
            }
        }

        return BFS(adj, dst, nextLayer, seen, parent);
    }
}
