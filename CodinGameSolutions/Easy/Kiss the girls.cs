using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

class Solution
{
    static void Main()
    {
        var dims = Console.ReadLine().Split(' ');
        int H = int.Parse(dims[0]);
        int W = int.Parse(dims[1]);

        var risks = new List<double>();
        for (int y = 0; y < H; y++)
        {
            var row = Console.ReadLine();
            for (int x = 0; x < W; x++)
            {
                if (row[x] == 'G')
                {
                    double numerator = Math.Min(x, y);
                    double denom = x * x + y * y + 1;
                    risks.Add(numerator / denom);
                }
            }
        }

        risks.Sort();
        double noCatchProb = 1.0;
        const double threshold = 0.6;
        int count = 0;
        foreach (var p in risks)
        {
            noCatchProb *= (1.0 - p);
            if (noCatchProb < threshold)
                break;
            count++;
        }

        Console.WriteLine(count);
    }
}
