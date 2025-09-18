using System;

class Solution
{
    static void Main()
    {
        var parts = Console.ReadLine().Split(' ');
        int h = int.Parse(parts[0]);
        int w = int.Parse(parts[1]);
        int n = int.Parse(parts[2]);

        string surviveRule = Console.ReadLine().Trim();
        string birthRule   = Console.ReadLine().Trim();

        bool[] survive = new bool[9], birth = new bool[9];
        for (int i = 0; i < 9; i++)
        {
            survive[i] = surviveRule[i] == '1';
            birth[i]   = birthRule[i]   == '1';
        }

        char[,] grid = new char[h, w];
        for (int i = 0; i < h; i++)
        {
            var row = Console.ReadLine();
            for (int j = 0; j < w; j++)
                grid[i, j] = row[j];
        }

        char[,] next = new char[h, w];
        int[] dx = { -1,-1,-1, 0, 0, 1, 1, 1 };
        int[] dy = { -1, 0, 1,-1, 1,-1, 0, 1 };

        for (int step = 0; step < n; step++)
        {
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    int cnt = 0;
                    for (int d = 0; d < 8; d++)
                    {
                        int ni = i + dy[d], nj = j + dx[d];
                        if (ni < 0 || ni >= h || nj < 0 || nj >= w)
                            continue;
                        if (grid[ni, nj] == 'O') cnt++;
                    }

                    if (grid[i, j] == 'O')
                    {
                        next[i, j] = survive[cnt] ? 'O' : '.';
                    }
                    else
                    {
                        next[i, j] = birth[cnt]   ? 'O' : '.';
                    }
                }
            }

            var tmp = grid;
            grid = next;
            next = tmp;
        }

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
                Console.Write(grid[i, j]);
            Console.WriteLine();
        }
    }
}
