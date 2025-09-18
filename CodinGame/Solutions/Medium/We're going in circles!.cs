using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        var dims    = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int w       = dims[0], h = dims[1];
        int W       = w + 2, H = h + 2;
        var grid    = Enumerable.Range(0, H)
                    .Select(y => y == 0 || y == H - 1
                        ? Enumerable.Repeat('*', W).ToArray()
                        : ("*" + Console.ReadLine() + "*").ToCharArray()
                    )
                    .ToArray();

        var deltas = new Dictionary<char, int>
        {
            ['^'] = -W,
            ['>'] =  1,
            ['v'] = +W,
            ['<'] = -1
        };

        var indices = new HashSet<int>(
            from y in Enumerable.Range(1, h)
            from x in Enumerable.Range(1, w)
            where grid[y][x] != '.'
            select y * W + x
        );

        int loops = 0;
        var visited = new HashSet<int>();
        while (indices.Count > 0)
        {
            int pos = indices.First();
            indices.Remove(pos);
            visited.Clear();
            visited.Add(pos);

            int delta = deltas[grid[pos / W][pos % W]];

            int cur = pos;
            while (true)
            {
                cur += delta;
                char c = grid[cur / W][cur % W];
                if (c == '*') break;
                if (visited.Contains(cur))
                {
                    loops++;
                    break;
                }
                if (c != '.')
                {
                    visited.Add(cur);
                    delta = deltas[c];
                    if (!indices.Remove(cur)) break;
                }
            }
        }

        Console.WriteLine(loops);
    }
}
