using System;
class Solution
{
    public static void Main(string[] args)
    {
        var N = Console.ReadLine();
        var prevCount = 1;
        for (var b = 2; b <= 9; b++)
        {
            var currentRem = (N[0] - '0') % b;
            var clumps = 1;
            for (var i = 1; i < N.Length; i++)
            {
                var rem = (N[i] - '0') % b;
                if (rem != currentRem) { clumps++; currentRem = rem; }
            }
            if (clumps < prevCount) { Console.WriteLine(b); return; }
            prevCount = clumps;
        }
        Console.WriteLine("Normal");
    }
}
