using System;
using System.Collections.Generic;
using System.Linq;

class Player
{
    public readonly record struct Point(int X, int Y)
    {
        public static readonly Point None = new(-1, -1);
        public override string ToString() => $"{X} {Y}";
    }

    static void Main()
    {
        int width  = int.Parse(Console.ReadLine()!);
        int height = int.Parse(Console.ReadLine()!);

        var points = new List<Point>(width * height / 4);
        for (int y = 0; y < height; y++)
        {
            var line = Console.ReadLine()!;
            for (int x = 0; x < width; x++)
                if (line[x] == '0')
                    points.Add(new Point(x, y));
        }

        var rightMap = new Dictionary<Point, Point>(points.Count);
        foreach (var row in points.GroupBy(p => p.Y))
        {
            var sorted = row.OrderBy(p => p.X).ToArray();
            for (int i = 0; i < sorted.Length; i++)
                rightMap[sorted[i]] = (i + 1 < sorted.Length) 
                                      ? sorted[i + 1] 
                                      : Point.None;
        }

        var downMap = new Dictionary<Point, Point>(points.Count);
        foreach (var col in points.GroupBy(p => p.X))
        {
            var sorted = col.OrderBy(p => p.Y).ToArray();
            for (int i = 0; i < sorted.Length; i++)
                downMap[sorted[i]] = (i + 1 < sorted.Length) 
                                     ? sorted[i + 1] 
                                     : Point.None;
        }

        foreach (var p in points)
        {
            var r = rightMap[p];
            var d = downMap[p];
            Console.WriteLine($"{p.X} {p.Y} {r.X} {r.Y} {d.X} {d.Y}");
        }
    }
}
