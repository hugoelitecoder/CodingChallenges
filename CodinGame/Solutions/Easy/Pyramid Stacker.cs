using System;
using System.Diagnostics;
using System.Text;

class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();

        var input = ReadInput();
        Console.Error.WriteLine("[DEBUG] N=" + input.CubeCount + " H=" + input.Height);
        Console.Error.WriteLine("[DEBUG] Cubes=" + input.CubeLabels);

        var frontViewLines = PyramidFrontViewSolver.Solve(input.CubeCount, input.Height, input.CubeLabels);

        WriteOutput(frontViewLines);

        sw.Stop();
        Console.Error.WriteLine("[DEBUG] ElapsedMs=" + sw.ElapsedMilliseconds);
    }

    private static PyramidInput ReadInput()
    {
        var parts = Console.ReadLine().Split(' ');
        var cubeCount = int.Parse(parts[0]);
        var height = int.Parse(parts[1]);
        var cubeLabels = Console.ReadLine();
        return new PyramidInput(cubeCount, height, cubeLabels);
    }

    private static void WriteOutput(string[] lines)
    {
        var i = 0;
        while (i < lines.Length)
        {
            Console.WriteLine(lines[i]);
            i++;
        }
    }
}

readonly struct PyramidInput
{
    public readonly int CubeCount;
    public readonly int Height;
    public readonly string CubeLabels;

    public PyramidInput(int cubeCount, int height, string cubeLabels)
    {
        CubeCount = cubeCount;
        Height = height;
        CubeLabels = cubeLabels;
    }
}

static class PyramidFrontViewSolver
{
    public static string[] Solve(int cubeCount, int height, string cubeLabels)
    {
        var lines = new string[height];
        var labelIndex = 0;
        var layerSize = height;

        while (layerSize >= 1)
        {
            var placedCubeCount = GetPlacedCubeCountForLayer(cubeCount, labelIndex, layerSize);
            var layerGrid = BuildLayerGrid(layerSize, cubeLabels, labelIndex, placedCubeCount);
            var visibleFrontCubes = GetVisibleFrontCubes(layerGrid, layerSize);
            lines[layerSize - 1] = FormatFrontViewLine(visibleFrontCubes, height, layerSize);
            labelIndex += placedCubeCount;
            layerSize--;
        }

        return lines;
    }

    private static int GetPlacedCubeCountForLayer(int cubeCount, int labelIndex, int layerSize)
    {
        var remainingCubeCount = cubeCount - labelIndex;
        if (remainingCubeCount <= 0)
        {
            return 0;
        }

        var layerCapacity = layerSize * layerSize;
        return remainingCubeCount < layerCapacity ? remainingCubeCount : layerCapacity;
    }

    private static char[,] BuildLayerGrid(int layerSize, string cubeLabels, int labelIndex, int placedCubeCount)
    {
        var layerGrid = new char[layerSize, layerSize];
        var position = 0;

        while (position < placedCubeCount)
        {
            var row = position / layerSize;
            var column = position % layerSize;
            layerGrid[row, column] = cubeLabels[labelIndex + position];
            position++;
        }

        return layerGrid;
    }

    private static char[] GetVisibleFrontCubes(char[,] layerGrid, int layerSize)
    {
        var visibleFrontCubes = new char[layerSize];
        var column = 0;

        while (column < layerSize)
        {
            visibleFrontCubes[column] = GetVisibleFrontCubeAtColumn(layerGrid, layerSize, column);
            column++;
        }

        return visibleFrontCubes;
    }

    private static char GetVisibleFrontCubeAtColumn(char[,] layerGrid, int layerSize, int column)
    {
        var row = layerSize - 1;
        while (row >= 0)
        {
            var cubeLabel = layerGrid[row, column];
            if (cubeLabel != '\0')
            {
                return cubeLabel;
            }
            row--;
        }

        return ' ';
    }

    private static string FormatFrontViewLine(char[] visibleFrontCubes, int height, int layerSize)
    {
        var sb = new StringBuilder();
        var leftPadding = height - layerSize;
        var i = 0;

        while (i < leftPadding)
        {
            sb.Append(' ');
            i++;
        }

        var j = 0;
        while (j < layerSize)
        {
            if (j > 0)
            {
                sb.Append(' ');
            }

            sb.Append(visibleFrontCubes[j]);
            j++;
        }

        return sb.ToString().TrimEnd();
    }
}