using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var timeBits = Console.ReadLine().Trim();
        var bin = new string(timeBits.Select(c => c == '#' ? '1' : '0').ToArray());
        var tval = Convert.ToInt32(bin, 2);
        var hh = tval / 100;
        var mm = tval % 100;
        Console.WriteLine($"{hh:D2}:{mm:D2}");

        var monthMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["jan"] = 0,  ["feb"] = 1,  ["mar"] = 2,  ["apr"] = 3,
            ["may"] = 4,  ["jun"] = 5,  ["jul"] = 6,  ["aug"] = 7,
            ["sep"] = 8,  ["oct"] = 9,  ["nov"] = 10, ["dec"] = 11
        };

        var words = Console.ReadLine()
                           .Trim()
                           .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var decoded = words.Select(w =>
        {
            var m1 = w.Substring(0, 3);
            var m2 = w.Substring(3, 3);
            var v1 = monthMap[m1];
            var v2 = monthMap[m2];
            var code = v1 * 12 + v2;
            return (char)code;
        });

        Console.WriteLine(new string(decoded.ToArray()));
    }
}
