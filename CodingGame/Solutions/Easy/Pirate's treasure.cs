using System;

class Solution
{
    static void Main()
    {
        int W = int.Parse(Console.ReadLine());
        int H = int.Parse(Console.ReadLine());
        int[,] grid = new int[H, W];
        for (int y = 0; y < H; y++)
        {
            var parts = Console.ReadLine().Split(' ');
            for (int x = 0; x < W; x++)
                grid[y, x] = int.Parse(parts[x]);
        }

        int[] dx = { -1,  0,  1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1,  0, 0,  1, 1, 1 };
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                if (grid[y, x] != 0)  continue;
                int totalNbr = 0, obsCount = 0;
                for (int dir = 0; dir < 8; dir++)
                {
                    int nx = x + dx[dir], ny = y + dy[dir];
                    if (nx < 0 || nx >= W || ny < 0 || ny >= H)  continue;
                    totalNbr++;
                    if (grid[ny, nx] == 1) 
                        obsCount++;
                }
                if (obsCount == totalNbr &&
                    (totalNbr == 3 || totalNbr == 5 || totalNbr == 8))
                {
                    Console.WriteLine($"{x} {y}");
                    return;
                }
            }
        }
    }
}
