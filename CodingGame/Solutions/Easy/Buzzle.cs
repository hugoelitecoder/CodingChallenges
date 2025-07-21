using System;
using System.Linq;
using System.Collections.Generic;

public class Solution
{
    public static void Main()
    {
        var parts = Console.ReadLine().Split();
        var n = int.Parse(parts[0]);
        var a = int.Parse(parts[1]);
        var b = int.Parse(parts[2]);
        _ = int.Parse(Console.ReadLine());
        var nums = Console.ReadLine().Split().Select(int.Parse).ToHashSet();

        for (var x = a; x <= b; x++)
            Console.WriteLine(IsBuzzle(x, n, nums) ? "Buzzle" : x.ToString());
    }

    private static bool IsBuzzle(long x, int b, HashSet<int> ks)
    {
        var ax = Math.Abs(x);
        foreach (var k in ks)
            if (ax % k == 0) return true;
        var last = (int)(ax % b);
        if (ks.Contains(last)) return true;
        if (ax < b) return false;
        long sum = 0;
        var t = ax;
        while (t > 0)
        {
            sum += t % b;
            t /= b;
        }
        return IsBuzzle(sum, b, ks);
    }
}