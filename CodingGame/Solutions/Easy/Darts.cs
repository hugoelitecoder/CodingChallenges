using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var SIZE = int.Parse(Console.ReadLine());
        var half = SIZE / 2.0;

        var N = int.Parse(Console.ReadLine());
        var names = new string[N];
        var indexOf = new Dictionary<string, int>();
        for (int i = 0; i < N; i++)
        {
            names[i] = Console.ReadLine();
            indexOf[names[i]] = i;
        }

        var scores = new int[N];
        var T = int.Parse(Console.ReadLine());
        for (int t = 0; t < T; t++)
        {
            var parts = Console.ReadLine().Split();
            var who = parts[0];
            var x = int.Parse(parts[1]);
            var y = int.Parse(parts[2]);

            int pts = 0;
            if (Math.Abs(x) <= half && Math.Abs(y) <= half)
            {
                if (x * x + y * y <= half * half)
                {
                    if (Math.Abs(x) + Math.Abs(y) <= half)
                        pts = 15;
                    else
                        pts = 10;
                }
                else
                {
                    pts = 5;
                }
            }
            scores[indexOf[who]] += pts;
        }

        var order = Enumerable.Range(0, N)
            .OrderByDescending(i => scores[i])
            .ThenBy(i => i)
            .ToList();

        foreach (var i in order)
            Console.WriteLine($"{names[i]} {scores[i]}");
    }
}
