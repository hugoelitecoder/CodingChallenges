using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Solution
{
    static void Main()
    {
        string line = Console.ReadLine().Trim();
        var cycles = ParseCycles(line);

        var domain = new SortedSet<int>(cycles.SelectMany(c => c));
        var mapping = domain.ToDictionary(x => x, x => x);

        for (int i = cycles.Count - 1; i >= 0; i--)
        {
            var cycle = cycles[i];
            var cycleMap = new Dictionary<int, int>();
            int k = cycle.Count;
            for (int j = 0; j < k; j++)
                cycleMap[cycle[j]] = cycle[(j + 1) % k];

            foreach (var x in domain.ToList())
            {
                int img = mapping[x];
                mapping[x] = cycleMap.ContainsKey(img) ? cycleMap[img] : img;
            }
        }

        var visited = new HashSet<int>();
        var resultCycles = new List<List<int>>();

        foreach (var x in domain)
        {
            if (visited.Contains(x) || mapping[x] == x)
                continue;

            var cycle = new List<int>();
            int curr = x;
            while (!visited.Contains(curr))
            {
                visited.Add(curr);
                cycle.Add(curr);
                curr = mapping[curr];
            }
            resultCycles.Add(cycle);
        }

        if (resultCycles.Count == 0)
        {
            Console.WriteLine("()");
        }
        else
        {
            var sb = new StringBuilder();
            foreach (var cycle in resultCycles)
            {
                sb.Append('(');
                sb.Append(string.Join(" ", cycle));
                sb.Append(')');
            }
            Console.WriteLine(sb.ToString());
        }
    }

    static List<List<int>> ParseCycles(string s)
    {
        var cycles = new List<List<int>>();
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] != '(') continue;
            int j = s.IndexOf(')', i);
            if (j < 0) break;
            string content = s.Substring(i + 1, j - i - 1).Trim();
            if (!string.IsNullOrEmpty(content))
            {
                var parts = content.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var cycle = parts.Select(int.Parse).ToList();
                if (cycle.Count > 0)
                    cycles.Add(cycle);
            }
            i = j;
        }
        return cycles;
    }
}
