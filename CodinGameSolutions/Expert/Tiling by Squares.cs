using System;
using System.Linq;

public class Solution
{
    public static void Main(string[] args)
    {
        var line = Console.ReadLine();
        var parts = line.Split(' ');
        var w = int.Parse(parts[0]);
        var h = int.Parse(parts[1]);
        var solver = new TilingSolver();
        var result = solver.Solve(w, h);
        Console.WriteLine(result);
    }
}

public class TilingSolver
{
    private int _w;
    private int _h;
    private bool[,] _filled;
    private int _best;

    public int Solve(int w, int h)
    {
        if (w == 0 || h == 0)
        {
            return 0;
        }
        if (w > h)
        {
            var temp = w;
            w = h;
            h = temp;
        }
        _w = w;
        _h = h;
        _filled = new bool[w, h];
        _best = w * h;
        Backtrack(0);
        return this._best;
    }

    private void Backtrack(int currentCount)
    {
        if (currentCount >= _best)
        {
            return;
        }
        var (nextX, nextY) = FindFirstEmpty();
        if (nextX == -1)
        {
            _best = currentCount;
            return;
        }
        var lowerBound = CalculateLowerBound();
        if (currentCount + lowerBound >= _best)
        {
            return;
        }
        var maxSize = CalculateMaxSize(nextX, nextY);
        for (var size = maxSize; size >= 1; size--)
        {
            PlaceTile(nextX, nextY, size);
            Backtrack(currentCount + 1);
            UnplaceTile(nextX, nextY, size);
        }
    }

    private (int, int) FindFirstEmpty()
    {
        for (var y = 0; y < _h; y++)
        {
            for (var x = 0; x < _w; x++)
            {
                if (!_filled[x, y])
                {
                    return (x, y);
                }
            }
        }
        return (-1, -1);
    }

    private int CalculateLowerBound()
    {
        var corners = 0;
        for (var y = 0; y < _h; y++)
        {
            for (var x = 0; x < _w; x++)
            {
                if (!_filled[x, y])
                {
                    var isTop = (y == 0) || _filled[x, y - 1];
                    var isLeft = (x == 0) || _filled[x - 1, y];
                    if (isTop && isLeft)
                    {
                        corners++;
                    }
                }
            }
        }
        return corners;
    }

    private int CalculateMaxSize(int startX, int startY)
    {
        var size = 1;
        while (true)
        {
            var endX = startX + size;
            var endY = startY + size;
            if (endX >= _w || endY >= _h)
            {
                return size;
            }
            for (var y = startY; y <= endY; y++)
            {
                if (_filled[endX, y])
                {
                    return size;
                }
            }
            for (var x = startX; x < endX; x++)
            {
                if (_filled[x, endY])
                {
                    return size;
                }
            }
            size++;
        }
    }

    private void PlaceTile(int startX, int startY, int size)
    {
        for (var y = startY; y < startY + size; y++)
        {
            for (var x = startX; x < startX + size; x++)
            {
                _filled[x, y] = true;
            }
        }
    }

    private void UnplaceTile(int startX, int startY, int size)
    {
        for (var y = startY; y < startY + size; y++)
        {
            for (var x = startX; x < startX + size; x++)
            {
                _filled[x, y] = false;
            }
        }
    }
}