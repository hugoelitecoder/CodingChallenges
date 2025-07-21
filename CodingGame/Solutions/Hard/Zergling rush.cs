using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var W = int.Parse(inputs[0]);
        var H = int.Parse(inputs[1]);
        var grid = new char[H][];
        for (var i = 0; i < H; i++)
        {
            var row = Console.ReadLine();
            grid[i] = row.ToCharArray();
        }

        var isReachable = FindReachableCells(W, H, grid);
        var reachableBuildings = FindReachableBuildings(W, H, grid, isReachable);

        if (reachableBuildings.Count == 0)
        {
            PrintGrid(H, grid);
            return;
        }

        var resultGrid = new char[H][];
        for (var i = 0; i < H; i++)
        {
            resultGrid[i] = (char[])grid[i].Clone();
        }
        
        PlaceZerglings(W, H, resultGrid, isReachable, reachableBuildings);
        
        PrintGrid(H, resultGrid);
    }

    private readonly record struct Point(int R, int C);
    private static void PrintGrid(int h, char[][] grid)
    {
        for (var i = 0; i < h; i++)
        {
            Console.WriteLine(new string(grid[i]));
        }
    }

    private static bool[][] FindReachableCells(int w, int h, char[][] grid)
    {
        var isReachable = new bool[h][];
        for (var i = 0; i < h; i++)
        {
            isReachable[i] = new bool[w];
        }
        var queue = new Queue<Point>();
        for (var r = 0; r < h; r++)
        {
            for (var c = 0; c < w; c++)
            {
                var isOnBorder = r == 0 || r == h - 1 || c == 0 || c == w - 1;
                if (isOnBorder && grid[r][c] == '.')
                {
                    if (!isReachable[r][c])
                    {
                        queue.Enqueue(new Point(r, c));
                        isReachable[r][c] = true;
                    }
                }
            }
        }

        var dr = new int[] { -1, 1, 0, 0 };
        var dc = new int[] { 0, 0, -1, 1 };
        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            for (var i = 0; i < 4; i++)
            {
                var nr = p.R + dr[i];
                var nc = p.C + dc[i];

                var isInBounds = nr >= 0 && nr < h && nc >= 0 && nc < w;
                if (isInBounds && !isReachable[nr][nc] && grid[nr][nc] == '.')
                {
                    isReachable[nr][nc] = true;
                    queue.Enqueue(new Point(nr, nc));
                }
            }
        }
        return isReachable;
    }

    private static List<Point> FindReachableBuildings(int w, int h, char[][] grid, bool[][] isReachable)
    {
        var buildings = new List<Point>();
        for (var r = 0; r < h; r++)
        {
            for (var c = 0; c < w; c++)
            {
                if (grid[r][c] == 'B')
                {
                    var foundReachableNeighbor = false;
                    for (var dr = -1; dr <= 1; dr++)
                    {
                        for (var dc = -1; dc <= 1; dc++)
                        {
                            if (dr == 0 && dc == 0) continue;
                            
                            var nr = r + dr;
                            var nc = c + dc;

                            var isInBounds = nr >= 0 && nr < h && nc >= 0 && nc < w;
                            if (isInBounds && isReachable[nr][nc])
                            {
                                buildings.Add(new Point(r, c));
                                foundReachableNeighbor = true;
                                break;
                            }
                        }
                        if (foundReachableNeighbor) break;
                    }
                }
            }
        }
        return buildings;
    }

    private static void PlaceZerglings(int w, int h, char[][] resultGrid, bool[][] isReachable, List<Point> buildings)
    {
        foreach (var building in buildings)
        {
            for (var dr = -1; dr <= 1; dr++)
            {
                for (var dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;

                    var nr = building.R + dr;
                    var nc = building.C + dc;

                    var isInBounds = nr >= 0 && nr < h && nc >= 0 && nc < w;
                    if (isInBounds && resultGrid[nr][nc] == '.' && isReachable[nr][nc])
                    {
                        resultGrid[nr][nc] = 'z';
                    }
                }
            }
        }
    }
}