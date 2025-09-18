using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine().Trim());
        var scans = new Dictionary<string, Dictionary<string, long>>();
        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string plate = parts[0];
            string radar = parts[1];
            long ts = long.Parse(parts[2]);

            if (!scans.ContainsKey(plate))
                scans[plate] = new Dictionary<string, long>();
            scans[plate][radar] = ts;
        }

        const long distanceKm = 13;
        const long factor = distanceKm * 3600000L;  
        var offenders = new List<(string plate, long speed)>();

        foreach (var kv in scans)
        {
            var plate = kv.Key;
            var dict = kv.Value;
            if (dict.TryGetValue("A21-42", out long t1) &&
                dict.TryGetValue("A21-55", out long t2))
            {
                long delta = Math.Abs(t2 - t1); 
                if (delta == 0) continue;
                long speed = factor / delta;

                if (speed > 130)
                    offenders.Add((plate, speed));
            }
        }

        offenders.Sort((a, b) => StringComparer.Ordinal.Compare(a.plate, b.plate));

        foreach (var (plate, speed) in offenders)
            Console.WriteLine($"{plate} {speed}");
    }
}
