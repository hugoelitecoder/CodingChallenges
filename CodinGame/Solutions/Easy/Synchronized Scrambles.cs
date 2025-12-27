using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var inputs = Console.ReadLine().Split(' ');
        var offset1 = inputs[0];
        var offset2 = inputs[1];

        Console.Error.WriteLine($"[DEBUG] Offset 1: {offset1}");
        Console.Error.WriteLine($"[DEBUG] Offset 2: {offset2}");

        var finder = new TimeAnagramFinder();
        var results = finder.Solve(offset1, offset2);
        
        foreach (var line in results)
        {
            Console.WriteLine(line);
        }

        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] Found {results.Count} anagram pairs.");
        Console.Error.WriteLine($"[DEBUG] Execution Time: {sw.ElapsedMilliseconds}ms");
    }
}

public class TimeAnagramFinder
{
    private int[] _counts = new int[10];

    public SortedSet<string> Solve(string offset1Str, string offset2Str)
    {
        var results = new SortedSet<string>();
        var offset1Mins = ParseOffset(offset1Str);
        var offset2Mins = ParseOffset(offset2Str);
        var totalMinsInDay = 1440;

        for (var utcMinute = 0; utcMinute < totalMinsInDay; utcMinute++)
        {
            var local1Mins = (utcMinute + offset1Mins % totalMinsInDay + totalMinsInDay) % totalMinsInDay;
            var local2Mins = (utcMinute + offset2Mins % totalMinsInDay + totalMinsInDay) % totalMinsInDay;

            var time1Str = FormatTime(local1Mins);
            var time2Str = FormatTime(local2Mins);

            if (AreAnagrams(time1Str, time2Str))
            {
                results.Add($"{time1Str}, {time2Str}");
            }
        }
        return results;
    }

    private int ParseOffset(string s)
    {
        var sign = (s[0] == '-') ? -1 : 1;
        var hh = (s[1] - '0') * 10 + (s[2] - '0');
        var mm = (s[3] - '0') * 10 + (s[4] - '0');
        return sign * (hh * 60 + mm);
    }

    private string FormatTime(int totalMinutes)
    {
        var hh = totalMinutes / 60;
        var mm = totalMinutes % 60;
        return $"{hh:D2}{mm:D2}";
    }

    private bool AreAnagrams(string s1, string s2)
    {
        Array.Clear(_counts, 0, 10);
        _counts[s1[0] - '0']++;
        _counts[s1[1] - '0']++;
        _counts[s1[2] - '0']++;
        _counts[s1[3] - '0']++;

        _counts[s2[0] - '0']--;
        _counts[s2[1] - '0']--;
        _counts[s2[2] - '0']--;
        _counts[s2[3] - '0']--;

        for (var i = 0; i < 10; i++)
        {
            if (_counts[i] != 0) return false;
        }
        return true;
    }
}

