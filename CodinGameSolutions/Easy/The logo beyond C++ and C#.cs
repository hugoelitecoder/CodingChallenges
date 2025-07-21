using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int size  = int.Parse(Console.ReadLine());
        int thick = int.Parse(Console.ReadLine());
        int N     = int.Parse(Console.ReadLine());

        // Read all text rows and track the maximum length
        var text = new string[N];
        int maxCols = 0;
        for (int i = 0; i < N; i++)
        {
            text[i] = Console.ReadLine();
            if (text[i].Length > maxCols)
                maxCols = text[i].Length;
        }

        // Output grid dimensions
        int rowsOut = N * size;
        int colsOut = maxCols * size;

        // Initialize empty grid
        var grid = new char[rowsOut][];
        for (int r = 0; r < rowsOut; r++)
            grid[r] = new string(' ', colsOut).ToCharArray();

        int V = (size - thick) / 2;

        // Draw each plus
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < text[i].Length; j++)
            {
                if (text[i][j] != '+') continue;

                // Safe neighbor checks
                bool north = (i == 0)
                    || j >= text[i-1].Length
                    || text[i - 1][j] != '+';
                bool south = (i == N - 1)
                    || j >= text[i+1].Length
                    || text[i + 1][j] != '+';
                bool west = (j == 0) || text[i][j - 1] != '+';
                bool east = (j == text[i].Length - 1) || text[i][j + 1] != '+';

                int baseRow = i * size;
                int baseCol = j * size;

                // Outer stroke
                for (int k = 0; k < thick; k++)
                {
                    if (north) grid[baseRow            ][baseCol + V + k] = '+';
                    if (south) grid[baseRow + size - 1][baseCol + V + k] = '+';
                    if (west ) grid[baseRow + V + k    ][baseCol          ] = '+';
                    if (east ) grid[baseRow + V + k    ][baseCol + size - 1] = '+';
                }

                // Inner verticals
                for (int rr = baseRow; rr < baseRow + size; rr++)
                {
                    int off = rr - baseRow;
                    if (off > V && off < V + thick - 1) continue;
                    grid[rr][baseCol + V]             = '+';
                    grid[rr][baseCol + V + thick - 1] = '+';
                }

                // Inner horizontals
                for (int cc = baseCol; cc < baseCol + size; cc++)
                {
                    int off = cc - baseCol;
                    if (off > V && off < V + thick - 1) continue;
                    grid[baseRow + V            ][cc] = '+';
                    grid[baseRow + V + thick - 1][cc] = '+';
                }
            }
        }

        // Trim trailing spaces and output
        var trimRegex = new Regex(@"\s+$");
        for (int r = 0; r < rowsOut; r++)
        {
            string line = new string(grid[r]);
            Console.WriteLine(trimRegex.Replace(line, ""));
        }
    }
}
