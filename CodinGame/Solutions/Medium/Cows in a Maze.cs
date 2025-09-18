using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var parts = Console.ReadLine().Split();
        int C = int.Parse(parts[0]);
        int N = int.Parse(parts[1]);
        int M = int.Parse(parts[2]);

        int[] endurance = new int[C];
        for (int i = 0; i < C; i++)
            endurance[i] = int.Parse(Console.ReadLine());

        int[,] maze = new int[N, M];
        for (int i = 0; i < N; i++)
        {
            var row = Console.ReadLine().Split();
            for (int j = 0; j < M; j++)
                maze[i, j] = int.Parse(row[j]);
        }

        int passCount = 0;
        for (int i = 0; i < C; i++)
        {
            if (CanPass(maze, N, M, endurance[i]))
                passCount++;
        }

        Console.WriteLine(passCount);
    }

    static bool CanPass(int[,] maze, int N, int M, int maxDifficulty)
    {
        if (maze[0, 0] > maxDifficulty || maze[N - 1, M - 1] > maxDifficulty)
            return false;

        bool[,] visited = new bool[N, M];
        var queue = new Queue<(int r, int c)>();
        queue.Enqueue((0, 0));
        visited[0, 0] = true;

        int[] dr = { -1, 1, 0, 0 };
        int[] dc = { 0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();
            if (r == N - 1 && c == M - 1)
                return true;

            for (int d = 0; d < 4; d++)
            {
                int nr = r + dr[d];
                int nc = c + dc[d];
                if (nr >= 0 && nr < N && nc >= 0 && nc < M && !visited[nr, nc]
                    && maze[nr, nc] <= maxDifficulty)
                {
                    visited[nr, nc] = true;
                    queue.Enqueue((nr, nc));
                }
            }
        }

        return false;
    }
}