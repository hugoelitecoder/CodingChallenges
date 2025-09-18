using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        int h = int.Parse(Console.ReadLine());
        int w = int.Parse(Console.ReadLine());
        int d = Console.ReadLine() == "RIGHT" ? 1 : -1;

        var grid = new string[h];
        for (int i = 0; i < h; i++)
        {
            grid[i] = Console.ReadLine();
        }

        var stick = FindStick(grid);
        var left = new List<Point2D>();
        var right = new List<Point2D>();
        var trash = new List<Point2D>();

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                char c = grid[y][x];
                if (x == stick.x || c == '.')
                    continue;

                if (c == '<')
                    left.Add(new Point2D(x, y));
                else if (c == '>')
                    right.Add(new Point2D(x, y));
                else
                    trash.Add(new Point2D(x, y));
            }
        }

        Console.WriteLine(Simulate(stick, d, left, right, trash, w));
    }

    class Point2D
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point2D(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    static (int x, int h) FindStick(string[] grid)
    {
        int x = grid[0].IndexOf('|');
        if (x < 0)
            x = grid[0].IndexOf('C');

        int h = 1;
        while (h < grid.Length && grid[h - 1][x] == '|')
            h++;

        return (x, h);
    }

    static int Simulate(
        (int x, int h) stick,
        int d,
        List<Point2D> left,
        List<Point2D> right,
        List<Point2D> trash,
        int width)
    {
        int caught = 0;
        int maxLoops = Math.Max(width - (stick.x + 1), stick.x - 1);

        for (int loop = 0; loop < maxLoops; loop++)
        {
            MoveFish(right, 1, stick, ref caught);
            MoveFish(left, -1, stick, ref caught);
            FishCollision(left, right);
            if (TrashStep(trash, d, stick, left, right))
                break;
        }

        return caught;
    }

    static void MoveFish(
        List<Point2D> fish,
        int dx,
        (int x, int h) stick,
        ref int caught)
    {
        for (int i = fish.Count - 1; i >= 0; i--)
        {
            var p = fish[i];
            p.X += dx;

            if (p.X == stick.x && p.Y < stick.h)
            {
                caught++;
                fish.RemoveAt(i);
            }
        }
    }

    static void FishCollision(List<Point2D> left, List<Point2D> right)
    {
        for (int i = left.Count - 1; i >= 0; i--)
        {
            for (int j = right.Count - 1; j >= 0; j--)
            {
                var a = left[i];
                var b = right[j];
                bool sameCell = a.X == b.X && a.Y == b.Y;
                bool crossing = a.Y == b.Y && a.X - b.X == -1;

                if (sameCell || crossing)
                {
                    left.RemoveAt(i);
                    right.RemoveAt(j);
                    break;
                }
            }
        }
    }

    static bool TrashStep(
        List<Point2D> trash,
        int d,
        (int x, int h) stick,
        List<Point2D> left,
        List<Point2D> right)
    {
        for (int i = trash.Count - 1; i >= 0; i--)
        {
            var g = trash[i];
            g.X += d;

            if (g.X == stick.x && g.Y < stick.h)
                return true;

            bool removed = false;
            for (int j = right.Count - 1; j >= 0; j--)
            {
                var f = right[j];
                if (g.Y == f.Y && g.X - f.X == -1)
                {
                    right.RemoveAt(j);
                    trash.RemoveAt(i);
                    removed = true;
                    break;
                }
            }

            if (removed)
                continue;

            for (int j = left.Count - 1; j >= 0; j--)
            {
                var f = left[j];
                if (g.Y == f.Y && g.X - f.X == 1)
                {
                    left.RemoveAt(j);
                    trash.RemoveAt(i);
                    break;
                }
            }
        }

        return false;
    }
}
