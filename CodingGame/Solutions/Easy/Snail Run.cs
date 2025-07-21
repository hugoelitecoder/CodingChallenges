using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int numberSnails = int.Parse(Console.ReadLine());
        var speeds = new int[numberSnails + 1];

        for (int i = 1; i <= numberSnails; i++)
            speeds[i] = int.Parse(Console.ReadLine());

        int height = int.Parse(Console.ReadLine());
        int width = int.Parse(Console.ReadLine());

        var map = new char[height, width];
        var starts = new Dictionary<int, (int y, int x)>();
        (int y, int x) dest = (-1, -1);

        for (int y = 0; y < height; y++)
        {
            var row = Console.ReadLine();
            for (int x = 0; x < width; x++)
            {
                map[y, x] = row[x];
                if (char.IsDigit(map[y, x]))
                {
                    int id = map[y, x] - '0';
                    starts[id] = (y, x);
                }
                else if (map[y, x] == '#')
                {
                    dest = (y, x);
                }
            }
        }

        var dirs = new[] { (0, 1), (1, 0), (0, -1), (-1, 0) };

        int winner = -1;
        double bestTime = double.MaxValue;

        foreach (var kvp in starts)
        {
            int id = kvp.Key;
            var (sy, sx) = kvp.Value;
            var visited = new bool[height, width];
            var queue = new Queue<((int y, int x), int steps)>();
            queue.Enqueue(((sy, sx), 0));
            visited[sy, sx] = true;

            while (queue.Count > 0)
            {
                var ((y, x), steps) = queue.Dequeue();

                if (y == dest.y && x == dest.x)
                {
                    double time = (double)steps / speeds[id];
                    if (time < bestTime)
                    {
                        bestTime = time;
                        winner = id;
                    }
                    break;
                }

                foreach (var (dy, dx) in dirs)
                {
                    int ny = y + dy, nx = x + dx;
                    if (ny >= 0 && ny < height && nx >= 0 && nx < width && !visited[ny, nx])
                    {
                        char c = map[ny, nx];
                        if (c == '*' || c == '#')
                        {
                            visited[ny, nx] = true;
                            queue.Enqueue(((ny, nx), steps + 1));
                        }
                    }
                }
            }
        }

        Console.WriteLine(winner);
    }
}
