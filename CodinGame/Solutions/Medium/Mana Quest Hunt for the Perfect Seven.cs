using System;
using System.Globalization;

class Solution
{
    public static void Main(string[] args)
    {
        var c = int.Parse(Console.ReadLine());
        var categories = new string[c];
        var counts = new int[c];
        var i = 0;

        while (i < c)
        {
            var parts = Console.ReadLine().Split(' ');
            categories[i] = parts[0];
            counts[i] = int.Parse(parts[1]);
            i++;
        }

        var q = int.Parse(Console.ReadLine());
        var patterns = new string[q];
        i = 0;

        while (i < q)
        {
            patterns[i] = Console.ReadLine();
            i++;
        }

        var probability = MagicOpeningHandProbabilitySolver.Solve(categories, counts, patterns);

        Console.Error.WriteLine("[DEBUG] Categories=" + c);
        Console.Error.WriteLine("[DEBUG] Patterns=" + q);
        Console.Error.WriteLine("[DEBUG] Probability=" + probability.ToString("F10", CultureInfo.InvariantCulture));

        Console.WriteLine(probability.ToString("F4", CultureInfo.InvariantCulture));
    }
}

static class MagicOpeningHandProbabilitySolver
{
    public static double Solve(string[] categories, int[] counts, string[] patterns)
    {
        var q = patterns.Length;
        var subsetCount = 1 << q;
        var matchedCounts = new int[subsetCount];
        var i = 0;

        while (i < categories.Length)
        {
            var mask = BuildPatternMask(categories[i], patterns);
            matchedCounts[mask] += counts[i];
            i++;
        }

        var unionCounts = BuildUnionCounts(matchedCounts, subsetCount);
        var totalHands = Combination(60, 7);
        var goodHands = 0L;
        var subset = 0;

        while (subset < subsetCount)
        {
            var available = 60 - unionCounts[subset];
            var ways = Combination(available, 7);

            if ((BitCount(subset) & 1) == 0)
            {
                goodHands += ways;
            }
            else
            {
                goodHands -= ways;
            }

            subset++;
        }

        return (double)goodHands / totalHands;
    }

    private static int BuildPatternMask(string category, string[] patterns)
    {
        var mask = 0;
        var i = 0;

        while (i < patterns.Length)
        {
            if (Matches(category, patterns[i]))
            {
                mask |= 1 << i;
            }

            i++;
        }

        return mask;
    }

    private static bool Matches(string category, string pattern)
    {
        return (pattern[0] == 'x' || pattern[0] == category[0]) &&
               (pattern[1] == 'x' || pattern[1] == category[1]) &&
               (pattern[2] == 'x' || pattern[2] == category[2]);
    }

    private static int[] BuildUnionCounts(int[] matchedCounts, int subsetCount)
    {
        var unionCounts = new int[subsetCount];
        var mask = 0;

        while (mask < subsetCount)
        {
            var total = 0;
            var matchedMask = 0;

            while (matchedMask < subsetCount)
            {
                if ((matchedMask & mask) != 0)
                {
                    total += matchedCounts[matchedMask];
                }

                matchedMask++;
            }

            unionCounts[mask] = total;
            mask++;
        }

        return unionCounts;
    }

    private static int BitCount(int value)
    {
        var count = 0;

        while (value != 0)
        {
            value &= value - 1;
            count++;
        }

        return count;
    }

    private static long Combination(int n, int k)
    {
        if (k < 0 || k > n)
        {
            return 0;
        }

        if (k > n - k)
        {
            k = n - k;
        }

        var result = 1L;
        var i = 1;

        while (i <= k)
        {
            result = result * (n - k + i) / i;
            i++;
        }

        return result;
    }
}