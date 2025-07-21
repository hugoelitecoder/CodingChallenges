using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        Console.ReadLine();
        var p = Console.ReadLine()
                       .Split()
                       .Select(int.Parse)
                       .Where(x => x > 0)
                       .OrderBy(x => x)
                       .ToList();
        var seen = new Dictionary<string, int>();
        int turn = 0;
        while (true)
        {
            var key = string.Join(",", p);
            if (seen.TryGetValue(key, out var first))
            {
                Console.WriteLine(turn - first);
                return;
            }
            seen[key] = turn++;
            int count = p.Count;
            p = p.Select(x => x - 1)
                 .Where(x => x > 0)
                 .Append(count)
                 .OrderBy(x => x)
                 .ToList();
        }
    }
}
