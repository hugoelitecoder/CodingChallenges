using System;

class Solution
{
    public static void Main()
    {
        var parts = Console.ReadLine().Split(' ');
        var inputSpeed = int.Parse(parts[0]);
        var dist1 = parts[1];
        var time1 = parts[3];
        var dist2 = parts[6];
        var time2 = parts[8];
        var distMap = new System.Collections.Generic.Dictionary<string,double> {
            {"miles",63360}, {"furlongs",7920}, {"chains",792},
            {"yards",36}, {"feet",12}, {"inches",1}
        };
        var timeMap = new System.Collections.Generic.Dictionary<string,double> {
            {"fortnight",1209600}, {"week",604800}, {"day",86400},
            {"hour",3600}, {"minute",60}, {"second",1}
        };
        var baseRate = inputSpeed * distMap[dist1] / timeMap[time1];
        var converted = baseRate * timeMap[time2] / distMap[dist2];
        var rounded = Math.Round(converted,1,MidpointRounding.AwayFromZero);
        Console.WriteLine(rounded.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)
            + " " + dist2 + " per " + time2);
    }
}
