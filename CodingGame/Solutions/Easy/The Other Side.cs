using System;
using System.Collections.Generic;

class Solution
{
    static int h, w;
    static string[,] grid;

    static void Main()
    {
        h = int.Parse(Console.ReadLine());
        w = int.Parse(Console.ReadLine());
        grid = new string[h, w];

        for (int i = 0; i < h; i++)
        {
            var row = Console.ReadLine().Split(' ');
            for (int j = 0; j < w; j++)
                grid[i, j] = row[j];
        }

        int count = 0;
        for (int r = 0; r < h; r++)
        {
            if (grid[r, 0] == "+" && BFS(r, 0))
                count++;
        }

        Console.WriteLine(count);
    }

    static bool BFS(int sr, int sc)
    {
        var visited = new bool[h, w];
        var queue = new Queue<(int r, int c)>();
        queue.Enqueue((sr, sc));
        visited[sr, sc] = true;

        int[] dr = { -1, 1, 0, 0 };
        int[] dc = { 0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();
            if (c == w - 1)
                return true;

            for (int d = 0; d < 4; d++)
            {
                int nr = r + dr[d], nc = c + dc[d];
                if (nr >= 0 && nr < h && nc >= 0 && nc < w &&
                    !visited[nr, nc] && grid[nr, nc] == "+")
                {
                    visited[nr, nc] = true;
                    queue.Enqueue((nr, nc));
                }
            }
        }

        return false;
    }
}
