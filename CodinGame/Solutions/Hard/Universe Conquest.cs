using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var P = int.Parse(inputs[0]);
        var H = int.Parse(inputs[1]);
        var faction = new char[P];
        var ships = new int[P];
        for (var i = 0; i < P; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            faction[i] = inputs[0][0];
            ships[i] = int.Parse(inputs[1]);
        }
        var edges = new List<(int, int)>();
        for (var i = 0; i < H; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var a = int.Parse(inputs[0]) - 1;
            var b = int.Parse(inputs[1]) - 1;
            edges.Add((a, b));
        }
        var minShips = int.MaxValue;
        var N = P;
        var maxMask = 1 << N;
        for (var mask = 0; mask < maxMask; mask++)
        {
            var destroyed = new bool[N];
            var totalShips = 0;
            for (var i = 0; i < N; i++)
            {
                if (((mask >> i) & 1) == 1)
                {
                    destroyed[i] = true;
                    totalShips += ships[i];
                }
            }
            var ok = true;
            foreach (var (a, b) in edges)
            {
                if (destroyed[a] || destroyed[b]) continue;
                if (faction[a] != faction[b]) { ok = false; break; }
            }
            if (ok && totalShips < minShips) minShips = totalShips;
        }
        Console.WriteLine(minShips);
    }
}
