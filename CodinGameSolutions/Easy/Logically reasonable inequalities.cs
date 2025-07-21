using System;
using System.Collections.Generic;

class Solution
{
    static int[] color = new int[26];
    static List<int>[] adj = new List<int>[26];

    static bool DFS(int u)
    {
        color[u] = 1;
        foreach (var v in adj[u])
        {
            if (color[v] == 1) return true;
            if (color[v] == 0 && DFS(v)) return true;
        }
        color[u] = 2;
        return false;
    }

    static void Main()
    {
        var n = int.Parse(Console.ReadLine());
        for (var i = 0; i < 26; i++) adj[i] = new List<int>();

        var present = new bool[26];
        for (var i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split('>');
            var u = parts[0].Trim()[0] - 'A';
            var v = parts[1].Trim()[0] - 'A';
            adj[u].Add(v);
            present[u] = present[v] = true;
        }

        for (var i = 0; i < 26; i++)
            if (present[i] && color[i] == 0 && DFS(i))
            {
                Console.WriteLine("contradiction");
                return;
            }

        Console.WriteLine("consistent");
    }
}
