using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

class Solution
{
    static void Main(string[] args)
    {
        var gymnasts = Console.ReadLine()
            .Split(',')
            .Select(s => s.Trim())
            .ToList();
        var categories = Console.ReadLine()
            .Split(',')
            .Select(s => s.Trim().ToLowerInvariant())
            .ToList();

        var best = new Dictionary<string, Dictionary<string, double>>(StringComparer.OrdinalIgnoreCase);
        foreach (var g in gymnasts)
        {
            best[g] = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["bars"] = double.MinValue,
                ["beam"] = double.MinValue,
                ["floor"] = double.MinValue
            };
        }

        var n = int.Parse(Console.ReadLine());
        for (int i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split(',');
            var name = parts[0].Trim();
            if (!best.ContainsKey(name)) continue;

            var bars  = double.Parse(parts[1], CultureInfo.InvariantCulture);
            var beam  = double.Parse(parts[2], CultureInfo.InvariantCulture);
            var floor = double.Parse(parts[3], CultureInfo.InvariantCulture);

            if (bars  > best[name]["bars"])  best[name]["bars"]  = bars;
            if (beam  > best[name]["beam"])  best[name]["beam"]  = beam;
            if (floor > best[name]["floor"]) best[name]["floor"] = floor;
        }

        foreach (var g in gymnasts)
        {
            var scores = categories
                .Select(cat => best[g][cat])
                .Select(FormatScore);
            Console.WriteLine(string.Join(",", scores));
        }
    }

    static string FormatScore(double d)
    {
        return d.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
