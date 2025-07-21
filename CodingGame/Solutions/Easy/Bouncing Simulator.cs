using System;

class Solution
{
    static void Main()
    {
        int w = int.Parse(Console.ReadLine());
        int h = int.Parse(Console.ReadLine());
        int n = int.Parse(Console.ReadLine());

        int[,] grid = new int[h + 1, w + 1];
        int r = 1, c = 1;
        grid[r, c]++;
        int dr = 1, dc = 1;
        if (w == 1) dc = 0;
        if (h == 1) dr = 0;
        if (w == 1 || h == 1)
        {
            for (int step = 0; step < n-1; step++)
            {
                int nr = r + dr, nc = c + dc;
                if (h > 1 && (nr < 1 || nr > h))
                {
                    dr = -dr;
                    nr = r + dr;
                }
                if (w > 1 && (nc < 1 || nc > w))
                {
                    dc = -dc;
                    nc = c + dc;
                }
                r = nr; c = nc;
                grid[r, c]++;
            }
        }
        else
        {
            int bounces = 0;
            while (bounces < n)
            {
                int nr = r + dr, nc = c + dc;
                bool hitV = nr < 1 || nr > h;
                bool hitH = nc < 1 || nc > w;
                if (hitV || hitH)
                {
                    if (hitV) dr = -dr;
                    if (hitH) dc = -dc;
                    bounces++;
                    if (bounces == n) break;
                    nr = r + dr; nc = c + dc;
                }
                r = nr; c = nc;
                grid[r, c]++;
            }
        }

        Console.WriteLine(new string('#', w + 2));
        for (int row = 1; row <= h; row++)
        {
            Console.Write('#');
            for (int col = 1; col <= w; col++)
            {
                int cnt = grid[row, col];
                Console.Write(cnt > 0 ? (char)('0' + cnt) : ' ');
            }
            Console.WriteLine('#');
        }
        Console.WriteLine(new string('#', w + 2));
    }
}
