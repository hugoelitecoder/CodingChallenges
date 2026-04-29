using System;
using System.Diagnostics;

class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var line = Console.ReadLine();
        var parts = line.Split(' ');
        var m = long.Parse(parts[0]);
        var n = long.Parse(parts[1]);
        var b = long.Parse(parts[2]);
        var x = long.Parse(parts[3]);
        Console.Error.WriteLine("[DEBUG] Input m=" + m + " n=" + n + " b=" + b + " x=" + x);
        var solver = new LexicographicBaseOrderFinder(m, n, b);
        var ans = solver.FindKth(x);
        sw.Stop();
        Console.Error.WriteLine("[DEBUG] Answer=" + ans);
        Console.Error.WriteLine("[DEBUG] ElapsedMs=" + sw.ElapsedMilliseconds);
        Console.WriteLine(ans);
    }
}

class LexicographicBaseOrderFinder
{
    private readonly long _m;
    private readonly long _n;
    private readonly long _b;

    public LexicographicBaseOrderFinder(long m, long n, long b)
    {
        _m = m;
        _n = n;
        _b = b;
    }

    public long FindKth(long k)
    {
        var prefix = 0L;
        var foundRoot = false;
        for (var d = 1L; d < _b; d++)
        {
            var cnt = CountPrefix(d);
            if (cnt == 0) continue;
            if (k > cnt)
            {
                k -= cnt;
                continue;
            }
            prefix = d;
            foundRoot = true;
            break;
        }
        if (!foundRoot) return -1;
        while (true)
        {
            if (prefix >= _m && prefix <= _n)
            {
                if (k == 1) return prefix;
                k--;
            }
            var nextFound = false;
            var childBase = prefix * _b;
            for (var d = 0L; d < _b; d++)
            {
                var child = childBase + d;
                var cnt = CountPrefix(child);
                if (cnt == 0) continue;
                if (k > cnt)
                {
                    k -= cnt;
                    continue;
                }
                prefix = child;
                nextFound = true;
                break;
            }
            if (!nextFound) return -1;
        }
    }

    private long CountPrefix(long prefix)
    {
        if (prefix > _n) return 0;
        var count = 0L;
        var low = prefix;
        var high = prefix;
        while (low <= _n)
        {
            var left = low > _m ? low : _m;
            var right = high < _n ? high : _n;
            if (left <= right) count += right - left + 1;
            if (low > _n / _b) break;
            low *= _b;
            var limit = (long.MaxValue - (_b - 1)) / _b;
            if (high > limit) high = long.MaxValue;
            else high = high * _b + (_b - 1);
        }
        return count;
    }
}