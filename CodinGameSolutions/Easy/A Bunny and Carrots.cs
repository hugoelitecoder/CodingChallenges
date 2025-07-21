using System;

class Solution
{
    static void Main()
    {
        // Read dimensions
        var parts = Console.ReadLine()!.Split();
        int M = int.Parse(parts[0]);
        int N = int.Parse(parts[1]);

        // Read number of moves
        int T = int.Parse(Console.ReadLine()!);
        var moves = Array.ConvertAll(Console.ReadLine()!.Split(), int.Parse);

        // Initialize grid: true = carrot present
        bool[,] grid = new bool[M, N];
        for (int r = 0; r < M; r++)
            for (int c = 0; c < N; c++)
                grid[r, c] = true;

        // heights[c] = number of carrots currently in column c (from bottom up)
        int[] heights = new int[N];
        for (int c = 0; c < N; c++)
            heights[c] = M;

        // For each move, remove topmost carrot in that column and compute perimeter
        foreach (int move in moves)
        {
            int col = move - 1;         // convert to 0-based
            // remove topmost carrot: at row index heights[col]-1
            int rowToRemove = heights[col] - 1;
            grid[rowToRemove, col] = false;
            heights[col]--;

            // compute perimeter
            int peri = 0;
            for (int r = 0; r < M; r++)
            {
                for (int c = 0; c < N; c++)
                {
                    if (!grid[r, c]) continue;
                    // up
                    if (r + 1 >= M || !grid[r + 1, c]) peri++;
                    // down
                    if (r - 1 < 0   || !grid[r - 1, c]) peri++;
                    // right
                    if (c + 1 >= N || !grid[r, c + 1]) peri++;
                    // left
                    if (c - 1 < 0   || !grid[r, c - 1]) peri++;
                }
            }

            Console.WriteLine(peri);
        }
    }
}
