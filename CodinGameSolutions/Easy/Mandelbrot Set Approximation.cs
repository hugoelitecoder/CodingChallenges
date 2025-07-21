using System;

class Solution
{
    public static void Main()
    {
        var n = int.Parse(Console.ReadLine());
        var cols = (3 * (n - 1)) / 2 + 1;
        var step = 2.0 / (n - 1);
        for (var i = 0; i < n; i++)
        {
            var y = 1.0 - i * step;
            var line = new char[cols];
            for (var j = 0; j < cols; j++)
            {
                var x = -2.0 + j * step;
                line[j] = IsInSet(x, y) ? '*' : ' ';
            }
            Console.WriteLine(line);
        }
    }

    private static bool IsInSet(double cx, double cy)
    {
        var zr = 0.0; 
        var zi = 0.0;
        for (var k = 0; k < 10; k++)
        {
            var zr2 = zr * zr - zi * zi + cx;
            var zi2 = 2 * zr * zi + cy;
            zr = zr2; 
            zi = zi2;
        }
        return zr * zr + zi * zi <= 1.0;
    }
}
