using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    public static void Main()
    {
        var threads = Enumerable
            .Range(0, int.Parse(Console.ReadLine()))
            .Select(_ => Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Select(arr => new HashSet<string>(arr))
            .ToList();

        Console.WriteLine(Consolidate(threads).Count);
    }

    private static List<HashSet<string>> Consolidate(List<HashSet<string>> sets)
    {
        for (var i = 0; i < sets.Count; i++)
        {
            var a = sets[i];
            if (a.Count == 0) continue;
            for (var j = i + 1; j < sets.Count; j++)
            {
                var b = sets[j];
                if (a.Overlaps(b))
                {
                    b.UnionWith(a);
                    a.Clear();
                }
            }
        }
        return sets.Where(s => s.Count > 0).ToList();
    }
}
