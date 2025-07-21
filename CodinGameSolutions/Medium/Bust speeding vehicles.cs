using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int speedLimit = int.Parse(Console.ReadLine());
        int n = int.Parse(Console.ReadLine());
        var lastSeen = new Dictionary<string, (int distance, long timestamp)>();
        var outputs = new List<string>();

        for (int i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            string plate     = parts[0];
            int    distance  = int.Parse(parts[1]);
            long   timestamp = long.Parse(parts[2]);

            if (lastSeen.TryGetValue(plate, out var prev))
            {
                int  deltaKm  = distance - prev.distance;
                long deltaSec = timestamp - prev.timestamp;
                if (deltaSec > 0)
                {
                    double avgSpeed = deltaKm * 3600.0 / deltaSec;
                    if (avgSpeed > speedLimit)
                        outputs.Add($"{plate} {distance}");
                }
            }
            lastSeen[plate] = (distance, timestamp);
        }

        if (outputs.Count == 0)
        {
            Console.WriteLine("OK");
        }
        else
        {
            Console.WriteLine(string.Join(Environment.NewLine, outputs));
        }
    }
}
