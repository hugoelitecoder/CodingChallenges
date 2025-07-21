using System;
using System.Globalization;

class Solution
{
    static void Main()
    {
        // Read input
        var wh = Console.ReadLine()!.Split();
        int width  = int.Parse(wh[0], CultureInfo.InvariantCulture);
        int height = int.Parse(wh[1], CultureInfo.InvariantCulture);
        int steps  = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

        var a = Console.ReadLine()!.Split();
        var b = Console.ReadLine()!.Split();
        var c = Console.ReadLine()!.Split();
        var d = Console.ReadLine()!.Split();

        double ax = double.Parse(a[0], CultureInfo.InvariantCulture),
               ay = double.Parse(a[1], CultureInfo.InvariantCulture);
        double bx = double.Parse(b[0], CultureInfo.InvariantCulture),
               by = double.Parse(b[1], CultureInfo.InvariantCulture);
        double cx = double.Parse(c[0], CultureInfo.InvariantCulture),
               cy = double.Parse(c[1], CultureInfo.InvariantCulture);
        double dx = double.Parse(d[0], CultureInfo.InvariantCulture),
               dy = double.Parse(d[1], CultureInfo.InvariantCulture);

        // Prepare canvas
        var canvas = new char[height][];
        for (int y = 0; y < height; y++)
        {
            canvas[y] = new char[width];
            for (int x = 0; x < width; x++)
                canvas[y][x] = (x == 0 ? '.' : ' ');
        }

        // Linear interpolation helper
        static double Lerp(double p, double q, double t) => p * (1 - t) + q * t;

        // Plot the Bezier curve
        for (int i = 0; i < steps; i++)
        {
            double t = i / (double)(steps - 1);

            // level 1
            double abx = Lerp(ax, bx, t), aby = Lerp(ay, by, t);
            double bcx = Lerp(bx, cx, t), bcy = Lerp(by, cy, t);
            double cdx = Lerp(cx, dx, t), cdy = Lerp(cy, dy, t);

            // level 2
            double abc_x = Lerp(abx, bcx, t), abc_y = Lerp(aby, bcy, t);
            double bcd_x = Lerp(bcx, cdx, t), bcd_y = Lerp(bcy, cdy, t);

            // level 3 (curve point)
            double fx = Lerp(abc_x, bcd_x, t);
            double fy = Lerp(abc_y, bcd_y, t);

            // round .5 always up
            int ix = (int)(fx + 0.5);
            int iy = (int)(fy + 0.5);

            // mark if within bounds
            if (ix >= 0 && ix < width && iy >= 0 && iy < height)
                canvas[iy][ix] = '#';
        }

        // Output from top row down to bottom row
        for (int y = height - 1; y >= 0; y--)
        {
            // trim trailing spaces
            var line = new string(canvas[y]).TrimEnd();
            Console.WriteLine(line);
        }
    }
}
