using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var inputs = Console.ReadLine().Split();
        int HI = int.Parse(inputs[0]);
        int WI = int.Parse(inputs[1]);
        string style = Console.ReadLine();
        int triangles = int.Parse(Console.ReadLine());

        var grid = new bool[HI, WI]; // true = star, false = space
        for (int y = 0; y < HI; y++)
            for (int x = 0; x < WI; x++)
                grid[y, x] = true;

        for (int i = 0; i < triangles; i++)
        {
            var t = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
            ToggleTriangle(grid, t[0], t[1], t[2], t[3], t[4], t[5]);
        }

        PrintGrid(grid, style);
    }

    static void ToggleTriangle(bool[,] grid, int x1, int y1, int x2, int y2, int x3, int y3)
    {
        int h = grid.GetLength(0), w = grid.GetLength(1);

        int minX = Math.Max(0, Math.Min(x1, Math.Min(x2, x3)));
        int maxX = Math.Min(w - 1, Math.Max(x1, Math.Max(x2, x3)));
        int minY = Math.Max(0, Math.Min(y1, Math.Min(y2, y3)));
        int maxY = Math.Min(h - 1, Math.Max(y1, Math.Max(y2, y3)));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (PointInTriangle(x, y, x1, y1, x2, y2, x3, y3))
                    grid[y, x] = !grid[y, x]; // toggle
            }
        }
    }

    static bool PointInTriangle(int px, int py, int x1, int y1, int x2, int y2, int x3, int y3)
    {
        int Area(int xA, int yA, int xB, int yB, int xC, int yC)
        {
            return Math.Abs((xA*(yB - yC) + xB*(yC - yA) + xC*(yA - yB)));
        }

        int full = Area(x1, y1, x2, y2, x3, y3);
        int a1 = Area(px, py, x2, y2, x3, y3);
        int a2 = Area(x1, y1, px, py, x3, y3);
        int a3 = Area(x1, y1, x2, y2, px, py);

        return full == a1 + a2 + a3;
    }


    static void PrintGrid(bool[,] grid, string style)
    {
        int h = grid.GetLength(0), w = grid.GetLength(1);
        for (int y = 0; y < h; y++)
        {
            var row = new List<char>();
            for (int x = 0; x < w; x++)
            {
                row.Add(grid[y, x] ? '*' : ' ');
                if (style == "expanded" && x != w - 1)
                    row.Add(' ');
            }
            Console.WriteLine(new string(row.ToArray()));
        }
    }
}
