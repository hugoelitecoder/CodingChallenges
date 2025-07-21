using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var dims = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int W = dims[0], H = dims[1];
        int X = int.Parse(Console.ReadLine());

        var mat = new Matrix(H, W);
        for (int i = 0; i < H; i++)
            mat.SetRow(i, Console.ReadLine().Split().Select(int.Parse));

        var result = mat.Crank(X);

        for (int i = 0; i < result.H; i++)
            Console.WriteLine(result.GetRowString(i));
    }
}

class Point2D : IEquatable<Point2D>
{
    public int Y { get; }
    public int X { get; }
    public Point2D(int y, int x) { Y = y; X = x; }
    public bool Equals(Point2D other) => other != null && Y == other.Y && X == other.X;
    public override bool Equals(object obj) => Equals(obj as Point2D);
    public override int GetHashCode() => Y * 101 + X;
}

class Matrix
{
    public int H { get; }
    public int W { get; }
    private int[,] grid;

    public Matrix(int h, int w)
    {
        H = h; W = w;
        grid = new int[H, W];
    }

    public void SetRow(int row, IEnumerable<int> values)
    {
        int col = 0;
        foreach (var v in values)
            grid[row, col++] = v;
    }

    public int Get(Point2D p) => grid[p.Y, p.X];
    public void Set(Point2D p, int v) => grid[p.Y, p.X] = v;

    public string GetRowString(int row)
    {
        var vals = new string[W];
        for (int j = 0; j < W; j++)
            vals[j] = grid[row, j].ToString();
        return string.Join(" ", vals);
    }

    public Matrix Clone()
    {
        var m = new Matrix(H, W);
        for (int i = 0; i < H; i++)
            for (int j = 0; j < W; j++)
                m.grid[i, j] = grid[i, j];
        return m;
    }

    public Matrix Crank(int X)
    {
        var result = Clone();
        int numLayers = Math.Min(H, W) / 2;

        for (int layer = 0; layer < numLayers; layer++)
        {
            var indices = GetLayerIndices(H, W, layer);
            int len = indices.Count;
            int shift = X % len;
            for (int idx = 0; idx < len; idx++)
            {
                var src = indices[(idx + shift) % len];
                var dst = indices[idx];
                result.Set(dst, Get(src));
            }
        }

        if (Math.Min(H, W) % 2 == 1)
        {
            int layersDone = numLayers;
            if (H <= W)
            {
                int cy = H / 2;
                for (int x = layersDone; x < W - layersDone; x++)
                    result.Set(new Point2D(cy, x), Get(new Point2D(cy, x)));
            }
            else
            {
                int cx = W / 2;
                for (int y = layersDone; y < H - layersDone; y++)
                    result.Set(new Point2D(y, cx), Get(new Point2D(y, cx)));
            }
        }

        return result;
    }

    private static List<Point2D> GetLayerIndices(int H, int W, int layer)
    {
        var indices = new List<Point2D>();
        int maxY = H - layer - 1, maxX = W - layer - 1, minY = layer, minX = layer;
        for (int x = minX; x < maxX; x++)
            indices.Add(new Point2D(minY, x));
        for (int y = minY; y < maxY; y++)
            indices.Add(new Point2D(y, maxX));
        for (int x = maxX; x > minX; x--)
            indices.Add(new Point2D(maxY, x));
        for (int y = maxY; y > minY; y--)
            indices.Add(new Point2D(y, minX));
        return indices;
    }
}
