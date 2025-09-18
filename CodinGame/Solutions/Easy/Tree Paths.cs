using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var N = int.Parse(Console.ReadLine());
        var V = int.Parse(Console.ReadLine());
        var M = int.Parse(Console.ReadLine());

        var parent = new int[N + 1];
        var left = new int[N + 1];
        var right = new int[N + 1];

        for (var i = 0; i < M; i++)
        {
            var parts = Console.ReadLine().Split();
            var P = int.Parse(parts[0]);
            var L = int.Parse(parts[1]);
            var R = int.Parse(parts[2]);
            left[P] = L; right[P] = R;
            parent[L] = parent[R] = P;
        }

        var root = 1;
        while (root <= N && parent[root] != 0) root++;

        if (V == root)
        {
            Console.WriteLine("Root");
            return;
        }

        var moves = new List<string>();
        for (var cur = V; cur != root; cur = parent[cur])
            moves.Add(left[parent[cur]] == cur ? "Left" : "Right");

        moves.Reverse();
        Console.WriteLine(string.Join(" ", moves));
    }
}
