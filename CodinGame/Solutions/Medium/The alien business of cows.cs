using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;

class Solution
{
    // Constants
    const double ALIEN_STOP       = 500.0;   // m
    const double ALT_LAUNCH       =  46.0;   // m
    const double ALT_ROCKET       = 160.0;   // km
    const double DIST_EQUATORIAL  = 111.11;  // km per degree
    const double SPEED_CAPTURE    =   9.81;  // m/s
    const double SPEED_ROCKET     =   6.0;   // km/s
    const double PI               = 3.14159265358979;

    // Collector struct
    struct Collector
    {
        public string Name;
        public int    Capacity;
        public int    MinThreshold;
        public double Speed;      // km/s
        public double Efficiency; // unitless

        public Collector(string name, int cap, int minT, double sp, double ef)
        {
            Name         = name;
            Capacity     = cap;
            MinThreshold = minT;
            Speed        = sp;
            Efficiency   = ef;
        }
    }

    static readonly Collector[] colls = new[]
    {
        new Collector("VaCoWM Cleaner",  3,  0, 44.7 , 0.85),
        new Collector("L4nd MoWer",     10,  6, 22.38, 1.2 ),
        new Collector("Cow Harvester",  20, 14, 11.19, 1.5 )
    };

    // helper: degrees+minutes/60+seconds/3600
    static double Deci(double deg, double min, double sec)
        => deg + min/60.0 + sec/3600.0;

    // 3D distance
    static double Dist(double x, double y, double z)
        => Math.Sqrt(x*x + y*y + z*z);

    static void Main()
    {
        int N = int.Parse(Console.ReadLine()!);
        var rx = new Regex(
            @"^(.*) " +                     // group1 = location
            @"(\d*)°(\d*)'([\d.]*)""N " +   // g2=gdeg, g3=gmin, g4=gsec
            @"(\d*)°(\d*)'([\d.]*)""W " +   // g5=ldeg, g6=lmin, g7=lsec
            @"(\d*)$"                       // g8 = elevation in meters
        );

        while (N-- > 0)
        {
            string line = Console.ReadLine()!;
            var m = rx.Match(line);
            if (!m.Success) continue;

            string location = m.Groups[1].Value;

            // Launch coordinates
            double launchLat = Deci(34, 45, 21.8);
            double launchLon = Deci(120, 37, 34.8);

            // Parsed coordinates
            double lat = Deci(
                double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture),
                double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture),
                double.Parse(m.Groups[4].Value, CultureInfo.InvariantCulture)
            );
            double lon = Deci(
                double.Parse(m.Groups[5].Value, CultureInfo.InvariantCulture),
                double.Parse(m.Groups[6].Value, CultureInfo.InvariantCulture),
                double.Parse(m.Groups[7].Value, CultureInfo.InvariantCulture)
            );
            int elevation = int.Parse(m.Groups[8].Value, CultureInfo.InvariantCulture);

            // Horizontal deltas (km)
            double dLat = (launchLat - lat) * DIST_EQUATORIAL;
            double avgLatRad = (launchLat + lat) * 0.5 * Math.PI / 180.0;
            double dLon = (launchLon - lon) * DIST_EQUATORIAL * Math.Cos(avgLatRad);

            // Vertical leg (km)
            double vert = ALT_ROCKET - (ALT_LAUNCH / 1000.0);

            // Total 3D distance (km)
            double distance = Dist(dLat, dLon, vert);

            // Time for missile to intercept (s)
            double missileTime = distance / SPEED_ROCKET;

            int bestCows = 0;
            string bestColl = null!;

            // Try each collector
            foreach (var c in colls)
            {
                // Time to return from hover point:
                double hoverKm = (ALT_LAUNCH + elevation) / 1000.0;
                double returnLeg = (ALT_ROCKET - hoverKm) / c.Speed;

                // Time per cow (s)
                double abductionTime = ALIEN_STOP / (SPEED_CAPTURE * c.Efficiency);

                // Available abduction time
                double available = missileTime - returnLeg;

                int cows = (int)(available / abductionTime); // truncates toward zero

                if (cows >= c.MinThreshold)
                {
                    cows = Math.Min(cows, c.Capacity);
                    if (cows > bestCows)
                    {
                        bestCows = cows;
                        bestColl = c.Name;
                    }
                }
            }

            // Output
            if (bestCows > 0)
            {
                Console.WriteLine(
                    $"{location}: possible. Send a {bestColl} to bring back {bestCows} cow{(bestCows > 1 ? "s" : "")}."
                );
            }
            else
            {
                Console.WriteLine($"{location}: impossible.");
            }
        }
    }
}
