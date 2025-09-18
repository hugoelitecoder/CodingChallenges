using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        int roadLength = int.Parse(Console.ReadLine());
        int zoneQuantity = int.Parse(Console.ReadLine());
        var zones = new (int km, int speed)[zoneQuantity];
        for (int i = 0; i < zoneQuantity; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            zones[i] = (int.Parse(parts[0]), int.Parse(parts[1]));
        }
        Array.Sort(zones, (a, b) => a.km.CompareTo(b.km));
        const double normalSpeed = 130.0;
        double timeH = 0.0;
        int prevKm = 0;
        double currSpeed = normalSpeed;
        foreach (var (km, speed) in zones)
        {
            int segment = km - prevKm;
            if (segment > 0)
                timeH += segment / currSpeed;
            prevKm = km;
            currSpeed = speed;
        }
        int lastSeg = roadLength - prevKm;
        if (lastSeg > 0)
            timeH += lastSeg / currSpeed;

        double theoreticalH = roadLength / normalSpeed;
        double diffMin = (timeH - theoreticalH) * 60.0;
        int lost = (int)Math.Floor(diffMin + 0.5);

        Console.WriteLine(lost);
    }
}
