using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var input = Console.ReadLine();
        var length = input.Length;
        var sequence = new List<string>();
        var seen = new HashSet<string>();
        var current = input;

        while (seen.Add(current))
        {
            sequence.Add(current);
            current = Next(current, length);
        }

        var start = sequence.IndexOf(current);
        var cycle = sequence.GetRange(start, sequence.Count - start);
        Console.WriteLine(string.Join(" ", cycle));
    }

    private static string Next(string s, int length)
    {
        var desc = new string(s.OrderByDescending(c => c).ToArray());
        var asc  = new string(s.OrderBy(c => c)         .ToArray());
        var diff = int.Parse(desc) - int.Parse(asc);
        return diff.ToString().PadLeft(length, '0');
    }
}
