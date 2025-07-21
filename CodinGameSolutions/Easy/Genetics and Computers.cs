using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var parts = Console.ReadLine().Split(' ');
        var p1 = parts[0];
        var p2 = parts[1];
        var requests = Console.ReadLine().Split(':');

        List<string> Gametes(string parent)
        {
            var g1 = new[] { parent[0].ToString(), parent[1].ToString() };
            var g2 = new[] { parent[2].ToString(), parent[3].ToString() };
            var gametes = new List<string>(4);
            foreach (var a in g1)
                foreach (var b in g2)
                    gametes.Add(a + b);
            return gametes;
        }

        var gam1 = Gametes(p1);
        var gam2 = Gametes(p2);

        var counts = new Dictionary<string, int>();
        foreach (var ga in gam1)
            foreach (var gb in gam2)
            {
                string locus1 = SortAlleles(ga[0], gb[0]);
                string locus2 = SortAlleles(ga[1], gb[1]);
                var genotype = locus1 + locus2;
                counts[genotype] = counts.GetValueOrDefault(genotype) + 1;
            }

        var result = requests
            .Select(req => counts.GetValueOrDefault(req, 0))
            .ToArray();

        int g = result.Aggregate(0, Gcd);
        if (g == 0) g = 1;
        var simplified = result.Select(c => c / g);

        Console.WriteLine(string.Join(":", simplified));
    }

    static string SortAlleles(char a, char b)
    {
        return (a <= b)
            ? $"{a}{b}"
            : $"{b}{a}";
    }

    static int Gcd(int x, int y)
    {
        if (x == 0) return y;
        if (y == 0) return x;
        x = Math.Abs(x);
        y = Math.Abs(y);
        while (y != 0)
        {
            int t = x % y;
            x = y;
            y = t;
        }
        return x;
    }
}
