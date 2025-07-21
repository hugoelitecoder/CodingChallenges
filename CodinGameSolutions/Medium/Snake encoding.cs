using System;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        long X = long.Parse(Console.ReadLine());

        char[][] grid = new char[N][];
        for (int r = 0; r < N; r++)
            grid[r] = Console.ReadLine().ToCharArray();

        int L = N * N;
        int shift = (int)(X % L);

        var path = new (int r, int c)[L];
        int idx = 0;
        for (int c = 0; c < N; c++)
        {
            if ((c & 1) == 0)
                for (int r = N - 1; r >= 0; r--)
                    path[idx++] = (r, c);
            else
                for (int r = 0; r < N; r++)
                    path[idx++] = (r, c);
        }

        char[][] result = new char[N][];
        for (int r = 0; r < N; r++)
            result[r] = new char[N];

        for (int i = 0; i < L; i++)
        {
            var dest = path[i];
            var src  = path[(i - shift + L) % L];
            result[dest.r][dest.c] = grid[src.r][src.c];
        }

        for (int r = 0; r < N; r++)
            Console.WriteLine(new string(result[r]));
    }
}
