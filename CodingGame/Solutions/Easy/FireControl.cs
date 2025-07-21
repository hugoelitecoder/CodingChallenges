using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        const int N = 6;
        char[,] grid = new char[N, N];
        int totalTrees = 0;
        var fires = new List<(int r, int c)>();

        for (int r = 0; r < N; r++)
        {
            string line = Console.ReadLine() ?? string.Empty;
            for (int c = 0; c < N && c < line.Length; c++)
            {
                char ch = line[c];
                grid[r, c] = ch;
                if (ch == '#') totalTrees++;
                else if (ch == '*') fires.Add((r, c));
            }
        }

        if (fires.Count == 0)
        {
            Console.WriteLine("RELAX");
            return;
        }

        bool[,] marked = new bool[N, N];
        int cutCount = 0;
        foreach (var (fr, fc) in fires)
        {
            for (int dr = -2; dr <= 2; dr++)
            for (int dc = -2; dc <= 2; dc++)
            {
                int nr = fr + dr, nc = fc + dc;
                if (nr < 0 || nr >= N || nc < 0 || nc >= N) continue;
                if (grid[nr, nc] == '#')
                {
                    if (!marked[nr, nc])
                    {
                        marked[nr, nc] = true;
                        cutCount++;
                    }
                }
            }
        }
        if (cutCount >= totalTrees)
        {
            Console.WriteLine("JUST RUN");
        }
        else
        {
            Console.WriteLine(cutCount);
        }
    }
}