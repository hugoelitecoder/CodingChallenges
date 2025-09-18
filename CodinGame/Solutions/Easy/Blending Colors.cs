using System;
using System.Linq;

class Solution
{
    public static void Main()
    {
        var sp = Console.ReadLine().Split(' ');
        var S = int.Parse(sp[0]);
        var P = int.Parse(sp[1]);
        var shapes = new (string type, int x0, int y0, int len, int r, int g, int b)[S];
        for (var i = 0; i < S; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            shapes[i] = (parts[0], int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]), int.Parse(parts[6]));
        }
        for (var pi = 0; pi < P; pi++)
        {
            var pt = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
            var x = pt[0];
            var y = pt[1];
            var layers = shapes.Where(sh => !IsBorder(sh, x, y) && IsInside(sh, x, y)).ToList();
            if (shapes.Any(sh => IsBorder(sh, x, y)))
            {
                Console.WriteLine("(0, 0, 0)");
                continue;
            }
            if (!layers.Any())
            {
                Console.WriteLine("(255, 255, 255)");
                continue;
            }
            var cnt = layers.Count;
            var sumR = layers.Sum(sh => sh.r);
            var sumG = layers.Sum(sh => sh.g);
            var sumB = layers.Sum(sh => sh.b);
            var rr = (int)Math.Round(sumR / (double)cnt, MidpointRounding.AwayFromZero);
            var gg = (int)Math.Round(sumG / (double)cnt, MidpointRounding.AwayFromZero);
            var bb = (int)Math.Round(sumB / (double)cnt, MidpointRounding.AwayFromZero);
            Console.WriteLine($"({rr}, {gg}, {bb})");
        }
    }
    private static bool IsInside((string type, int x0, int y0, int len, int r, int g, int b) sh, int x, int y)
    {
        if (sh.type == "SQUARE")
            return x > sh.x0 && x < sh.x0 + sh.len && y > sh.y0 && y < sh.y0 + sh.len;
        var dx = x - sh.x0;
        var dy = y - sh.y0;
        return dx * dx + dy * dy < sh.len * sh.len;
    }
    private static bool IsBorder((string type, int x0, int y0, int len, int r, int g, int b) sh, int x, int y)
    {
        if (sh.type == "SQUARE")
            return x >= sh.x0 && x <= sh.x0 + sh.len && y >= sh.y0 && y <= sh.y0 + sh.len
                && (x == sh.x0 || x == sh.x0 + sh.len || y == sh.y0 || y == sh.y0 + sh.len);
        var dx = x - sh.x0;
        var dy = y - sh.y0;
        return dx * dx + dy * dy == sh.len * sh.len;
    }
}