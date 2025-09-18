using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var ab = Console.ReadLine().Split(' ');
        int a = int.Parse(ab[0]), b = int.Parse(ab[1]);

        var obj = new char[a][];
        for (int i = 0; i < a; i++)
            obj[i] = Console.ReadLine().ToCharArray();

        var cd = Console.ReadLine().Split(' ');
        int c = int.Parse(cd[0]), d = int.Parse(cd[1]);

        var grid = new char[c][];
        for (int i = 0; i < c; i++)
            grid[i] = Console.ReadLine().ToCharArray();

        int count = 0;
        char[][] resultGrid = null;

        for (int i = 0; i <= c - a; i++)
        {
            for (int j = 0; j <= d - b; j++)
            {
                if (CanPlace(obj, grid, i, j, a, b))
                {
                    count++;
                    if (count == 1)
                        resultGrid = PlaceObject(obj, grid, i, j, a, b);
                }
            }
        }

        Console.WriteLine(count);
        if (count == 1 && resultGrid != null)
        {
            foreach (var row in resultGrid)
                Console.WriteLine(new string(row));
        }
    }

    static bool CanPlace(char[][] obj, char[][] grid, int x, int y, int a, int b)
    {
        for (int i = 0; i < a; i++)
            for (int j = 0; j < b; j++)
                if (obj[i][j] == '*' && grid[x + i][y + j] != '.')
                    return false;
        return true;
    }

    static char[][] PlaceObject(char[][] obj, char[][] grid, int x, int y, int a, int b)
    {
        var newGrid = new char[grid.Length][];
        for (int i = 0; i < grid.Length; i++)
            newGrid[i] = (char[])grid[i].Clone();

        for (int i = 0; i < a; i++)
            for (int j = 0; j < b; j++)
                if (obj[i][j] == '*')
                    newGrid[x + i][y + j] = '*';

        return newGrid;
    }
}
