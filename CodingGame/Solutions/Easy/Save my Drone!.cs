using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Solution
{
    public static void Main()
    {
        if (!int.TryParse(Console.ReadLine(), out var X)
         || !int.TryParse(Console.ReadLine(), out var Y)
         || !int.TryParse(Console.ReadLine(), out var R))
            return;

        var grid = Enumerable.Range(0, Y)
                             .Select(_ => Console.ReadLine() ?? "")
                             .ToArray();

        if (R == 1)
        {
            Array.Reverse(grid);
            for (var i = 0; i < Y; i++)
                grid[i] = new string(grid[i].Reverse().ToArray());
        }

        var names = new Dictionary<char, string>
        {
            ['#'] = "Block",
            ['^'] = "Thruster",
            ['@'] = "Gyroscope",
            ['+'] = "Fuel",
            ['§'] = "Core"
        };

        var seq = grid
            .SelectMany(r => r)
            .Where(names.ContainsKey)
            .Select(c => names[c])
            .ToArray();

        if (seq.Length == 0)
        {
            Console.WriteLine("Nothing");
            return;
        }

        var sb = new StringBuilder();
        var prev = seq[0];
        var cnt = 1;
        for (var i = 1; i < seq.Length; i++)
        {
            if (seq[i] == prev) cnt++;
            else
            {
                Append(sb, cnt, prev);
                prev = seq[i];
                cnt = 1;
            }
        }
        Append(sb, cnt, prev);

        var outp = sb.ToString();
        if (outp.EndsWith(", ")) outp = outp[..^2];
        Console.WriteLine(outp);
    }

    private static void Append(StringBuilder sb, int c, string n)
    {
        sb.Append(c).Append(' ').Append(n);
        if (c > 1) sb.Append('s');
        sb.Append(", ");
    }
}
