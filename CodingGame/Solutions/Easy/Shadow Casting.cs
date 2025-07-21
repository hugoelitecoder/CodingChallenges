using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    public static void Main()
    {
        var n = int.Parse(Console.ReadLine());
        var input = new List<string>(n);
        for (var i = 0; i < n; i++)
            input.Add(Console.ReadLine());

        var w = input.Max(s => s.Length) + 2;
        var pad = new string(' ', w);
        var grid = new List<string>(input.Count + 2);
        foreach (var s in input)
            grid.Add(s.PadRight(w));
        grid.Add(pad);
        grid.Add(pad);

        for (var r = 0; r < grid.Count; r++)
        {
            var line = grid[r];
            var sb = new System.Text.StringBuilder(line.Length);
            for (var c = 0; c < line.Length; c++)
            {
                var ch = line[c];
                if (ch == ' ' && r > 0 && c > 0 && grid[r - 1][c - 1] != ' ')
                    ch = '-';
                if (ch == ' ' && r > 1 && c > 1 && grid[r - 2][c - 2] != ' ')
                    ch = '`';
                sb.Append(ch);
            }
            Console.WriteLine(sb.ToString().TrimEnd());
        }
    }
}
