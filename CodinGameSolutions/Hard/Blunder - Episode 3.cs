using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var nums = new List<double>();
        var times = new List<double>();
        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split();
            nums.Add(double.Parse(parts[0]));
            times.Add(double.Parse(parts[1]));
        }

        var models = new List<(string Label, Func<double, double> F)>
        {
            ("O(1)",             n => 1),
            ("O(log n)",         n => Math.Log(n)),
            ("O(n)",             n => n),
            ("O(n log n)",       n => n * Math.Log(n)),
            ("O(n^2)",           n => n * n),
            ("O(n^2 log n)",     n => n * n * Math.Log(n)),
            ("O(n^3)",           n => n * n * n),
            ("O(2^n)",           n => Math.Pow(2, n > 30 ? 30 : n)) // Clamp for overflow
        };

        int best = -1;
        double minError = double.MaxValue;

        for (int i = 0; i < models.Count; i++)
        {
            var (label, f) = models[i];
            var fs = nums.Select(f).ToArray();
            var ts = times.ToArray();
            double sumF2 = fs.Select(x => x * x).Sum();
            double sumFT = fs.Zip(ts, (x, y) => x * y).Sum();
            double alpha = sumF2 == 0 ? 0 : sumFT / sumF2;
            var predicted = fs.Select(x => alpha * x).ToArray();
            double error = ts.Zip(predicted, (y, yhat) => (y - yhat) * (y - yhat)).Average();

            if (error < minError)
            {
                minError = error;
                best = i;
            }
        }
        Console.WriteLine(models[best].Label);
    }
}
