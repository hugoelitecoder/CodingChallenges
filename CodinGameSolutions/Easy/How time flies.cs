using System;
using System.Globalization;

class Solution
{
    static void Main()
    {
        var b = Parse(Console.ReadLine());
        var e = Parse(Console.ReadLine());
        int totalDays = (e - b).Days;

        int years = e.Year - b.Year;
        if (e.Month < b.Month || (e.Month == b.Month && e.Day < b.Day))
            years--;
        var by = b.AddYears(years);

        int months = e.Month - by.Month;
        if (e.Day < by.Day)
            months--;
        if (months < 0)
            months += 12;

        var parts = new System.Collections.Generic.List<string>();
        if (years > 0)
            parts.Add($"{years} year{(years > 1 ? "s" : "")}");
        if (months > 0)
            parts.Add($"{months} month{(months > 1 ? "s" : "")}");
        parts.Add($"total {totalDays} days");

        Console.WriteLine(string.Join(", ", parts));
    }

    static DateTime Parse(string s)
    {
        var p = s.Split('.');
        int d = int.Parse(p[0], CultureInfo.InvariantCulture);
        int m = int.Parse(p[1], CultureInfo.InvariantCulture);
        int y = int.Parse(p[2], CultureInfo.InvariantCulture);
        return new DateTime(y, m, d);
    }
}
