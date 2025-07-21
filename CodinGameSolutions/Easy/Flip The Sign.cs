using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var parts = Console.ReadLine().Split(' ');
        var height = int.Parse(parts[0]);
        var width = int.Parse(parts[1]);
        var values = new int[height][];
        for (var i = 0; i < height; i++)
            values[i] = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
        var mask = new string[height];
        for (var i = 0; i < height; i++)
            mask[i] = Console.ReadLine().Replace(" ", string.Empty);
        var seq = new List<int>();
        for (var i = 0; i < height; i++)
            for (var j = 0; j < width; j++)
                if (mask[i][j] == 'X')
                    seq.Add(values[i][j]);
        var ok = true;
        for (var k = 1; k < seq.Count; k++)
            if (seq[k] * seq[k - 1] > 0)
            {
                ok = false;
                break;
            }
        Console.WriteLine(ok.ToString().ToLower());
    }
}