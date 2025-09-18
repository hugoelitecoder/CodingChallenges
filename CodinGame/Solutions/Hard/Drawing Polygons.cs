using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var x = new int[n];
        var y = new int[n];
        for (var i = 0; i < n; ++i)
        {
            var s = Console.ReadLine().Split();
            x[i] = int.Parse(s[0]);
            y[i] = int.Parse(s[1]);
        }
        var area = 0;
        for (var i = 0; i < n; ++i)
        {
            var j = (i + 1) % n;
            area += x[i] * y[j] - x[j] * y[i];
        }
        if (area < 0)
            Console.WriteLine("CLOCKWISE");
        else
            Console.WriteLine("COUNTERCLOCKWISE");
    }
}
