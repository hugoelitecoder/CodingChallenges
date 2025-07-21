using System;
class Solution
{
    static void Main()
    {
        int W = int.Parse(Console.ReadLine());
        int H = int.Parse(Console.ReadLine());
        char[][] grid = new char[H][];
        for (int i = 0; i < H; i++)
            grid[i] = Console.ReadLine().ToCharArray();

        int maxComplete = 0;
        for (int r = 0; r < H - 1; r++)
        for (int c = 0; c < W - 1; c++)
        {
            if (grid[r][c] == '.' && grid[r][c+1] == '.' && grid[r+1][c] == '.' && grid[r+1][c+1] == '.')
            {
                char[][] copy = CopyGrid(grid);
                copy[r][c] = copy[r][c+1] = copy[r+1][c] = copy[r+1][c+1] = '#';
                int complete = CountComplete(copy, W, H);
                if (complete > maxComplete)
                    maxComplete = complete;
            }
        }

        Console.WriteLine(maxComplete);
    }

    static char[][] CopyGrid(char[][] src)
    {
        int H = src.Length;
        char[][] dst = new char[H][];
        for (int i = 0; i < H; i++)
        {
            dst[i] = new char[src[i].Length];
            Array.Copy(src[i], dst[i], src[i].Length);
        }
        return dst;
    }

    static int CountComplete(char[][] grid, int W, int H)
    {
        int count = 0;
        for (int i = 0; i < H; i++)
        {
            bool fullRow = true;
            for (int j = 0; j < W; j++)
                if (grid[i][j] != '#') { fullRow = false; break; }
            if (fullRow) count++;
        }
        for (int j = 0; j < W; j++)
        {
            bool fullCol = true;
            for (int i = 0; i < H; i++)
                if (grid[i][j] != '#') { fullCol = false; break; }
            if (fullCol) count++;
        }
        return count;
    }
}