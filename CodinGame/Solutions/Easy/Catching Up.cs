using System;
using System.Collections.Generic;

class Player
{
    const int N = 10, M = 10;
    static char[,] grid = new char[N, M];
    static int playerY, playerX;
    static bool hasWall = false;

    public static void Main(string[] args)
    {
        int K = int.Parse(Console.ReadLine());
        for (int y = 0; y < N; y++)
        {
            var line = Console.ReadLine();
            for (int x = 0; x < M; x++)
            {
                grid[y, x] = line[x];
                if (grid[y, x] == '*')
                    hasWall = true;
                else if (grid[y, x] == 'P')
                {
                    playerY = y;
                    playerX = x;
                    grid[y, x] = '-';
                }
            }
        }

        while (true)
        {
            var inputs = Console.ReadLine().Split();
            int eneY = int.Parse(inputs[0]);
            int eneX = int.Parse(inputs[1]);

            char move;
            if (!hasWall)
                move = GreedyMove(eneY, eneX);
            else
                move = BFSChase(eneY, eneX);

            Console.WriteLine(move);
            switch (move)
            {
                case 'U': playerY--; break;
                case 'D': playerY++; break;
                case 'L': playerX--; break;
                case 'R': playerX++; break;
            }
        }
    }

    static char GreedyMove(int ty, int tx)
    {
        var dy = ty - playerY;
        var dx = tx - playerX;
        if (Math.Abs(dx) > Math.Abs(dy))
            return dx > 0 ? 'R' : 'L';
        else
            return dy > 0 ? 'D' : 'U';
    }

    static char BFSChase(int ty, int tx)
    {
        var visited = new bool[N, M];
        var q = new Queue<Node>();
        int[] dy = { -1, 1, 0, 0 };
        int[] dx = { 0, 0, -1, 1 };
        char[] dch = { 'U', 'D', 'L', 'R' };

        visited[playerY, playerX] = true;
        q.Enqueue(new Node { Y = playerY, X = playerX, FirstMove = '\0' });

        while (q.Count > 0)
        {
            var u = q.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                int ny = u.Y + dy[i], nx = u.X + dx[i];
                if (!InBounds(ny, nx) || visited[ny, nx]) continue;
                if (grid[ny, nx] == '*') continue;
                if (ny == ty && nx == tx) continue;

                visited[ny, nx] = true;
                var first = u.FirstMove != '\0' ? u.FirstMove : dch[i];

                if (Math.Abs(ny - ty) + Math.Abs(nx - tx) == 1)
                    return first;

                q.Enqueue(new Node { Y = ny, X = nx, FirstMove = first });
            }
        }

        return 'U';
    }

    static bool InBounds(int y, int x)
        => y >= 0 && y < N && x >= 0 && x < M;

    class Node
    {
        public int Y;
        public int X;
        public char FirstMove;
    }
}
