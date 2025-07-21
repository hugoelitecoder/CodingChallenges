using System;
using System.Collections.Generic;

class Solution
{
    static readonly int[][] KnightMoves = {
        new[]{ 2, 1 }, new[]{ 2, -1 }, new[]{ -2, 1 }, new[]{ -2, -1 },
        new[]{ 1, 2 }, new[]{ 1, -2 }, new[]{ -1, 2 }, new[]{ -1, -2 }
    };

    static List<int>[] BuildNeighbors()
    {
        var nbr = new List<int>[9];
        for (int i = 0; i < 9; i++)
        {
            nbr[i] = new List<int>();
            int r = i / 3, c = i % 3;
            foreach (var mv in KnightMoves)
            {
                int nr = r + mv[0], nc = c + mv[1];
                if (nr >= 0 && nr < 3 && nc >= 0 && nc < 3)
                    nbr[i].Add(nr * 3 + nc);
            }
        }
        return nbr;
    }

    static int BFS(string start, string target, List<int>[] nbr)
    {
        var visited = new HashSet<string>();
        var q = new Queue<(string state, int dist)>();
        visited.Add(start);
        q.Enqueue((start, 0));

        while (q.Count > 0)
        {
            var (s, d) = q.Dequeue();
            if (s == target) return d;

            int bi = s.IndexOf('.');
            foreach (int ni in nbr[bi])
            {
                char[] arr = s.ToCharArray();
                arr[bi] = arr[ni];
                arr[ni] = '.';
                string ns = new string(arr);
                if (visited.Add(ns))
                    q.Enqueue((ns, d + 1));
            }
        }
        return -1;
    }

    static void Main()
    {
        char[] buf = new char[9];
        for (int i = 0, p = 0; i < 3; i++)
        {
            string line = Console.ReadLine();
            for (int j = 0; j < 3; j++)
                buf[p++] = line[j];
        }
        string start = new string(buf);
        const string target = "12345678.";

        var neighbors = BuildNeighbors();
        int ans = BFS(start, target, neighbors);
        Console.WriteLine(ans);
    }
}
