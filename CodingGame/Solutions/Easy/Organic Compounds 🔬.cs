using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var lines = new List<string>(N);
        int maxW = 0;
        for (int i = 0; i < N; i++)
        {
            var line = Console.ReadLine();
            lines.Add(line);
            if (line.Length > maxW) maxW = line.Length;
        }

        var grid = new char[N][];
        for (int r = 0; r < N; r++)
        {
            var line = lines[r].PadRight(maxW, ' ');
            grid[r] = line.ToCharArray();
        }

        bool IsFence(char ch) => ch == '|' || ch == '-' || ch == '+';
        bool valid = true;
        for (int r = 0; r < N && valid; r++)
        {
            for (int c = 0; c + 2 < maxW && valid; c++)
            {
                if (grid[r][c] == 'C' &&
                    c + 1 < maxW && grid[r][c+1] == 'H' &&
                    c + 2 < maxW && char.IsDigit(grid[r][c+2]))
                {
                    int h = grid[r][c+2] - '0';
                    int needed = 4 - h;
                    int sumBonds = 0;

                    // Right
                    if (c + 5 < maxW &&
                        grid[r][c+3] == '(' &&
                        char.IsDigit(grid[r][c+4]) &&
                        grid[r][c+5] == ')')
                        sumBonds += grid[r][c+4] - '0';

                    // Left
                    if (c - 3 >= 0 &&
                        grid[r][c-3] == '(' &&
                        char.IsDigit(grid[r][c-2]) &&
                        grid[r][c-1] == ')')
                        sumBonds += grid[r][c-2] - '0';

                    // Down
                    if (r + 1 < N &&
                        c + 2 < maxW &&
                        grid[r+1][c]   == '(' &&
                        char.IsDigit(grid[r+1][c+1]) &&
                        grid[r+1][c+2] == ')')
                        sumBonds += grid[r+1][c+1] - '0';

                    // Up
                    if (r - 1 >= 0 &&
                        c + 2 < maxW &&
                        grid[r-1][c]   == '(' &&
                        char.IsDigit(grid[r-1][c+1]) &&
                        grid[r-1][c+2] == ')')
                        sumBonds += grid[r-1][c+1] - '0';

                    if (sumBonds != needed)
                        valid = false;
                }
            }
        }

        Console.WriteLine(valid ? "VALID" : "INVALID");
    }
}
