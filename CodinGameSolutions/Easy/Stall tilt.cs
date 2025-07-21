using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        int v = int.Parse(Console.ReadLine());

        var competitors = new List<Participant>();
        for (int i = 0; i < n; i++)
        {
            double speed = double.Parse(Console.ReadLine(), System.Globalization.CultureInfo.InvariantCulture);
            competitors.Add(new Participant
            {
                Id = ((char)('a' + i)).ToString(),
                Speed = speed
            });
        }

        var radii = new double[v];
        for (int i = 0; i < v; i++)
            radii[i] = double.Parse(Console.ReadLine(), System.Globalization.CultureInfo.InvariantCulture);

        const double g = 9.81;
        double tan60 = Math.Sqrt(3.0);

        var thresholds = radii
            .Select(r => Math.Sqrt(r * g * tan60))
            .ToArray();

        double minThreshold = thresholds.Min();
        int ourSpeed = (int)Math.Floor(minThreshold);

        var all = new List<Participant>(competitors);
        all.Add(new Participant { Id = "y", Speed = ourSpeed });

        foreach (var p in all)
        {
            p.StallAt = v + 1;
            for (int bend = 0; bend < v; bend++)
            {
                if (p.Speed > thresholds[bend])
                {
                    p.StallAt = bend + 1;
                    break;
                }
            }
        }

        var ranking = all
            .OrderByDescending(p => p.StallAt)
            .ThenByDescending(p => p.Speed)
            .Select(p => p.Id)
            .ToList();

        Console.WriteLine(ourSpeed);
        foreach (var id in ranking)
            Console.WriteLine(id);
    }

    class Participant
    {
        public string Id;
        public double Speed;
        public int StallAt;
    }
}
