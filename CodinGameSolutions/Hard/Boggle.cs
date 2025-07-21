using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        char[][] grid = new char[4][];
        for (int i = 0; i < 4; i++)
            grid[i] = Console.ReadLine().Trim().ToCharArray();

        int N = int.Parse(Console.ReadLine());
        List<string> words = new List<string>();
        for (int i = 0; i < N; i++)
            words.Add(Console.ReadLine().Trim());

        foreach (string word in words)
        {
            Console.WriteLine(CanForm(grid, word).ToString().ToLower());
        }
    }

    static int[] dx = { -1, -1, -1,  0, 0, 1, 1, 1 };
    static int[] dy = { -1,  0,  1, -1, 1,-1, 0, 1 };

    static bool Search(char[][] grid, string word, int x, int y, int pos, bool[,] visited)
    {
        if (pos == word.Length)
            return true;

        if (x < 0 || x >= 4 || y < 0 || y >= 4)
            return false;

        if (visited[x, y])
            return false;

        if (grid[x][y] != word[pos])
            return false;

        visited[x, y] = true;
        for (int dir = 0; dir < 8; dir++)
        {
            int nx = x + dx[dir];
            int ny = y + dy[dir];
            if (Search(grid, word, nx, ny, pos + 1, visited))
            {
                visited[x, y] = false;
                return true;
            }
        }
        visited[x, y] = false;
        return false;
    }

    static bool CanForm(char[][] grid, string word)
    {
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                bool[,] visited = new bool[4, 4];
                if (Search(grid, word, x, y, 0, visited))
                    return true;
            }
        }
        return false;
    }
}
