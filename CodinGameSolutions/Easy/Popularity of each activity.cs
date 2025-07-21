using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var height = int.Parse(Console.ReadLine());
        var rows = new List<string>();
        for (var i = 0; i < height; i++)
            rows.Add(Console.ReadLine());

        var delimiterRows = rows
            .Select((r, idx) => (r, idx))
            .Where(x => x.r.Contains('+'))
            .Select(x => x.idx)
            .ToArray();
        var hCuts = delimiterRows;
        var vCuts = rows[hCuts[0]]
            .Select((c, idx) => (c, idx))
            .Where(x => x.c == '+')
            .Select(x => x.idx)
            .ToArray();

        var rowRanges = new (int start, int end)[]
        {
            (0, hCuts[0] - 1),
            (hCuts[0] + 1, hCuts[1] - 1),
            (hCuts[1] + 1, height - 1)
        };
        var width = rows[0].Length;
        var colRanges = new (int start, int end)[]
        {
            (0, vCuts[0] - 1),
            (vCuts[0] + 1, vCuts[1] - 1),
            (vCuts[1] + 1, width - 1)
        };

        var counts = new int[3, 3];
        var total = 0;
        for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
            {
                var (r0, r1) = rowRanges[i];
                var (c0, c1) = colRanges[j];
                var cnt = 0;
                for (var r = r0; r <= r1; r++)
                    for (var c = c0; c <= c1; c++)
                        if (rows[r][c] == '*')
                            cnt++;
                counts[i, j] = cnt;
                total += cnt;
            }

        Console.WriteLine($"{total} attendees");
        for (var i = 0; i < 3; i++)
        {
            var line = Enumerable.Range(0, 3)
                .Select(j =>
                {
                    var pct = (int)(counts[i, j] * 100.0 / total + 0.5);
                    return (pct.ToString() + "%").PadLeft(4, '_');
                })
                .ToArray();
            Console.WriteLine(string.Join(" ", line));
        }
    }
}
