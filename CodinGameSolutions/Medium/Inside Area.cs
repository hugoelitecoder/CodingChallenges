using System;
using System.Globalization;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        long x = 0, y = 0;
        var xs = new long[n+1];
        var ys = new long[n+1];
        xs[0] = 0; ys[0] = 0;

        long perimeter = 0;
        for(int i = 1; i <= n; i++)
        {
            var parts = Console.ReadLine().Split();
            char d = parts[0][0];
            long step = long.Parse(parts[1], CultureInfo.InvariantCulture);
            perimeter += step;

            switch(d)
            {
                case '>': x += step; break;
                case '<': x -= step; break;
                case '^': y += step; break;
                case 'v': y -= step; break;
            }

            xs[i] = x;
            ys[i] = y;
        }

        long twiceArea = 0;
        for(int i = 0; i < n; i++)
        {
            twiceArea += xs[i] * ys[i+1] - xs[i+1] * ys[i];
        }
        twiceArea = Math.Abs(twiceArea);

        long cells = (twiceArea >> 1) + (perimeter >> 1) + 1;
        Console.WriteLine(cells);
    }
}
