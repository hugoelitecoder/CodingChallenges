using System;
using System.Collections.Generic;
using System.Globalization;

class Solution
{
    public static void Main()
    {
        var expr = Console.ReadLine().Trim();
        var parts = expr.Split('+', StringSplitOptions.TrimEntries);
        var v1u1 = ParseValueUnit(parts[0]);
        var v2u2 = ParseValueUnit(parts[1]);

        var factors = new Dictionary<string, decimal>
        {
            ["um"] = 1m,
            ["mm"] = 1000m,
            ["cm"] = 10000m,
            ["dm"] = 100000m,
            ["m" ] = 1000000m,
            ["km"] = 1000000000m
        };

        var f1 = factors[v1u1.unit];
        var f2 = factors[v2u2.unit];
        var smallUnit = f1 < f2 ? v1u1.unit : v2u2.unit;
        var smallFactor = Math.Min(f1, f2);

        decimal sumUm = v1u1.value * f1 + v2u2.value * f2;
        var result = sumUm / smallFactor;
        var s = result.ToString("0.#####", CultureInfo.InvariantCulture);

        Console.WriteLine($"{s}{smallUnit}");
    }

    private static (decimal value, string unit) ParseValueUnit(string vu)
    {
        int idx = 0;
        while (idx < vu.Length && (char.IsDigit(vu[idx]) || vu[idx]=='.'))
            idx++;
        var num = decimal.Parse(vu.Substring(0, idx), CultureInfo.InvariantCulture);
        var unit = vu.Substring(idx);
        return (num, unit);
    }
}
