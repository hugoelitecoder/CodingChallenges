using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int M = int.Parse(Console.ReadLine());
        var allMeasures = new List<int>(M * 15);

        for (int i = 0; i < M; i++)
        {
            var parts = Console.ReadLine()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();
            allMeasures.AddRange(parts);
        }

        var minuteEstimates = new List<double>(M);
        for (int i = 0; i < M; i++)
        {
            int sum60 = 0;
            for (int j = 0; j < 15; j++)
                sum60 += allMeasures[i * 15 + j];
            double tc = 10.0 + (sum60 - 40) / 7.0;
            minuteEstimates.Add(tc);
        }
        double avg60 = minuteEstimates.Average();
        Console.WriteLine(avg60.ToString("0.0", CultureInfo.InvariantCulture));

        if (avg60 >= 5.0 && avg60 <= 30.0)
        {
            int total = allMeasures.Count;
            if (total % 2 == 1)
                total--;
            int pairs = total / 2;
            var eightEstimates = new List<double>(pairs);
            for (int k = 0; k < pairs; k++)
            {
                int n8 = allMeasures[2 * k] + allMeasures[2 * k + 1];
                eightEstimates.Add(n8 + 5.0);
            }
            double avg8 = eightEstimates.Average();
            Console.WriteLine(avg8.ToString("0.0", CultureInfo.InvariantCulture));
        }
    }
}
