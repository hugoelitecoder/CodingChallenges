using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var wh = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int w = wh[0], h = wh[1];

        var grid = new int[h, w];
        var values = new List<(int r, int c, int val)>();

        for (int i = 0; i < h; i++)
        {
            var line = Console.ReadLine().Split().Select(int.Parse).ToArray();
            for (int j = 0; j < w; j++)
            {
                grid[i, j] = line[j];
                if (grid[i, j] > 0)
                    values.Add((i, j, grid[i, j]));
            }
        }

        foreach (var (r, c, val) in values)
        {
            var rectangles = new List<(int r, int c, int w, int h)>();

            for (int height = 1; height <= val; height++)
            {
                if (val % height != 0) continue;
                int width = val / height;

                for (int i = 0; i <= h - height; i++)
                {
                    for (int j = 0; j <= w - width; j++)
                    {
                        if (!(i <= r && r < i + height && j <= c && c < j + width)) continue;

                        bool valid = true;
                        for (int k = 0; k < height * width && valid; k++)
                        {
                            int y = i + k / width;
                            int x = j + k % width;
                            if (y == r && x == c) continue;
                            if (grid[y, x] > 0) valid = false;
                        }

                        if (valid) rectangles.Add((i, j, width, height));
                    }
                }
            }

            if (rectangles.Count > 0)
            {
                Console.WriteLine($"{r} {c} {val}");
                foreach (var rect in rectangles.OrderBy(x => x.r).ThenBy(x => x.c).ThenBy(x => x.w))
                    Console.WriteLine($"{rect.r} {rect.c} {rect.w} {rect.h}");
            }
        }
    }
}
