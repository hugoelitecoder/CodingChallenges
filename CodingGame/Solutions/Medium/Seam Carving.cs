using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        Console.ReadLine();
        var wh = Console.ReadLine().Split();
        int W = int.Parse(wh[0]), H = int.Parse(wh[1]);
        var comment = Console.ReadLine().Split();
        int targetWidth = int.Parse(comment[1]);
        Console.ReadLine();
        
        var image = new int[H][];
        for (int y = 0; y < H; y++)
        {
            image[y] = Console.ReadLine()
                              .Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries)
                              .Select(int.Parse).ToArray();
        }

        int seamsToRemove = W - targetWidth;
        for (int i = 0; i < seamsToRemove; i++)
        {
            var energy = ComputeEnergy(image);
            var (seamEnergy, seamPath) = FindMinimalVerticalSeam(energy);
            Console.WriteLine(seamEnergy);
            image = RemoveSeam(image, seamPath);
        }
    }

    static int[][] ComputeEnergy(int[][] img)
    {
        int H = img.Length, W = img[0].Length;
        var E = new int[H][];
        for (int y = 0; y < H; y++)
        {
            E[y] = new int[W];
            for (int x = 0; x < W; x++)
            {
                int dx = 0, dy = 0;
                if (x > 0 && x < W - 1)
                    dx = img[y][x+1] - img[y][x-1];
                if (y > 0 && y < H - 1)
                    dy = img[y+1][x] - img[y-1][x];
                E[y][x] = Math.Abs(dx) + Math.Abs(dy);
            }
        }
        return E;
    }

    static (int, int[]) FindMinimalVerticalSeam(int[][] energy)
    {
        int H = energy.Length, W = energy[0].Length;
        var dp = new int[H][];
        var parent = new int[H][];

        dp[0] = new int[W];
        parent[0] = Enumerable.Repeat(-1, W).ToArray();
        for (int x = 0; x < W; x++)
            dp[0][x] = energy[0][x];

        for (int y = 1; y < H; y++)
        {
            dp[y] = new int[W];
            parent[y] = new int[W];
            for (int x = 0; x < W; x++)
            {
                int bestVal = int.MaxValue, bestX = -1;
                for (int dx = -1; dx <= 1; dx++)
                {
                    int px = x + dx;
                    if (px < 0 || px >= W) continue;
                    int val = dp[y-1][px];
                    if (val < bestVal || (val == bestVal && px < bestX))
                    {
                        bestVal = val;
                        bestX = px;
                    }
                }
                dp[y][x] = bestVal + energy[y][x];
                parent[y][x] = bestX;
            }
        }
        int minTotal = int.MaxValue, endX = -1;
        for (int x = 0; x < W; x++)
        {
            int v = dp[H-1][x];
            if (v < minTotal || (v == minTotal && x < endX))
            {
                minTotal = v;
                endX = x;
            }
        }
        var seam = new int[H];
        int cx = endX;
        for (int y = H-1; y >= 0; y--)
        {
            seam[y] = cx;
            cx = parent[y][cx];
        }

        return (minTotal, seam);
    }

    static int[][] RemoveSeam(int[][] img, int[] seam)
    {
        int H = img.Length, W = img[0].Length;
        var result = new int[H][];
        for (int y = 0; y < H; y++)
        {
            result[y] = new int[W-1];
            int sx = seam[y];
            Array.Copy(img[y], 0, result[y], 0, sx);
            Array.Copy(img[y], sx+1, result[y], sx, W - sx - 1);
        }
        return result;
    }
}
