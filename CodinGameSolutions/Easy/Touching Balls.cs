using System;
using System.Collections.Generic;
using System.Globalization;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var X = new double[N];
        var Y = new double[N];
        var Z = new double[N];
        var R = new double[N];

        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            X[i] = double.Parse(parts[0], CultureInfo.InvariantCulture);
            Y[i] = double.Parse(parts[1], CultureInfo.InvariantCulture);
            Z[i] = double.Parse(parts[2], CultureInfo.InvariantCulture);
            R[i] = double.Parse(parts[3], CultureInfo.InvariantCulture);
        }

        double result = 0.0;

        for (int i = 0; i < N; i++)
        {
            double dbest = 1e6;
            for (int j = 0; j < N; j++)
            {
                if (i == j) continue;
                double dx = X[i] - X[j];
                double dy = Y[i] - Y[j];
                double dz = Z[i] - Z[j];
                double dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                double d = dist - R[i] - R[j];
                if (d < dbest)
                    dbest = d;
            }

            R[i] += dbest;
            result += Math.Pow(R[i], 3);
        }

        Console.WriteLine(Math.Round(result));
    }
}
