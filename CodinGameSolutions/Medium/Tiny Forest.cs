using System;
using System.Collections.Generic;

class Solution
{
    static int W, H;
    static bool[,] initTree;
    
    static void Main()
    {
        W = int.Parse(Console.ReadLine());
        H = int.Parse(Console.ReadLine());
        initTree = new bool[H, W];
        for (int y = 0; y < H; y++)
        {
            var line = Console.ReadLine();
            for (int x = 0; x < W; x++)
                initTree[y, x] = (line[x] == 'Y');
        }
        
        int best = 0;
        for (int sy = 0; sy < H; sy++)
        for (int sx = 0; sx < W; sx++)
            if (!initTree[sy, sx])
                best = Math.Max(best, SimulateTrees(sx, sy, 33));
        
        Console.WriteLine(best);
    }
    
    static int SimulateTrees(int sx, int sy, int years)
    {
        var seedAge = new int[H, W];
        var treeAge = new int[H, W];
        for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
        {
            seedAge[y, x] = -1;
            if (initTree[y, x])
                treeAge[y, x] = 1;
            else
                treeAge[y, x] = -1;
        }
        seedAge[sy, sx] = 0;
        int[] DX = {1, -1, 0, 0};
        int[] DY = {0, 0, 1, -1};
        
        for (int year = 1; year <= years; year++)
        {
            for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                if (seedAge[y, x] >= 0)
                {
                    seedAge[y, x]++;
                    if (seedAge[y, x] >= 10)
                    {
                        seedAge[y, x] = -1;
                        treeAge[y, x] = 0;
                    }
                }
            }
            
            var willSeed = new List<(int,int)>();
            for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                if (treeAge[y, x] >= 1)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + DX[d], ny = y + DY[d];
                        if (nx >= 0 && nx < W && ny >= 0 && ny < H)
                        {
                            if (treeAge[ny, nx] < 0 && seedAge[ny, nx] < 0)
                                willSeed.Add((ny, nx));
                        }
                    }
                }
            }
            foreach (var (ny,nx) in willSeed)
                seedAge[ny, nx] = 0;
            
            for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
                if (treeAge[y, x] >= 0)
                    treeAge[y, x]++;
        }
        
        int cnt = 0;
        for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
            if (treeAge[y, x] >= 0)
                cnt++;
        return cnt;
    }
}
