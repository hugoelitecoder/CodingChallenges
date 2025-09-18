using System;
using System.Collections.Generic;
using System.Globalization;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
        if (n == 0)
        {
            Console.WriteLine("NO GAME");
            return;
        }
        var times = new HashSet<int>();
        for (int i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split(':');
            int m = int.Parse(parts[0], CultureInfo.InvariantCulture);
            int s = int.Parse(parts[1], CultureInfo.InvariantCulture);
            times.Add(m * 60 + s);
        }
        int inClash = 0;
        double stopTime = 0.0;
        int curTime = 300;
        for (int t = 300; t >= 0; t--)
        {
            if (times.Contains(t))
            {
                stopTime = t - 256.0 / Math.Pow(2, inClash);
                if (stopTime < 0) stopTime = 0;
                inClash++;
            }
            if (Math.Abs(t - stopTime) < 1e-9 || inClash == 7)
            {
                curTime = t;
                break;
            }
        }
        int mm = curTime / 60;
        int ss = curTime % 60;
        Console.WriteLine($"{mm}:{ss:D2}");
    }
}
