using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var inputs = Console.ReadLine().Split();
        int w = int.Parse(inputs[0]), h = int.Parse(inputs[1]);
        long n = long.Parse(Console.ReadLine());

        char[,] map = new char[h, w];
        int x = 0, y = 0;

        for (int i = 0; i < h; i++)
        {
            var line = Console.ReadLine();
            for (int j = 0; j < w; j++)
            {
                map[i, j] = line[j];
                if (line[j] == 'O') { x = j; y = i; }
            }
        }

        var dx = new[] { 0, 1, 0, -1 }; // up, right, down, left
        var dy = new[] { -1, 0, 1, 0 };
        int dir = 0;

        var seen = new Dictionary<(int, int, int), long>();
        var steps = new List<(int x, int y, int dir)>();

        long step = 0;
        while (!seen.ContainsKey((x, y, dir)))
        {
            seen[(x, y, dir)] = step;
            steps.Add((x, y, dir));

            while (true)
            {
                int nx = x + dx[dir], ny = y + dy[dir];
                if (nx >= 0 && nx < w && ny >= 0 && ny < h && map[ny, nx] != '#')
                {
                    x = nx; y = ny;
                    break;
                }
                dir = (dir + 1) % 4;
            }

            step++;
            if (step == n)
            {
                Console.WriteLine($"{x} {y}");
                return;
            }
        }

        // Found cycle
        long cycleStart = seen[(x, y, dir)];
        long cycleLen = step - cycleStart;
        long remaining = (n - cycleStart) % cycleLen;
        var finalStep = cycleStart + remaining;

        var (fx, fy, _) = steps[(int)finalStep];
        Console.WriteLine($"{fx} {fy}");
    }
}
