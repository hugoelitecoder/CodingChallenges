using System;
class Solution
{
    public static void Main(string[] args)
    {
        var rawtime = Console.ReadLine();
        var timePart = rawtime.Substring(0, 8);
        var tzPart = rawtime.Substring(9);
        var parts = timePart.Split(':');
        var hh = int.Parse(parts[0]);
        var mm = int.Parse(parts[1]);
        var ss = int.Parse(parts[2]);
        var sign = tzPart[3] == '+' ? 1 : -1;
        var offsetHour = int.Parse(tzPart.Substring(4, 2));
        var offsetMin = int.Parse(tzPart.Substring(7, 2));
        var diffMin = 60 - sign * (offsetHour * 60 + offsetMin);
        var originalSec = hh * 3600 + mm * 60 + ss;
        var resultSec = originalSec + diffMin * 60;
        resultSec %= 86400;
        if (resultSec < 0) resultSec += 86400;
        var beats = (decimal)resultSec / 86.4m;
        var rounded = Math.Round(beats, 2, MidpointRounding.AwayFromZero);
        var output = rounded.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        Console.WriteLine($"@{output}");
    }
}
