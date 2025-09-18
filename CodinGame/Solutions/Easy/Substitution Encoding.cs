using System;
using System.Collections.Generic;
using System.Text;

class Solution
{
    public static void Main(string[] args)
    {
        var rows = int.Parse(Console.ReadLine());
        var map = new Dictionary<char, (int r, int c)>();
        for (var r = 0; r < rows; r++)
        {
            var cols = Console.ReadLine().Split(' ');
            for (var c = 0; c < cols.Length; c++)
            {
                var ch = cols[c][0];
                map[ch] = (r, c);
            }
        }
        var message = Console.ReadLine();
        var sb = new StringBuilder();
        foreach (var ch in message)
        {
            var pos = map[ch];
            sb.Append(pos.r);
            sb.Append(pos.c);
        }
        Console.WriteLine(sb.ToString());
    }
}
