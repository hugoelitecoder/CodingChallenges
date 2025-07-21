using System;

class Solution
{
    public static void Main()
    {
        var nExp = int.Parse(Console.ReadLine());
        var parts = Console.ReadLine().Split();
        var x = int.Parse(parts[0]);
        var y = int.Parse(parts[1]);
        var size = 1 << nExp;
        var grid = new int[size, size];
        for (var i = 0; i < size; i++)
        {
            for (var j = 0; j < size; j++)
            {
                grid[i, j] = 9;
            }
        }
        grid[y, x] = 0;
        SolveGrid(grid, nExp, 0, 0, x, y);
        PrintGrid(grid);
    }

    public static void PrintGrid(int[,] grid)
    {
        var size = grid.GetLength(0);
        var header = "+";
        for (var j = 0; j < size; j++)
        {
            header += "--+";
        }
        Console.WriteLine(header);
        for (var i = 0; i < size; i++)
        {
            var line1 = "|";
            for (var j = 0; j < size; j++)
            {
                var v = grid[i, j];
                if (v == 0)
                {
                    line1 += "##|";
                }
                else if (v == 1 || v == 5 || v == 6
                         || (j < size - 1 && (grid[i, j + 1] == 3 || grid[i, j + 1] == 7 || grid[i, j + 1] == 8)))
                {
                    line1 += "   ";
                }
                else
                {
                    line1 += "  |";
                }
            }
            Console.WriteLine(line1);

            var line2 = "+";
            for (var j = 0; j < size; j++)
            {
                var v = grid[i, j];
                if (v == 2 || v == 6 || v == 7
                    || (i < size - 1 && (grid[i + 1, j] == 4 || grid[i + 1, j] == 5 || grid[i + 1, j] == 8)))
                {
                    line2 += "  +";
                }
                else
                {
                    line2 += "--+";
                }
            }
            Console.WriteLine(line2);
        }
    }

    private static void SolveGrid(int[,] grid, int nExp, int coinX, int coinY, int holeX, int holeY)
    {
        if (nExp == 1)
        {
            PlaceTriomino(grid, coinX, coinY, holeX, holeY);
            return;
        }

        var half = 1 << (nExp - 1);
        var xc = coinX;
        var yc = coinY;
        var xt = holeX;
        var yt = holeY;
        var quadWithHole = 0;
        int localHX, localHY;

        // Top-Left quadrant
        if (xt < half && yt < half)
        {
            quadWithHole = 1;
            localHX = xt;
            localHY = yt;
        }
        else
        {
            localHX = half - 1;
            localHY = half - 1;
        }
        SolveGrid(grid, nExp - 1, xc, yc, localHX, localHY);

        // Top-Right quadrant
        if (xt >= half && yt < half)
        {
            quadWithHole = 2;
            localHX = xt - half;
            localHY = yt;
        }
        else
        {
            localHX = 0;
            localHY = half - 1;
        }
        SolveGrid(grid, nExp - 1, xc + half, yc, localHX, localHY);

        // Bottom-Left quadrant
        if (xt < half && yt >= half)
        {
            quadWithHole = 3;
            localHX = xt;
            localHY = yt - half;
        }
        else
        {
            localHX = half - 1;
            localHY = 0;
        }
        SolveGrid(grid, nExp - 1, xc, yc + half, localHX, localHY);

        // Bottom-Right quadrant
        if (xt >= half && yt >= half)
        {
            quadWithHole = 4;
            localHX = xt - half;
            localHY = yt - half;
        }
        else
        {
            localHX = 0;
            localHY = 0;
        }
        SolveGrid(grid, nExp - 1, xc + half, yc + half, localHX, localHY);

        // Place center tromino
        var centerOriginX = xc + half - 1;
        var centerOriginY = yc + half - 1;
        var holeRelX = (quadWithHole == 2 || quadWithHole == 4) ? 1 : 0;
        var holeRelY = (quadWithHole == 3 || quadWithHole == 4) ? 1 : 0;
        PlaceTriomino(grid, centerOriginX, centerOriginY, holeRelX, holeRelY);
    }

    private static void PlaceTriomino(int[,] grid, int coinX, int coinY, int holeX, int holeY)
    {
        if (holeX == 0 && holeY == 0)
        {
            grid[coinY + 0, coinX + 1] = 2;
            grid[coinY + 1, coinX + 0] = 1;
            grid[coinY + 1, coinX + 1] = 8;
        }
        else if (holeX == 1 && holeY == 0)
        {
            grid[coinY + 0, coinX + 0] = 2;
            grid[coinY + 1, coinX + 0] = 5;
            grid[coinY + 1, coinX + 1] = 3;
        }
        else if (holeX == 0 && holeY == 1)
        {
            grid[coinY + 0, coinX + 0] = 1;
            grid[coinY + 0, coinX + 1] = 7;
            grid[coinY + 1, coinX + 1] = 4;
        }
        else
        {
            grid[coinY + 0, coinX + 0] = 6;
            grid[coinY + 0, coinX + 1] = 3;
            grid[coinY + 1, coinX + 0] = 4;
        }
    }
}
