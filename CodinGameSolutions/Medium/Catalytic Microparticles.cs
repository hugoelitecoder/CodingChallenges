using System;
using System.Collections.Generic;

class Solution
{
    static readonly int[] dx = { 1, -1, 0, 0, 0, 0 };
    static readonly int[] dy = { 0, 0, 1, -1, 0, 0 };
    static readonly int[] dz = { 0, 0, 0, 0, 1, -1 };

    static void Main()
    {
        var parts = Console.ReadLine().Split();
        int L = int.Parse(parts[0]);
        int W = int.Parse(parts[1]);
        int H = int.Parse(parts[2]);
        int PX = L + 2, PY = H + 2, PZ = W + 2;

        char[,,] grid = new char[PX, PY, PZ];
        for (int x = 0; x < PX; x++)
            for (int y = 0; y < PY; y++)
                for (int z = 0; z < PZ; z++)
                    grid[x, y, z] = 'o';

        for (int x = 1; x <= L; x++)
        {
            var line = Console.ReadLine().Split();
            for (int y = 1; y <= H; y++)
            {
                var sliceRow = line[y - 1];
                for (int z = 1; z <= W; z++)
                    grid[x, y, z] = sliceRow[z - 1];
            }
        }

        var queue = new Queue<(int x, int y, int z)>();
        queue.Enqueue((0, 0, 0));
        grid[0, 0, 0] = 'x';
        while (queue.Count > 0)
        {
            var (x, y, z) = queue.Dequeue();
            for (int i = 0; i < 6; i++)
            {
                int nx = x + dx[i], ny = y + dy[i], nz = z + dz[i];
                if (nx >= 0 && nx < PX && ny >= 0 && ny < PY && nz >= 0 && nz < PZ)
                {
                    if (grid[nx, ny, nz] == 'o')
                    {
                        grid[nx, ny, nz] = 'x';
                        queue.Enqueue((nx, ny, nz));
                    }
                }
            }
        }

        int solidCount = 0;
        int interiorVoidCount = 0;
        long surfaceArea = 0;

        for (int x = 1; x <= L; x++)
        for (int y = 1; y <= H; y++)
        for (int z = 1; z <= W; z++)
        {
            char c = grid[x, y, z];
            if (c == '#')
            {
                solidCount++;
                for (int i = 0; i < 6; i++)
                {
                    int nx = x + dx[i], ny = y + dy[i], nz = z + dz[i];
                    if (grid[nx, ny, nz] == 'x') surfaceArea++;
                }
            }
            else if (c == 'o')
            {
                interiorVoidCount++;
            }
        }

        double activity = (double)surfaceArea / solidCount;
        double density = (double)solidCount / (solidCount + interiorVoidCount);

        Console.WriteLine($"{activity:F4} {density:F4}");
    }
}
