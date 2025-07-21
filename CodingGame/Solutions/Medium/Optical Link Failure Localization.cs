using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        var nums = Enumerable.Range(0, 3)
                             .Select(_ => int.Parse(Console.ReadLine()))
                             .ToArray();
        int links    = nums[0];
        int mtrails  = nums[1];
        int failures = nums[2];

        int observedMask = Console.ReadLine()
            .Select((c, idx) => c == '1' ? 1 << idx : 0)
            .Sum();

        var linkMasks = Enumerable.Range(0, links)
            .Select(_ => Console.ReadLine()
                                .Select((c, idx) => c == '1' ? 1 << idx : 0)
                                .Sum())
            .ToArray();

        var singles = failures >= 1
            ? Enumerable.Range(0, links)
                        .Where(i => linkMasks[i] == observedMask)
                        .Select(i => new[] { i })
            : Enumerable.Empty<int[]>();

        var duals = failures >= 2
            ? from i in Enumerable.Range(0, links)
              from j in Enumerable.Range(i + 1, links - i - 1)
              where (linkMasks[i] | linkMasks[j]) == observedMask
              select new[] { i, j }
            : Enumerable.Empty<int[]>();

        var candidates = singles.Concat(duals).ToList();

        Console.WriteLine(
            candidates.Count == 1
                ? string.Join(" ", candidates[0])
                : "AMBIGUOUS"
        );
    }
}
