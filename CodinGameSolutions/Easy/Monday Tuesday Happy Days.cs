using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        bool leap = Console.ReadLine()?.Trim() == "1";
        var monthDays = new Dictionary<string, int> {
            ["Jan"] = 31,
            ["Feb"] = leap ? 29 : 28,
            ["Mar"] = 31,
            ["Apr"] = 30,
            ["May"] = 31,
            ["Jun"] = 30,
            ["Jul"] = 31,
            ["Aug"] = 31,
            ["Sep"] = 30,
            ["Oct"] = 31,
            ["Nov"] = 30,
            ["Dec"] = 31
        };
        var weekdays = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        var parts = Console.ReadLine()?.Split(' ');
        string srcWeekday = parts[0];
        string srcMonth = parts[1];
        int srcDay = int.Parse(parts[2]);
        parts = Console.ReadLine()?.Split(' ');
        string tgtMonth = parts[0];
        int tgtDay = int.Parse(parts[1]);

        int DayOfYear(string month, int day)
        {
            int doy = 0;
            foreach (var m in monthDays.Keys)
            {
                if (m == month) break;
                doy += monthDays[m];
            }
            return doy + day;
        }

        int srcDOY = DayOfYear(srcMonth, srcDay);
        int tgtDOY = DayOfYear(tgtMonth, tgtDay);

        int srcIdx = Array.IndexOf(weekdays, srcWeekday);
        int delta = tgtDOY - srcDOY;
        int tgtIdx = (srcIdx + delta % 7 + 7) % 7;

        Console.WriteLine(weekdays[tgtIdx]);
    }
}