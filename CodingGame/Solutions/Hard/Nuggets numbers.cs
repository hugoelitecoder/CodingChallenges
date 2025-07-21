using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var boxes = new int[n];
        for (var i = 0; i < n; ++i)
            boxes[i] = int.Parse(Console.ReadLine());

        var result = GetLargestImpossible(boxes);
        Console.WriteLine(result);
    }

    static int GetLargestImpossible(int[] boxes)
    {
        var g = boxes[0];
        foreach (var b in boxes)
            g = GCD(g, b);
        if (g != 1) return -1;

        var maxBox = boxes.Max();
        var bound = maxBox * maxBox;
        var possible = new bool[bound + 1];
        possible[0] = true;
        for (var i = 1; i <= bound; ++i)
        {
            foreach (var b in boxes)
            {
                if (i - b >= 0 && possible[i - b])
                {
                    possible[i] = true;
                    break;
                }
            }
        }
        var lastImpossible = -1;
        var streak = 0;
        for (var i = 1; i <= bound; ++i)
        {
            if (possible[i])
            {
                streak++;
                if (streak >= maxBox) break;
            }
            else
            {
                lastImpossible = i;
                streak = 0;
            }
        }
        return lastImpossible;
    }

    static int GCD(int a, int b)
    {
        while (b != 0)
        {
            var t = b;
            b = a % b;
            a = t;
        }
        return a;
    }
}
