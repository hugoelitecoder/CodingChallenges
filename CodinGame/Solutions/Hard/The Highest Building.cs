using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    public static void Main(string[] args)
    {
        var (W, H) = ReadDimensions();
        var grid = ReadGrid(H);
        var buildings = BuildingExtractor.ExtractBuildings(grid, W, H);
        var tallest = BuildingExtractor.GetTallestBuildings(buildings);
        for (var i = 0; i < tallest.Count; ++i)
        {
            foreach (var row in tallest[i].ToAsciiRows())
                Console.WriteLine(row);
            if (i != tallest.Count - 1) Console.WriteLine();
        }
    }

    static (int, int) ReadDimensions()
    {
        var parts = Console.ReadLine().Split(' ');
        return (int.Parse(parts[0]), int.Parse(parts[1]));
    }

    static string[] ReadGrid(int H)
    {
        var grid = new string[H];
        for (var i = 0; i < H; ++i) grid[i] = Console.ReadLine();
        return grid;
    }
}

class Building
{
    public int Top { get; }
    public int Bottom { get; }
    public int Left { get; }
    public int Right { get; }
    private HashSet<(int, int)> Parts { get; }
    private int Height => Bottom - Top + 1;

    public Building(HashSet<(int, int)> cells)
    {
        Parts = cells;
        Top = cells.Min(p => p.Item1);
        Bottom = cells.Max(p => p.Item1);
        Left = cells.Min(p => p.Item2);
        Right = cells.Max(p => p.Item2);
    }

    public int GetHeight() => Height;

    public string[] ToAsciiRows()
    {
        var rows = new string[Height];
        for (var r = 0; r < Height; ++r)
        {
            var rowChars = new char[Right - Left + 1];
            for (var c = 0; c < rowChars.Length; ++c)
                rowChars[c] = Parts.Contains((Top + r, Left + c)) ? '#' : ' ';
            rows[r] = new string(rowChars).TrimEnd();
        }
        return rows;
    }
}

static class BuildingExtractor
{
    public static List<Building> ExtractBuildings(string[] grid, int W, int H)
    {
        var buildings = new List<Building>();
        var visited = new bool[H, W];
        for (var col = 0; col < W; ++col)
        {
            for (var row = 0; row < H; ++row)
            {
                if (grid[row][col] == '#' && !visited[row, col])
                {
                    var cells = new HashSet<(int, int)>();
                    DFS(grid, visited, row, col, H, W, cells);
                    buildings.Add(new Building(cells));
                }
            }
        }
        return OrderByLeftmost(buildings);
    }

    private static void DFS(string[] grid, bool[,] visited, int r, int c, int H, int W, HashSet<(int, int)> cells)
    {
        var stack = new Stack<(int, int)>();
        stack.Push((r, c));
        while (stack.Count > 0)
        {
            var (row, col) = stack.Pop();
            if (row < 0 || row >= H || col < 0 || col >= W) continue;
            if (visited[row, col]) continue;
            if (grid[row][col] != '#') continue;
            visited[row, col] = true;
            cells.Add((row, col));
            stack.Push((row - 1, col));
            stack.Push((row + 1, col));
            stack.Push((row, col - 1));
            stack.Push((row, col + 1));
        }
    }

    public static List<Building> GetTallestBuildings(List<Building> buildings)
    {
        var maxHeight = buildings.Max(b => b.GetHeight());
        return buildings.Where(b => b.GetHeight() == maxHeight).ToList();
    }

    private static List<Building> OrderByLeftmost(List<Building> buildings)
    {
        return buildings.OrderBy(b => b.Left).ToList();
    }
}
