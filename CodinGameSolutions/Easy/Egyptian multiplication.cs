using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var parts = Console.ReadLine().Split(' ');
        var a0 = int.Parse(parts[0]);
        var b0 = int.Parse(parts[1]);
        var big   = a0 >= b0 ? a0 : b0;
        var small = a0 >= b0 ? b0 : a0;

        Console.WriteLine($"{big} * {small}");
        var adds    = new List<int>();
        var currBig = big;
        var s       = small;

        while (s != 0)
        {
            if (s % 2 == 1)
            {
                adds.Add(currBig);
                s = s - 1;
                Console.Write($"= {currBig} * {s}");
            }
            else
            {
                currBig = currBig * 2;
                s       = s / 2;
                Console.Write($"= {currBig} * {s}");
            }

            foreach (var x in adds)
                Console.Write($" + {x}");
            Console.WriteLine();
        }

        var result = 0;
        foreach (var x in adds) result += x;
        Console.WriteLine($"= {result}");
    }
}
