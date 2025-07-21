using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var nImp = int.Parse(Console.ReadLine());
        var imports = new List<string>(nImp);
        for (var i = 0; i < nImp; i++)
        {
            var line = Console.ReadLine().Trim();
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            imports.Add(parts.Length == 2 && parts[0] == "import" ? parts[1] : line);
        }

        var nDep = int.Parse(Console.ReadLine());
        var depsMap = new Dictionary<string, List<string>>();
        for (var i = 0; i < nDep; i++)
        {
            var split = Console.ReadLine().Trim().Split(new[] { " requires " }, StringSplitOptions.None);
            var lib = split[0].Trim();
            var deps = split[1].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            if (!depsMap.ContainsKey(lib)) depsMap[lib] = new List<string>();
            depsMap[lib].AddRange(deps);
        }

        var loaded = new HashSet<string>();
        foreach (var lib in imports)
        {
            if (depsMap.TryGetValue(lib, out var reqs))
            {
                foreach (var req in reqs)
                {
                    if (!loaded.Contains(req))
                    {
                        Console.WriteLine($"Import error: tried to import {lib} but {req} is required.");
                        AttemptReorder(imports, depsMap);
                        return;
                    }
                }
            }
            loaded.Add(lib);
        }

        Console.WriteLine("Compiled successfully!");
    }

    static void AttemptReorder(List<string> imports, Dictionary<string, List<string>> depsMap)
    {
        var graph = new Dictionary<string, List<string>>();
        var indegree = new Dictionary<string, int>();
        foreach (var lib in imports)
        {
            graph[lib] = new List<string>();
            indegree[lib] = 0;
        }

        foreach (var lib in imports)
        {
            if (depsMap.TryGetValue(lib, out var reqs))
            {
                foreach (var req in reqs)
                {
                    if (!indegree.ContainsKey(req))
                    {
                        Console.WriteLine("Fatal error: interdependencies.");
                        return;
                    }
                    graph[req].Add(lib);
                    indegree[lib]++;
                }
            }
        }

        var ready = new SortedSet<string>(indegree.Where(p => p.Value == 0).Select(p => p.Key));
        var result = new List<string>();
        while (ready.Count > 0)
        {
            var next = ready.Min;
            ready.Remove(next);
            result.Add(next);
            foreach (var nbr in graph[next])
            {
                if (--indegree[nbr] == 0) ready.Add(nbr);
            }
        }

        if (result.Count != imports.Count)
        {
            Console.WriteLine("Fatal error: interdependencies.");
        }
        else
        {
            Console.WriteLine("Suggest to change import order:");
            foreach (var lib in result)
                Console.WriteLine($"import {lib}");
        }
    }
}

