using System;
using System.Collections.Generic;
using System.Numerics;

class Solution
{
    static void Main(string[] args)
    {
        var N = int.Parse(Console.ReadLine());
        var lines = new string[N];
        for (var i = 0; i < N; i++)
            lines[i] = Console.ReadLine();
        var idx = 1;
        var result = ParseBlock(lines, ref idx);
        Console.WriteLine(result);
    }

    private static BigInteger ParseBlock(string[] lines, ref int idx)
    {
        var paths = BigInteger.One;
        while (idx < lines.Length)
        {
            var line = lines[idx];
            if (line == "if")
            {
                idx++;
                var truePaths = ParseBlock(lines, ref idx);
                idx++;
                var falsePaths = ParseBlock(lines, ref idx);
                idx++;
                paths *= (truePaths + falsePaths);
            }
            else if (line == "else" || line == "endif" || line == "end")
            {
                break;
            }
            else
            {
                idx++;
            }
        }
        return paths;
    }
}
