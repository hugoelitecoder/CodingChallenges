using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var orbSizeMin = int.Parse(inputs[0]);
        var orbSizeMax = int.Parse(inputs[1]);
        inputs = Console.ReadLine().Split(' ');
        var g1 = int.Parse(inputs[0]);
        var g2 = int.Parse(inputs[1]);
        var T = (long)g1 * g1 * g1 + (long)g2 * g2 * g2;
        var dict = new Dictionary<long,int>();
        for (var d = orbSizeMin; d <= orbSizeMax; d++)
            dict[(long)d * d * d] = d;
        var hasValid = false;
        var bestS1 = 0; var bestS2 = 0;
        long bestInterest = -1;
        for (var s1 = orbSizeMin; s1 <= orbSizeMax; s1++)
        {
            var rem = T - (long)s1 * s1 * s1;
            if (dict.TryGetValue(rem, out var s2))
            {
                hasValid = true;
                if (s1 != s2 && s1 != g1 && s1 != g2 && s2 != g1 && s2 != g2)
                {
                    var a = s1; var b = s2;
                    if (a > b) { var tmp = a; a = b; b = tmp; }
                    var interest = (long)b * b - (long)a * a;
                    if (interest > bestInterest)
                    {
                        bestInterest = interest;
                        bestS1 = a; bestS2 = b;
                    }
                }
            }
        }
        if (bestInterest >= 0)
        {
            Console.WriteLine($"{bestS1} {bestS2}");
            return;
        }
        if (hasValid)
        {
            Console.WriteLine("VALID");
            return;
        }
        Console.WriteLine("IMPOSSIBLE");
    }
}
