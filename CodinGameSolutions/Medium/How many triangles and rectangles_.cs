using System;
using System.Linq;
using System.Collections.Generic;

class Player
{
    static void Main()
    {
        var dims = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int height = dims[0], width = dims[1];
        var grid = new List<List<char>>(height);
        for (int i = 0; i < height; i++)
            grid.Add(Console.ReadLine().ToList());

        var (triangleCount, rectangleCount) = ImageProcessor.Calculate(grid);

        Console.WriteLine(triangleCount);
        Console.WriteLine(rectangleCount);
    }
}

public static class ImageProcessor
{
    public static (int triangleCount, int rectangleCount) Calculate(List<List<char>> grid)
    {
        var vertices = FindVertices(grid);

        var rowGroup   = new Dictionary<(int, int), int>();
        var colGroup   = new Dictionary<(int, int), int>();
        var diag1Group = new Dictionary<(int, int), int>();
        var diag2Group = new Dictionary<(int, int), int>();

        ProcessGroups(grid, rowGroup,   '-', GetRowLines);
        ProcessGroups(grid, colGroup,   '|', GetColLines);
        ProcessGroups(grid, diag1Group, '\\', GetDiag1Lines);
        ProcessGroups(grid, diag2Group, '/',  GetDiag2Lines);

        int rectangleCount = CountRectangles(grid, vertices, rowGroup, colGroup);
        int triangleCount  = CountTriangles(grid, vertices, rowGroup, colGroup, diag1Group, diag2Group);

        return (triangleCount, rectangleCount);
    }

    static List<(int, int)> FindVertices(List<List<char>> grid)
    {
        var vertices = new List<(int, int)>();
        for (int r = 0; r < grid.Count; r++)
            for (int c = 0; c < grid[0].Count; c++)
                if (grid[r][c] == '+')
                    vertices.Add((r, c));
        return vertices;
    }

    static void ProcessGroups(
        List<List<char>> grid,
        Dictionary<(int, int), int> groupDict,
        char segmentChar,
        Func<List<List<char>>, IEnumerable<IEnumerable<(int r, int c)>>> lineProvider)
    {
        int group = 0;
        foreach (var line in lineProvider(grid))
        {
            bool inSeg = false;
            group++;
            foreach (var (r, c) in line)
            {
                char ch = grid[r][c];
                if (ch == '+')
                {
                    inSeg = true;
                    groupDict[(r, c)] = group;
                }
                else if (ch != segmentChar)
                {
                    if (inSeg) group++;
                    inSeg = false;
                }
            }
        }
    }

    static IEnumerable<IEnumerable<(int r, int c)>> GetRowLines(List<List<char>> grid)
    {
        int height = grid.Count, width = grid[0].Count;
        for (int r = 0; r < height; r++)
            yield return Enumerable.Range(0, width).Select(c => (r, c));
    }

    static IEnumerable<IEnumerable<(int r, int c)>> GetColLines(List<List<char>> grid)
    {
        int height = grid.Count, width = grid[0].Count;
        for (int c = 0; c < width; c++)
            yield return Enumerable.Range(0, height).Select(r => (r, c));
    }

    static IEnumerable<IEnumerable<(int r, int c)>> GetDiag1Lines(List<List<char>> grid)
    {
        int h = grid.Count, w = grid[0].Count;
        for (int sc = 0; sc < w; sc++)
            yield return Diagonal((0, sc), (1, 1), grid);
        for (int sr = 1; sr < h; sr++)
            yield return Diagonal((sr, 0), (1, 1), grid);
    }

    static IEnumerable<IEnumerable<(int r, int c)>> GetDiag2Lines(List<List<char>> grid)
    {
        int h = grid.Count, w = grid[0].Count;
        for (int sc = 0; sc < w; sc++)
            yield return Diagonal((0, sc), (1, -1), grid);
        for (int sr = 1; sr < h; sr++)
            yield return Diagonal((sr, w - 1), (1, -1), grid);
    }

    static IEnumerable<(int r, int c)> Diagonal((int r, int c) start, (int dr, int dc) delta, List<List<char>> grid)
    {
        int r = start.r, c = start.c, h = grid.Count, w = grid[0].Count;
        while (r >= 0 && r < h && c >= 0 && c < w)
        {
            yield return (r, c);
            r += delta.dr; c += delta.dc;
        }
    }

    static int CountRectangles(
        List<List<char>> grid,
        List<(int r, int c)> vertices,
        Dictionary<(int, int), int> rowGroup,
        Dictionary<(int, int), int> colGroup)
    {
        int cnt = 0, n = vertices.Count;
        for (int a = 0; a < n; a++)
        {
            var (r1, c1) = vertices[a];
            for (int b = a + 1; b < n; b++)
            {
                var (r2, c2) = vertices[b];
                if (r1 < r2 && c1 < c2
                    && grid[r1][c2] == '+' && grid[r2][c1] == '+'
                    && rowGroup[(r1, c1)] == rowGroup[(r1, c2)]
                    && rowGroup[(r2, c1)] == rowGroup[(r2, c2)]
                    && colGroup[(r1, c1)] == colGroup[(r2, c1)]
                    && colGroup[(r1, c2)] == colGroup[(r2, c2)])
                {
                    cnt++;
                }
            }
        }
        return cnt;
    }

    static int CountTriangles(
        List<List<char>> grid,
        List<(int r, int c)> vertices,
        Dictionary<(int, int), int> rowGroup,
        Dictionary<(int, int), int> colGroup,
        Dictionary<(int, int), int> diag1Group,
        Dictionary<(int, int), int> diag2Group)
    {
        int n = vertices.Count, total = 0;
        for (int i = 0; i < n; i++)
        {
            var (r1, c1) = vertices[i];
            for (int j = i + 1; j < n; j++)
            {
                var (r2, c2) = vertices[j];
                if (r1 == r2 && c1 < c2 && rowGroup[(r1, c1)] == rowGroup[(r2, c2)])
                {
                    if ((c1 + c2) % 2 == 0)
                    {
                        int cm = (c1 + c2) / 2;
                        int rm = r1 + (c2 - c1) / 2;
                        if (0 <= rm && rm < grid.Count && grid[rm][cm] == '+'
                            && diag1Group[(r1, c1)] == diag1Group[(rm, cm)]
                            && diag2Group[(r2, c2)] == diag2Group[(rm, cm)])
                            total++;
                        rm = r1 - (c2 - c1) / 2;
                        if (0 <= rm && rm < grid.Count && grid[rm][cm] == '+'
                            && diag2Group[(r1, c1)] == diag2Group[(rm, cm)]
                            && diag1Group[(r2, c2)] == diag1Group[(rm, cm)])
                            total++;
                    }
                    int c3 = c1;
                    int r3 = r1 + (c2 - c1);
                    if (0 <= r3 && r3 < grid.Count && grid[r3][c3] == '+'
                        && colGroup[(r1, c1)] == colGroup[(r3, c3)]
                        && diag2Group[(r2, c2)] == diag2Group[(r3, c3)])
                        total++;
                    r3 = r1 - (c2 - c1);
                    if (0 <= r3 && r3 < grid.Count && grid[r3][c3] == '+'
                        && colGroup[(r1, c1)] == colGroup[(r3, c3)]
                        && diag1Group[(r2, c2)] == diag1Group[(r3, c3)])
                        total++;
                    c3 = c2;
                    int r33 = r2 + (c2 - c1);
                    if (0 <= r33 && r33 < grid.Count && grid[r33][c3] == '+'
                        && colGroup[(r2, c2)] == colGroup[(r33, c3)]
                        && diag1Group[(r1, c1)] == diag1Group[(r33, c3)])
                        total++;
                    r33 = r2 - (c2 - c1);
                    if (0 <= r33 && r33 < grid.Count && grid[r33][c3] == '+'
                        && colGroup[(r2, c2)] == colGroup[(r33, c3)]
                        && diag2Group[(r1, c1)] == diag2Group[(r33, c3)])
                        total++;
                }
                else if (c1 == c2 && (r1 + r2) % 2 == 0 && colGroup[(r1, c1)] == colGroup[(r2, c2)])
                {
                    int rm = (r1 + r2) / 2;
                    int cm = c1 + (r2 - r1) / 2;
                    if (0 <= cm && cm < grid[0].Count && grid[rm][cm] == '+'
                        && diag1Group[(r1, c1)] == diag1Group[(rm, cm)]
                        && diag2Group[(r2, c2)] == diag2Group[(rm, cm)])
                        total++;
                    cm = c1 - (r2 - r1) / 2;
                    if (0 <= cm && cm < grid[0].Count && grid[rm][cm] == '+'
                        && diag2Group[(r1, c1)] == diag2Group[(rm, cm)]
                        && diag1Group[(r2, c2)] == diag1Group[(rm, cm)])
                        total++;
                }
            }
        }
        return total;
    }
}