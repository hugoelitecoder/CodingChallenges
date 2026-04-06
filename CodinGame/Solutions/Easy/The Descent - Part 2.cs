using System;

class Solution
{
    public static void Main(string[] args)
    {
        var parts = Console.ReadLine().Split(' ');
        var w = int.Parse(parts[0]);
        var h = int.Parse(parts[1]);
        var grid = new int[h][];
        var y = 0;

        while (y < h)
        {
            parts = Console.ReadLine().Split(' ');
            grid[y] = new int[w];
            var x = 0;
            while (x < w)
            {
                grid[y][x] = int.Parse(parts[x]);
                x++;
            }
            y++;
        }

        parts = Console.ReadLine().Split(' ');
        var a = int.Parse(parts[0]);
        var b = int.Parse(parts[1]);
        var t = int.Parse(Console.ReadLine());

        var result = StarshipLandingSolver.Solve(grid, w, h, a, b);

        Console.Error.WriteLine("[DEBUG] W=" + w);
        Console.Error.WriteLine("[DEBUG] H=" + h);
        Console.Error.WriteLine("[DEBUG] A=" + a);
        Console.Error.WriteLine("[DEBUG] B=" + b);
        Console.Error.WriteLine("[DEBUG] T=" + t);
        Console.Error.WriteLine("[DEBUG] BestShots=" + result);

        if (result <= t)
        {
            Console.WriteLine(result);
        }
        else
        {
            Console.WriteLine("Not Possible");
        }
    }
}

static class StarshipLandingSolver
{
    public static int Solve(int[][] grid, int w, int h, int a, int b)
    {
        var prefix = BuildPrefixSums(grid, w, h);
        var best = int.MaxValue;

        if (a <= w && b <= h)
        {
            var shots = GetBestForSize(grid, prefix, w, h, a, b);
            if (shots < best)
            {
                best = shots;
            }
        }

        if (a != b && b <= w && a <= h)
        {
            var shots = GetBestForSize(grid, prefix, w, h, b, a);
            if (shots < best)
            {
                best = shots;
            }
        }

        return best;
    }

    private static int[,] BuildPrefixSums(int[][] grid, int w, int h)
    {
        var prefix = new int[h + 1, w + 1];
        var y = 0;

        while (y < h)
        {
            var x = 0;
            while (x < w)
            {
                prefix[y + 1, x + 1] = prefix[y, x + 1] + prefix[y + 1, x] - prefix[y, x] + grid[y][x];
                x++;
            }
            y++;
        }

        return prefix;
    }

    private static int GetBestForSize(int[][] grid, int[,] prefix, int w, int h, int rectW, int rectH)
    {
        var best = int.MaxValue;
        var y = 0;

        while (y + rectH <= h)
        {
            var x = 0;
            while (x + rectW <= w)
            {
                var sum = GetRectangleSum(prefix, x, y, rectW, rectH);
                var min = GetRectangleMin(grid, x, y, rectW, rectH);
                var shots = sum - rectW * rectH * min;

                if (shots < best)
                {
                    best = shots;
                }

                x++;
            }
            y++;
        }

        return best;
    }

    private static int GetRectangleSum(int[,] prefix, int x, int y, int rectW, int rectH)
    {
        var x2 = x + rectW;
        var y2 = y + rectH;
        return prefix[y2, x2] - prefix[y, x2] - prefix[y2, x] + prefix[y, x];
    }

    private static int GetRectangleMin(int[][] grid, int x, int y, int rectW, int rectH)
    {
        var min = int.MaxValue;
        var yy = y;

        while (yy < y + rectH)
        {
            var xx = x;
            while (xx < x + rectW)
            {
                var v = grid[yy][xx];
                if (v < min)
                {
                    min = v;
                }
                xx++;
            }
            yy++;
        }

        return min;
    }
}