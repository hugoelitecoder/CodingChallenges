using System;
using System.Globalization;

class Solution
{
    static void Main(string[] args)
    {
        string input = Console.ReadLine().Trim();
        Console.WriteLine(ConvertToTrueTime(input));
    }

    static string ConvertToTrueTime(string observedTime)
    {
        DateTime obs = DateTime.ParseExact(observedTime, "h:mm:ss tt", CultureInfo.InvariantCulture);
        TimeSpan obsTime = obs.TimeOfDay;
        TimeSpan reset = TimeSpan.FromHours(8);
        TimeSpan day = TimeSpan.FromDays(1);

        TimeSpan relObs = obsTime >= reset
            ? obsTime - reset
            : obsTime + day - reset;

        long trueTicks = relObs.Ticks * 240L / 239L;
        TimeSpan total = reset + TimeSpan.FromTicks(trueTicks);
        TimeSpan trueTimeOfDay = TimeSpan.FromTicks(total.Ticks % day.Ticks);

        DateTime result = DateTime.Today.Date + trueTimeOfDay;
        return result.ToString("h:mm:ss tt", CultureInfo.InvariantCulture);
    }
}