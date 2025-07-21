using System;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var w = int.Parse(inputs[0]);
        var h = int.Parse(inputs[1]);
        var grid = new int[h, w];
        for (var i = 0; i < h; i++)
        {
            var line = Console.ReadLine().Split(' ');
            for (var j = 0; j < w; j++)
            {
                grid[i, j] = int.Parse(line[j]);
            }
        }
        var solver = new MaxSubRectangleSolver(w, h, grid);
        var result = solver.ComputeLargestSum();
        Console.WriteLine(result);
    }
}

class MaxSubRectangleSolver
{
    private readonly int _width;
    private readonly int _height;
    private readonly int[,] _grid;

    public MaxSubRectangleSolver(int w, int h, int[,] grid)
    {
        _width = w;
        _height = h;
        _grid = grid;
    }

    public int ComputeLargestSum()
    {
        var maxOverallSum = int.MinValue;
        for (var leftCol = 0; leftCol < _width; leftCol++)
        {
            var rowSums = new int[_height];
            for (var rightCol = leftCol; rightCol < _width; rightCol++)
            {
                for (var i = 0; i < _height; i++)
                {
                    rowSums[i] += _grid[i, rightCol];
                }
                var currentStripMaxSum = Kadane(rowSums);
                if (currentStripMaxSum > maxOverallSum)
                {
                    maxOverallSum = currentStripMaxSum;
                }
            }
        }
        return maxOverallSum;
    }

    private int Kadane(int[] arr)
    {
        var maxGlobal = arr[0];
        var maxCurrent = arr[0];
        for (var i = 1; i < arr.Length; i++)
        {
            maxCurrent = Math.Max(arr[i], maxCurrent + arr[i]);
            maxGlobal = Math.Max(maxGlobal, maxCurrent);
        }
        return maxGlobal;
    }
}

