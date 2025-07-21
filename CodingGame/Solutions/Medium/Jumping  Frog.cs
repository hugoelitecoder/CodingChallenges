using System;

class Solution
{
    static int N;
    static char[][] grid;
    static bool[][] visited;
    static int best;
    static int total;
    static bool done;
    static int[] dx = { -1, 1, 0, 0, -1, -1, 1, 1 };
    static int[] dy = { 0, 0, -1, 1, -1, 1, -1, 1 };

    static void Main()
    {
        N = int.Parse(Console.ReadLine());
        grid = new char[N][];
        for (int i = 0; i < N; i++)
            grid[i] = Console.ReadLine().ToCharArray();
        int startX = int.Parse(Console.ReadLine());
        int startY = int.Parse(Console.ReadLine());

        visited = new bool[N][];
        for (int i = 0; i < N; i++)
            visited[i] = new bool[N];

        total = 0;
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
                if (grid[y][x] == '#') total++;

        best = 0;
        done = false;
        if (InBounds(startX, startY) && grid[startY][startX] == '#')
            DFS(startX, startY, 1);

        Console.WriteLine(best);
    }

    static void DFS(int x, int y, int length)
    {
        if (done) return;

        visited[y][x] = true;
        if (length > best)
        {
            best = length;
            if (best == total)
            {
                done = true;
                visited[y][x] = false;
                return;
            }
        }
        for (int dir = 0; dir < 4; dir++)
        {
            for (int step = 1; step <= 2 && !done; step++)
            {
                int nx = x + dx[dir] * step;
                int ny = y + dy[dir] * step;
                TryMove(nx, ny, length);
            }
        }
        for (int dir = 4; dir < 8 && !done; dir++)
        {
            int nx = x + dx[dir];
            int ny = y + dy[dir];
            TryMove(nx, ny, length);
        }

        visited[y][x] = false;
    }

    static void TryMove(int nx, int ny, int length)
    {
        if (done) return;
        if (InBounds(nx, ny) && !visited[ny][nx] && grid[ny][nx] == '#')
            DFS(nx, ny, length + 1);
    }

    static bool InBounds(int x, int y)
    {
        return x >= 0 && x < N && y >= 0 && y < N;
    }
}