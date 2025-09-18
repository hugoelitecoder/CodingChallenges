using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        var input = Console.ReadLine().Split().Select(long.Parse).ToArray();
        long s = input[0], n = input[1];
        long prev;
        for (long i = 0, steps = Math.Min(n, 6); i < steps; i++)
        {
            prev = s;
            var bin = Convert.ToString(s, 2);
            s = bin.Length * 4L - bin.Count(c => c == '1');
            if (s == prev) break;
        }
        Console.WriteLine(s);
    }
}
