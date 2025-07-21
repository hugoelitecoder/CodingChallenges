using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        double rAlice = 0, mAlice = 0;
        var planets = new (string name, double r, double m, double c)[N-1];
        int idx = 0;

        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            string name = parts[0];
            double r = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
            double m = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
            double c = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);

            if (name == "Alice")
            {
                rAlice = r;
                mAlice = m;
            }
            else
            {
                planets[idx++] = (name, r, m, c);
            }
        }

        double volumeAlice = (4.0/3.0) * Math.PI * Math.Pow(rAlice, 3);
        double dAlice = mAlice / volumeAlice;
        string bestName = null;
        double bestDist = double.MaxValue;

        foreach (var p in planets)
        {
            double volumeP = (4.0/3.0) * Math.PI * Math.Pow(p.r, 3);
            double dP = p.m / volumeP;
            double roche = rAlice * Math.Pow(2.0 * dAlice / dP, 1.0/3.0);
            if (p.c > roche && p.c < bestDist)
            {
                bestDist = p.c;
                bestName = p.name;
            }
        }

        Console.WriteLine(bestName);
    }
}
