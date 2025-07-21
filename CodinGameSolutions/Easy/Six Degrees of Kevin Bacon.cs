using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    const string TARGET = "Kevin Bacon";
    static void Main()
    {
        string start = Console.ReadLine().Trim();
        int n = int.Parse(Console.ReadLine());

        var movieToCast = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        for (int i = 0; i < n; i++)
        {
            string line = Console.ReadLine();
            int colon = line.IndexOf(':');
            string movie = line.Substring(0, colon).Trim();
            var cast = line.Substring(colon + 1)
                           .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(a => a.Trim())
                           .ToList();
            movieToCast[movie] = cast;
        }

        var adj = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var cast in movieToCast.Values)
        {
            foreach (var a in cast)
            {
                if (!adj.ContainsKey(a)) adj[a] = new List<string>();
            }
            for (int i = 0; i < cast.Count; i++)
            {
                for (int j = i + 1; j < cast.Count; j++)
                {
                    adj[cast[i]].Add(cast[j]);
                    adj[cast[j]].Add(cast[i]);
                }
            }
        }

        if (start == TARGET)
        {
            Console.WriteLine(0);
            return;
        }

        var q = new Queue<(string actor, int dist)>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        seen.Add(start);
        q.Enqueue((start, 0));
        while (q.Count > 0)
        {
            var (actor, dist) = q.Dequeue();
            if (!adj.ContainsKey(actor)) continue; 

            foreach (var nei in adj[actor])
            {
                if (seen.Contains(nei)) continue;
                if (nei == TARGET)
                {
                    Console.WriteLine(dist + 1);
                    return;
                }
                seen.Add(nei);
                q.Enqueue((nei, dist + 1));
            }
        }
        Console.WriteLine(-1);
    }
}
