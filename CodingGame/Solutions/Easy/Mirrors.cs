using System;
using System.Globalization;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine()!);
        var parts = Console.ReadLine()!.Split();
        double[] r = new double[n + 1];
        for (int i = 1; i <= n; i++)
            r[i] = double.Parse(parts[i - 1], CultureInfo.InvariantCulture);

        double[] R = new double[n + 2];
        R[n] = r[n];
        for (int k = n - 1; k >= 1; k--)
        {
            double tk = 1.0 - r[k];
            double denom = 1.0 - r[k] * R[k + 1];
            double extra = denom != 0.0
                ? (tk * tk * R[k + 1]) / denom
                : 0.0;
            R[k] = r[k] + extra;
        }

        Console.WriteLine(R[1].ToString("F4", CultureInfo.InvariantCulture));
    }
}
