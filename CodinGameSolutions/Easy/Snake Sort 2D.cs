using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        var apples = new List<(string name, int row, int col)>(N);
        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            string name = parts[0];
            int r = int.Parse(parts[1]);
            int c = int.Parse(parts[2]);
            apples.Add((name, r, c));
        }
        var rows = apples
            .GroupBy(a => a.row)
            .OrderBy(g => g.Key)
            .Select(g => new {
                row = g.Key,
                apples = g.ToList()
            })
            .ToList();

        var result = new List<string>();
        for (int i = 0; i < rows.Count; i++)
        {
            var group = rows[i].apples;
            var ordered = (i % 2 == 0)
                ? group.OrderBy(a => a.col)
                : group.OrderByDescending(a => a.col);

            foreach (var a in ordered)
                result.Add(a.name);
        }

        Console.WriteLine(string.Join(",", result));
    }
}
