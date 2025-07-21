using System;

class Solution
{
    static void Main()
    {
        int steps = int.Parse(Console.ReadLine()!);
        int h = int.Parse(Console.ReadLine()!);
        int w = int.Parse(Console.ReadLine()!);

        // Read initial grid
        var grid = new char[h][];
        for (int i = 0; i < h; i++)
            grid[i] = Console.ReadLine()!.ToCharArray();

        // Temporary buffers
        var tmp = new char[h][];
        for (int i = 0; i < h; i++)
            tmp[i] = new char[w];

        for (int s = 0; s < steps; s++)
        {
            // 1) Column shifts
            // Compute each column's weight mod h
            var colShift = new int[w];
            for (int c = 0; c < w; c++)
            {
                int sum = 0;
                for (int r = 0; r < h; r++)
                    sum += grid[r][c];
                colShift[c] = sum % h;
            }

            // Apply column shifts into tmp
            for (int r = 0; r < h; r++)
                for (int c = 0; c < w; c++)
                {
                    int nr = (r + colShift[c]) % h;
                    tmp[nr][c] = grid[r][c];
                }

            // Copy back to grid
            for (int r = 0; r < h; r++)
                Array.Copy(tmp[r], grid[r], w);

            // 2) Row shifts
            // Compute each row's weight mod w
            var rowShift = new int[h];
            for (int r = 0; r < h; r++)
            {
                int sum = 0;
                for (int c = 0; c < w; c++)
                    sum += grid[r][c];
                rowShift[r] = sum % w;
            }

            // Apply row shifts into tmp
            for (int r = 0; r < h; r++)
            {
                int k = rowShift[r];
                for (int c = 0; c < w; c++)
                {
                    int nc = (c + k) % w;
                    tmp[r][nc] = grid[r][c];
                }
            }

            // Copy back to grid
            for (int r = 0; r < h; r++)
                Array.Copy(tmp[r], grid[r], w);
        }

        // Output final grid
        for (int r = 0; r < h; r++)
            Console.WriteLine(new string(grid[r]));
    }
}
