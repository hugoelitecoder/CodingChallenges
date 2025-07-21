using System;
using System.Collections.Generic;
using System.Globalization;

class Solution
{
    const double R = 6371.0;
    static void Main()
    {
        int N = int.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
        int M = int.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
        var lats = new double[N];
        var lons = new double[N];
        for (int i = 0; i < N; i++)
        {
            var t = Console.ReadLine().Split(' ');
            lats[i] = ParseLat(t[1]);
            lons[i] = ParseLon(t[2]);
        }
        var msgs = new string[N];
        for (int i = 0; i < N; i++)
            msgs[i] = Console.ReadLine();
        for (int i = 0; i < M; i++)
        {
            var t = Console.ReadLine().Split(' ');
            double lat = ParseLat(t[0]), lon = ParseLon(t[1]);
            int best = int.MaxValue;
            var sel = new List<string>();
            for (int j = 0; j < N; j++)
            {
                double ca = Math.Sin(lat)*Math.Sin(lats[j])
                          + Math.Cos(lat)*Math.Cos(lats[j])
                          * Math.Cos(lon - lons[j]);
                if (ca > 1) ca = 1;
                if (ca < -1) ca = -1;
                double d = R * Math.Acos(ca);
                int rd = (int)Math.Round(d, 0, MidpointRounding.AwayFromZero);
                if (rd < best)
                {
                    best = rd;
                    sel.Clear();
                    sel.Add(msgs[j]);
                }
                else if (rd == best)
                    sel.Add(msgs[j]);
            }
            Console.WriteLine(string.Join(" ", sel));
        }
    }

    static double ParseLat(string s)
    {
        double d = int.Parse(s.Substring(1,2),CultureInfo.InvariantCulture);
        double m = int.Parse(s.Substring(3,2),CultureInfo.InvariantCulture);
        double sec = int.Parse(s.Substring(5,2),CultureInfo.InvariantCulture);
        double deg = d + m/60.0 + sec/3600.0;
        if (s[0]=='S') deg = -deg;
        return deg * Math.PI/180.0;
    }
    static double ParseLon(string s)
    {
        double d = int.Parse(s.Substring(1,3),CultureInfo.InvariantCulture);
        double m = int.Parse(s.Substring(4,2),CultureInfo.InvariantCulture);
        double sec = int.Parse(s.Substring(6,2),CultureInfo.InvariantCulture);
        double deg = d + m/60.0 + sec/3600.0;
        if (s[0]=='W') deg = -deg;
        return deg * Math.PI/180.0;
    }
}
