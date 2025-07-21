using System;
using System.Globalization;

class Solution
{
    static void Main()
    {
        var parts = Console.ReadLine()!.Split();
        var bottomRadius = double.Parse(parts[0], CultureInfo.InvariantCulture);
        var topRadius    = double.Parse(parts[1], CultureInfo.InvariantCulture);
        var glassHeight  = double.Parse(parts[2], CultureInfo.InvariantCulture);
        var beerVol      = double.Parse(parts[3], CultureInfo.InvariantCulture);

        var dr = (topRadius - bottomRadius) / glassHeight;

        double FrustumVol(double h)
        {
            var r = bottomRadius + dr * h;
            return Math.PI * h * (bottomRadius * bottomRadius + bottomRadius * r + r * r) / 3.0;
        }

        var low = 0.0;
        var high = glassHeight;
        for (var i = 0; i < 100; i++)
        {
            var mid = (low + high) / 2.0;
            if (FrustumVol(mid) < beerVol) low = mid;
            else high = mid;
        }

        var beerHeight = Math.Round((low + high) / 2.0, 1);
        Console.WriteLine(beerHeight.ToString("0.0", CultureInfo.InvariantCulture));
    }
}
