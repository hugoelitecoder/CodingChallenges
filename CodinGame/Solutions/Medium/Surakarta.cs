using System;

class Solution {
    const int N = 6, L = N - 1, M = L / 2, S = N * N - 1;
    static readonly (int dY, int dX)[] Dirs = { (-1, 0), (1, 0), (0, -1), (0, 1) };
    static char[][] g = new char[N][];

    static void Main() {
        for (int i = 0; i < N; i++)
            g[i] = Console.ReadLine().ToCharArray();

        int result = 0;
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
                if (g[y][x] == 'X') {
                    g[y][x] = '.';
                    foreach (var (dY, dX) in Dirs)
                        result += DFS(y + dY, x + dX, dY, dX, 0, false);
                    g[y][x] = 'X';
                }
        Console.WriteLine(result);
    }
    
    static int DFS(int y, int x, int dY, int dX, int steps, bool canCapture) {
        if (!In(y, x)) {
            canCapture = true;
            (y, x, dY, dX) = Wrap(y, x, dY, dX);
        }
        if (In(y, x) && canCapture && g[y][x] == 'O') return 1;
        if (!In(y, x) || g[y][x] != '.' || steps > S || Corner(y, x)) return 0;
        return DFS(y + dY, x + dX, dY, dX, steps + 1, canCapture);
    }

    static bool In(int y, int x) => y >= 0 && y <= L && x >= 0 && x <= L;
    static bool Corner(int y, int x) => (y == 0 || y == L) && (x == 0 || x == L);
    static (int y, int x, int dY, int dX) Wrap(int y, int x, int dY, int dX) {
        int oY = y, oX = x, region;
        if (x > L) {
            region = oY > M ? 1 : 0;
            int delta = region == 1 ? -1 : 1;
            return (region == 1 ? L : 0, region == 1 ? oY : -oY + L, delta, 0);
        }
        if (y > L) {
            region = oX > M ? 1 : 0;
            int delta = region == 1 ? -1 : 1;
            return (region == 1 ? oX : -oX + L, region == 1 ? L : 0, 0, delta);
        }
        if (x < 0) {
            region = oY > M ? 1 : 0;
            int delta = region == 1 ? -1 : 1;
            return (region == 1 ? L : 0, region == 1 ? -oY + L : oY, delta, 0);
        }
        region = oX > M ? 1 : 0;
        int d = region == 1 ? -1 : 1;
        return (region == 1 ? -oX + L : oX, region == 1 ? L : 0, 0, d);
    }
   
}